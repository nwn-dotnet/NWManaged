using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiButtonImage : NuiElement
  {
    public override string Type
    {
      get => "button_image";
    }

    [JsonProperty("label")]
    public NuiProperty<string> ResRef { get; set; }
  }
}
