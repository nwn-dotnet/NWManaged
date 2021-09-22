using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Anvil.API
{
  // https://stackoverflow.com/questions/39461518/how-to-deserialize-an-array-of-values-with-a-fixed-schema-to-a-strongly-typed-da/39462464#39462464
  internal class ObjectToArrayConverter<T> : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return typeof(T) == objectType;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Type objectType = value.GetType();
      JsonObjectContract contract = serializer.ContractResolver.ResolveContract(objectType) as JsonObjectContract;
      if (contract == null)
      {
        throw new JsonSerializationException(string.Format("invalid type {0}.", objectType.FullName));
      }

      writer.WriteStartArray();
      foreach (JsonProperty property in SerializableProperties(contract))
      {
        object? propertyValue = property.ValueProvider.GetValue(value);
        if (property.Converter != null && property.Converter.CanWrite)
        {
          property.Converter.WriteJson(writer, propertyValue, serializer);
        }
        else
        {
          serializer.Serialize(writer, propertyValue);
        }
      }

      writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      JsonObjectContract contract = serializer.ContractResolver.ResolveContract(objectType) as JsonObjectContract;
      if (contract == null)
      {
        throw new JsonSerializationException(string.Format("invalid type {0}.", objectType.FullName));
      }

      if (reader.MoveToContentAndAssert().TokenType == JsonToken.Null)
      {
        return null;
      }

      if (reader.TokenType != JsonToken.StartArray)
      {
        throw new JsonSerializationException(string.Format("token {0} was not JsonToken.StartArray", reader.TokenType));
      }

      // Not implemented: JsonObjectContract.CreatorParameters, serialization callbacks,
      existingValue = existingValue ?? contract.DefaultCreator();

      using IEnumerator<JsonProperty> enumerator = SerializableProperties(contract).GetEnumerator();

      while (true)
      {
        switch (reader.ReadToContentAndAssert().TokenType)
        {
          case JsonToken.EndArray:
            return existingValue;

          default:
            if (!enumerator.MoveNext())
            {
              reader.Skip();
              break;
            }

            JsonProperty property = enumerator.Current;
            object propertyValue;
            // TODO:
            // https://www.newtonsoft.com/json/help/html/Properties_T_Newtonsoft_Json_Serialization_JsonProperty.htm
            // JsonProperty.ItemConverter, ItemIsReference, ItemReferenceLoopHandling, ItemTypeNameHandling, DefaultValue, DefaultValueHandling, ReferenceLoopHandling, Required, TypeNameHandling, ...
            if (property.Converter != null && property.Converter.CanRead)
            {
              propertyValue = property.Converter.ReadJson(reader, property.PropertyType, property.ValueProvider.GetValue(existingValue), serializer);
            }
            else
            {
              propertyValue = serializer.Deserialize(reader, property.PropertyType);
            }

            property.ValueProvider.SetValue(existingValue, propertyValue);
            break;
        }
      }
    }

    private static IEnumerable<JsonProperty> SerializableProperties(JsonObjectContract contract)
    {
      return contract.Properties.Where(p => !p.Ignored && p.Readable && p.Writable);
    }
  }

  internal static partial class JsonExtensions
  {
    public static JsonReader ReadToContentAndAssert(this JsonReader reader)
    {
      return reader.ReadAndAssert().MoveToContentAndAssert();
    }

    public static JsonReader MoveToContentAndAssert(this JsonReader reader)
    {
      if (reader == null)
      {
        throw new ArgumentNullException();
      }

      if (reader.TokenType == JsonToken.None) // Skip past beginning of stream.
      {
        reader.ReadAndAssert();
      }

      while (reader.TokenType == JsonToken.Comment) // Skip past comments.
      {
        reader.ReadAndAssert();
      }

      return reader;
    }

    public static JsonReader ReadAndAssert(this JsonReader reader)
    {
      if (reader == null)
      {
        throw new ArgumentNullException();
      }

      if (!reader.Read())
      {
        throw new JsonReaderException("Unexpected end of JSON stream.");
      }

      return reader;
    }
  }
}
