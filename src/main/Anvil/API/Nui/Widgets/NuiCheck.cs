using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiCheck : NuiElement
  {
    public override string Type
    {
      get => "check";
    }

    [JsonProperty("label")]
    public NuiProperty<string> Label { get; set; }

    [JsonProperty("value")]
    public NuiProperty<bool> Value { get; set; }
  }
}
