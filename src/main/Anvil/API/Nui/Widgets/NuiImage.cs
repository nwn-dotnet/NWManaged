using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiImage : NuiElement
  {
    public override string Type
    {
      get => "image";
    }

    [JsonProperty("value")]
    public NuiProperty<string> ResRef { get; set; }

    [JsonProperty("image_halign")]
    public NuiProperty<NuiHAlign> HorizontalAlign { get; set; }

    [JsonProperty("image_valign")]
    public NuiProperty<NuiVAlign> VerticalAlign { get; set; }

    [JsonProperty("image_aspect")]
    public NuiProperty<NuiAspect> ImageAspect { get; set; }
  }
}
