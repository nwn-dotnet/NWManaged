using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawListCurve : NuiDrawListItem
  {
    public override NuiDrawListItemType Type
    {
      get => NuiDrawListItemType.Curve;
    }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("line_thickness")]
    public NuiProperty<float> LineThickness { get; set; }

    [JsonProperty("a")]
    public NuiProperty<NuiVector> PointA { get; set; }

    [JsonProperty("b")]
    public NuiProperty<NuiVector> PointB { get; set; }

    [JsonProperty("ctrl0")]
    public NuiProperty<NuiVector> Control0 { get; set; }

    [JsonProperty("ctrl1")]
    public NuiProperty<NuiVector> Control1 { get; set; }
  }
}
