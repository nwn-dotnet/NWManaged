using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawListPolyLine : NuiDrawListItem
  {
    public override NuiDrawListItemType Type
    {
      get => NuiDrawListItemType.PolyLine;
    }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("fill")]
    public NuiProperty<bool> Fill { get; set; }

    [JsonProperty("line_thickness")]
    public NuiProperty<float> LineThickness { get; set; }

    [JsonProperty("points")]
    public List<float> Points { get; set; }
  }
}
