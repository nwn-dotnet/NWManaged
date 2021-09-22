using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiText : NuiElement
  {
    public override string Type
    {
      get => "text";
    }

    [JsonProperty("value")]
    public NuiProperty<string> Value { get; set; }
  }
}
