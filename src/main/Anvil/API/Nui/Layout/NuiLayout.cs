using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public abstract class NuiLayout : NuiElement
  {
    [JsonProperty("children")]
    public List<NuiElement> Children { get; set; } = new List<NuiElement>();
  }
}
