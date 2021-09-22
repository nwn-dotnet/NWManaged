using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiSlider : NuiElement
  {
    public override string Type
    {
      get => "slider";
    }

    [JsonProperty("value")]
    public NuiProperty<int> Value { get; set; }

    [JsonProperty("min")]
    public NuiProperty<int> Min { get; set; }

    [JsonProperty("max")]
    public NuiProperty<int> Max { get; set; }

    [JsonProperty("step")]
    public NuiProperty<int> Step { get; set; }
  }
}
