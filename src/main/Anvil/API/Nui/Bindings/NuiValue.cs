using Newtonsoft.Json;

namespace Anvil.API
{
  /// <summary>
  /// A readonly NUI property value that cannot be changed at runtime.
  /// </summary>
  /// <typeparam name="T">The type of value being assigned.</typeparam>
  [JsonConverter(typeof(NuiValueConverter))]
  public sealed class NuiValue<T> : NuiProperty<T>
  {
    /// <summary>
    /// Gets the value of this property.
    /// </summary>
    public T Value { get; init; }

    public NuiValue(T value)
    {
      Value = value;
    }
  }
}
