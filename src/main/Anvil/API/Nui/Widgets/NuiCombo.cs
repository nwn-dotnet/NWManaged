using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiCombo : NuiElement
  {
    public override string Type
    {
      get => "combo";
    }

    [JsonProperty("elements")]
    public NuiProperty<List<NuiComboEntry>> Entries { get; set; }

    [JsonProperty("value")]
    public NuiProperty<int> Selected { get; set; }
  }
}
