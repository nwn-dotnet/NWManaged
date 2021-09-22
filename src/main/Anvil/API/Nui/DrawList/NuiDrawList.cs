using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiDrawList
  {
    [JsonProperty("draw_list_scissor")]
    public NuiProperty<bool> Scissor { get; set; }

    [JsonProperty("draw_list")]
    public List<NuiDrawListItem> DrawList { get; set; }
  }
}
