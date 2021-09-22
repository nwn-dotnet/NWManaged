using Newtonsoft.Json;

namespace Anvil.API
{
  public sealed class NuiColor
  {
    /// <summary>
    /// Gets the red value of this color as a byte (0-255).
    /// </summary>
    [JsonProperty("r")]
    public readonly byte Red;

    /// <summary>
    /// Gets or sets the green value of this color as a byte (0-255).
    /// </summary>
    [JsonProperty("g")]
    public byte Green { get; set; }

    /// <summary>
    /// Gets or sets the blue value of this color as a byte (0-255).
    /// </summary>
    [JsonProperty("b")]
    public byte Blue { get; set; }

    /// <summary>
    /// Gets the alpha value of this color as a byte (0-255).
    /// </summary>
    [JsonProperty("a")]
    public byte Alpha { get; set; }

    /// <summary>
    /// Constructs a new Color from the given rgba values.
    /// </summary>
    /// <param name="red">The red value.</param>
    /// <param name="green">The green value.</param>
    /// <param name="blue">The blue value.</param>
    /// <param name="alpha">The alpha value.</param>
    public NuiColor(byte red, byte green, byte blue, byte alpha = 255)
    {
      Red = red;
      Green = green;
      Blue = blue;
      Alpha = alpha;
    }

    public static implicit operator Color(NuiColor nuiColor)
    {
      return new Color(nuiColor.Red, nuiColor.Green, nuiColor.Blue, nuiColor.Alpha);
    }

    public static implicit operator NuiColor(Color color)
    {
      return new NuiColor(color.Red, color.Green, color.Blue, color.Alpha);
    }
  }
}
