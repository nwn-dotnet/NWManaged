using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawListText : NuiDrawListItem
  {
    public override NuiDrawListItemType Type
    {
      get => NuiDrawListItemType.Text;
    }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("rect")]
    public NuiProperty<NuiRect> Rect { get; set; }

    [JsonProperty("text")]
    public NuiProperty<string> Text { get; set; }
  }
}
