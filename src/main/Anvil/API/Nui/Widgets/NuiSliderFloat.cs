using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiSliderFloat : NuiElement
  {
    public override string Type
    {
      get => "sliderf";
    }

    [JsonProperty("value")]
    public NuiProperty<float> Value { get; set; }

    [JsonProperty("min")]
    public NuiProperty<float> Min { get; set; }

    [JsonProperty("max")]
    public NuiProperty<float> Max { get; set; }

    [JsonProperty("step")]
    public NuiProperty<float> Step { get; set; }
  }
}
