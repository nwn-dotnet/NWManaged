using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiColorPicker : NuiElement
  {
    public override string Type
    {
      get => "color_picker";
    }

    [JsonProperty("value")]
    public NuiProperty<NuiColor> Value { get; set; }
  }
}
