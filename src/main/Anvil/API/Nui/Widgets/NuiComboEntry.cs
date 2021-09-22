using Newtonsoft.Json;

namespace Anvil.API
{
  [JsonConverter(typeof(ObjectToArrayConverter<NuiComboEntry>))]
  public sealed class NuiComboEntry
  {
    [JsonProperty(Order = 1)]
    public string Label { get; set; }

    [JsonProperty(Order = 2)]
    public int Value { get; set; }

    public NuiComboEntry() {}

    public NuiComboEntry(string label, int value)
    {
      Label = label;
      Value = value;
    }
  }
}
