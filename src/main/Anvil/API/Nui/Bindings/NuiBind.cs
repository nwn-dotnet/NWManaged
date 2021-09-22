using Newtonsoft.Json;
using NWN.Core;

namespace Anvil.API
{
  /// <summary>
  /// A NUI property binding that can be updated after being sent to the client.
  /// </summary>
  /// <typeparam name="T">The type of value being bound.</typeparam>
  public sealed class NuiBind<T> : NuiProperty<T>
  {
    [JsonProperty("bind")]
    public string Key { get; init; }

    public NuiBind(string key)
    {
      Key = key;
    }

    /// <summary>
    /// Queries the specified player for the value of this binding.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <param name="nUiToken">The associated UI token.</param>
    /// <returns>The current value of the binding.</returns>
    public T GetBindValue(NwPlayer player, int nUiToken)
    {
      Json json = NWScript.NuiGetBind(player.ControlledCreature, nUiToken, Key);
      return JsonConvert.DeserializeObject<T>(json.Dump());
    }

    /// <summary>
    /// Assigns a value to the binding for the specified player.
    /// </summary>
    /// <param name="player">The player whose binding will be updated.</param>
    /// <param name="uiToken">The unique UI token to be updated.</param>
    /// <param name="value">The new value to assign.</param>
    public void SetBindValue(NwPlayer player, int uiToken, T value)
    {
      string jsonString = JsonConvert.SerializeObject(value);
      Json json = Json.Parse(jsonString);

      NWScript.NuiSetBind(player.ControlledCreature, uiToken, Key, json);
    }

    /// <summary>
    /// Marks this property as watched/un-watched.<br/>
    /// A watched bind will invoke the NUI script event every time its value changes.
    /// </summary>
    /// <param name="player">The player whose binding will be updated.</param>
    /// <param name="uiToken">The unique UI token to be updated.</param>
    /// <param name="watch">True if the value should be watched, false if it should not be watched.</param>
    public void SetBindWatch(NwPlayer player, int uiToken, bool watch)
    {
      NWScript.NuiSetBindWatch(player.ControlledCreature, uiToken, Key, watch.ToInt());
    }
  }
}
