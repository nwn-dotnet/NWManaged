using Newtonsoft.Json;

namespace Anvil.API
{
  public abstract class NuiElement
  {
    [JsonProperty("type")]
    public abstract string Type { get; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public float? Width { get; set; }

    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public float? Height { get; set; }

    [JsonProperty("aspect", NullValueHandling = NullValueHandling.Ignore)]
    public float? Aspect { get; set; }

    [JsonProperty("margin", NullValueHandling = NullValueHandling.Ignore)]
    public float? Margin { get; set; }

    [JsonProperty("padding", NullValueHandling = NullValueHandling.Ignore)]
    public float? Padding { get; set; }

    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public NuiProperty<bool> Enabled { get; set; }

    [JsonProperty("visible", NullValueHandling = NullValueHandling.Ignore)]
    public NuiProperty<bool> Visible { get; set; }

    [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
    public NuiProperty<string> Tooltip { get; set; }
  }
}
