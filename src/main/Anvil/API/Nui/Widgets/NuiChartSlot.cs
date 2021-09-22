using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiChartSlot
  {
    [JsonProperty("type")]
    public NuiChartType ChartType { get; set; }

    [JsonProperty("legend")]
    public NuiProperty<string> Legend { get; set; }

    [JsonProperty("color")]
    public NuiProperty<NuiColor> Color { get; set; }

    [JsonProperty("data")]
    public NuiProperty<List<float>> Data { get; set; }
  }
}
