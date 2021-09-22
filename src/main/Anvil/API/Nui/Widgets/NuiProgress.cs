using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiProgress : NuiElement
  {
    public override string Type
    {
      get => "progress";
    }

    [JsonProperty("value")]
    public NuiProperty<int> Value { get; set; }
  }
}
