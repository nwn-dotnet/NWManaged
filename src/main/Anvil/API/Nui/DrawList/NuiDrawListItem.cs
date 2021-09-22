using Newtonsoft.Json;

namespace Anvil.API
{
  public abstract class NuiDrawListItem
  {
    [JsonProperty("type")]
    public abstract NuiDrawListItemType Type { get; }

    [JsonProperty("enabled")]
    public NuiProperty<bool> Enabled { get; set; }
  }
}
