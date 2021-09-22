using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawListCircle : NuiDrawListItem
  {
    public override NuiDrawListItemType Type
    {
      get => NuiDrawListItemType.Circle;
    }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("fill")]
    public NuiProperty<bool> Fill { get; set; }

    [JsonProperty("line_thickness")]
    public NuiProperty<float> LineThickness { get; set; }

    [JsonProperty("rect")]
    public NuiBind<NuiRect> Rect { get; set; }
  }
}
