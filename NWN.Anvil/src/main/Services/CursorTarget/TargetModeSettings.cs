using Anvil.API;

namespace Anvil.Services
{
  /// <summary>
  /// Configuration options for target mode. See <see cref="NwPlayer.EnterTargetMode"/>, <see cref="NwPlayer.TryEnterTargetMode"/>.
  /// </summary>
  public sealed class TargetModeSettings
  {
    /// <summary>
    /// Gets or sets the type of objects that are valid for selection.<br/>
    /// ObjectTypes is a flags enum, so multiple types may be specified using the OR operator (ObjectTypes.Creature | ObjectTypes.Placeable).
    /// </summary>
    public ObjectTypes ValidTargets { get; set; } = ObjectTypes.All;

    /// <summary>
    /// Gets or sets the type of cursor to show if the player is hovering over a valid target.
    /// </summary>
    public MouseCursor CursorType { get; set; } = MouseCursor.Magic;

    /// <summary>
    /// Gets or sets the type of cursor to show if the player is hovering over an invalid target.
    /// </summary>
    public MouseCursor BadCursorType { get; set; } = MouseCursor.NoMagic;

    /// <summary>
    /// Gets or sets overlay targeting data used to represent the effect generated by this target.
    /// </summary>
    public TargetingData? TargetingData { get; set; } = null;
  }
}
