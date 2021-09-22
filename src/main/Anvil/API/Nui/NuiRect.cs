using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiRect
  {
    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }

    [JsonProperty("w")]
    public float Width { get; set; }

    [JsonProperty("h")]
    public float Height { get; set; }

    public NuiRect() {}

    public NuiRect(float x, float y, float width, float height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }
  }
}
