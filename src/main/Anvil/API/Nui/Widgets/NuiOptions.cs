using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiOptions : NuiElement
  {
    public override string Type
    {
      get => "options";
    }

    [JsonProperty("value")]
    public NuiProperty<int> Selection { get; set; }

    [JsonProperty("direction")]
    public NuiDirection Direction { get; set; }

    [JsonProperty("elements")]
    public List<string> Elements { get; set; }
  }
}
