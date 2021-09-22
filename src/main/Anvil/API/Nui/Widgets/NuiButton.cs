using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiButton : NuiElement
  {
    public override string Type
    {
      get => "button";
    }

    [JsonProperty("label")]
    public NuiProperty<string> Label { get; set; }
  }
}
