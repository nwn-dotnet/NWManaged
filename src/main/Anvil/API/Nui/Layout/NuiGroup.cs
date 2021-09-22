using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiGroup : NuiLayout
  {
    public override string Type { get; } = "group";

    [JsonProperty("border")]
    public bool Border { get; set; } = true;

    [JsonProperty("scrollbars")]
    public NuiScrollbars Scrollbars { get; set; } = NuiScrollbars.Auto;
  }
}
