using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiLabel : NuiElement
  {
    public override string Type
    {
      get => "label";
    }

    [JsonProperty("value")]
    public NuiProperty<string> Value { get; set; }

    [JsonProperty("text_halign")]
    public NuiProperty<NuiHAlign> HorizontalAlign { get; set; }

    [JsonProperty("text_valign")]
    public NuiProperty<NuiVAlign> VerticalAlign { get; set; }

    [JsonProperty("text_color")]
    public NuiProperty<NuiColor> TextColor { get; set; }
  }
}
