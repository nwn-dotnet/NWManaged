using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawListArc : NuiDrawListItem
  {
    public override NuiDrawListItemType Type
    {
      get => NuiDrawListItemType.Arc;
    }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("fill")]
    public NuiProperty<bool> Fill { get; set; }

    [JsonProperty("line_thickness")]
    public NuiProperty<float> LineThickness { get; set; }

    [JsonProperty("c")]
    public NuiProperty<NuiVector> Center { get; set; }

    [JsonProperty("radius")]
    public NuiProperty<float> Radius { get; set; }

    [JsonProperty("amin")]
    public NuiProperty<float> AngleMin { get; set; }

    [JsonProperty("amax")]
    public NuiProperty<float> AngleMax { get; set; }
  }
}
