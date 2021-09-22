using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Anvil.Internal;
using Anvil.Services;
using NLog;
using NWN.Core;
using NWN.Native.API;
using Vector = NWN.Native.API.Vector;

namespace Anvil.API
{
  public sealed partial class NwPlayer : IEquatable<NwPlayer>
  {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [Inject]
    private static EventService EventService { get; set; }

    internal readonly CNWSPlayer Player;

    internal NwPlayer(CNWSPlayer player)
    {
      Player = player;
      PlayerId = player.m_nPlayerID;
    }

    public static implicit operator CNWSPlayer(NwPlayer player)
    {
      return player?.Player;
    }

    public bool Equals(NwPlayer other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return Player.Pointer.Equals(other.Player.Pointer);
    }

    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj) || obj is NwPlayer other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Player.Pointer.GetHashCode();
    }

    public static bool operator ==(NwPlayer left, NwPlayer right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(NwPlayer left, NwPlayer right)
    {
      return !Equals(left, right);
    }

    /// <summary>
    /// Gets the unique numeric ID of this player.
    /// </summary>
    public uint PlayerId { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="NwPlayer"/> object is valid.<br/>
    /// Returns false after the player disconnects from the server.
    /// </summary>
    public bool IsValid
    {
      get
      {
        CNWSClient client = LowLevel.ServerExoApp.GetClientObjectByPlayerId(PlayerId);
        return client != null && client.AsNWSPlayer() == Player;
      }
    }

    /// <summary>
    /// Gets the creature this player is currently controlling.<br/>
    /// This will return the player's current possessed creature (familiar, DM possession), otherwise their player character if they are currently not possessing a creature.
    /// </summary>
    public NwCreature ControlledCreature
    {
      get => Player.m_oidNWSObject.ToNwObject<NwCreature>();
    }

    /// <summary>
    /// Gets the original creature that this player logged in with.
    /// </summary>
    public NwCreature LoginCreature
    {
      get => Player.m_oidPCObject.ToNwObject<NwCreature>();
    }

    /// <summary>
    /// Gets a value indicating whether the player is a Dungeon Master.
    /// </summary>
    public bool IsDM
    {
      get => NWScript.GetIsDM(ControlledCreature).ToBool();
    }

    /// <summary>
    /// Gets or sets whether the player has DM privileges gained through a player login (as opposed to the DM client).
    /// </summary>
    public bool IsPlayerDM
    {
      get => NWScript.GetIsPlayerDM(ControlledCreature).ToBool();
      set
      {
        if (IsPlayerDM == value)
        {
          return;
        }

        CNetLayerPlayerInfo playerInfo = LowLevel.ServerExoApp.GetNetLayer().GetPlayerInfo(PlayerId);
        if (playerInfo == null)
        {
          return;
        }

        if (!playerInfo.SatisfiesBuild(8193, 14))
        {
          Log.Warn("{PlayerName} cannot toggle DM mode as the player's client does not support PlayerDM functionality", PlayerName);
          return;
        }

        if (playerInfo.m_bGameMasterPrivileges.ToBool() && !playerInfo.m_bGameMasterIsPlayerLogin.ToBool())
        {
          Log.Warn("{PlayerName} cannot toggle DM mode as this player is using the DMClient", PlayerName);
          return;
        }

        CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
        if (message == null)
        {
          return;
        }

        bool currentlyPlayerDM = playerInfo.m_bGameMasterPrivileges.ToBool() && playerInfo.m_bGameMasterIsPlayerLogin.ToBool();
        if (value && !currentlyPlayerDM)
        {
          playerInfo.m_bGameMasterPrivileges = true.ToInt();
          playerInfo.m_bGameMasterIsPlayerLogin = true.ToInt();
          message.SendServerToPlayerDungeonMasterLoginState(Player, true.ToInt(), true.ToInt());

          NwCreature creature = ControlledCreature;
          if (creature != null)
          {
            creature.Creature.m_pStats.m_bDMManifested = true.ToInt();
            creature.Creature.UpdateVisibleList();
          }

          LowLevel.ServerExoApp.AddToExclusionList(Player.m_oidNWSObject, 1); // Timestop
          LowLevel.ServerExoApp.AddToExclusionList(Player.m_oidNWSObject, 2); // Pause
          byte nActivePauseState = LowLevel.ServerExoApp.GetActivePauseState();
          message.SendServerToPlayerModule_SetPauseState(nActivePauseState, (nActivePauseState > 0).ToInt());

          message.SendServerToPlayerDungeonMasterAreaList(PlayerId);
          message.SendServerToPlayerDungeonMasterCreatorLists(Player);
        }
        else if (!value && currentlyPlayerDM)
        {
          Player.PossessCreature(NwObject.Invalid, (byte)NWN.Native.API.AssociateType.None);

          playerInfo.m_bGameMasterPrivileges = false.ToInt();
          playerInfo.m_bGameMasterIsPlayerLogin = false.ToInt();
          message.SendServerToPlayerDungeonMasterLoginState(Player, false.ToInt(), true.ToInt());

          NwCreature creature = ControlledCreature;
          if (creature != null)
          {
            creature.Creature.m_pStats.m_bDMManifested = true.ToInt();
            creature.Creature.UpdateVisibleList();
          }

          LowLevel.ServerExoApp.RemoveFromExclusionList(Player.m_oidNWSObject, 1); // Timestop
          LowLevel.ServerExoApp.RemoveFromExclusionList(Player.m_oidNWSObject, 2); // Pause

          byte nActivePauseState = LowLevel.ServerExoApp.GetActivePauseState();
          message.SendServerToPlayerModule_SetPauseState(nActivePauseState, (nActivePauseState > 0).ToInt());
        }
      }
    }

    /// <summary>
    /// Gets the player's login name.
    /// </summary>
    public string PlayerName
    {
      get => NWScript.GetPCPlayerName(ControlledCreature);
    }

    /// <summary>
    /// Gets the player's client version (Major + Minor).
    /// </summary>
    public Version ClientVersion
    {
      get => new Version(NWScript.GetPlayerBuildVersionMajor(ControlledCreature), NWScript.GetPlayerBuildVersionMinor(ControlledCreature));
    }

    /// <summary>
    /// Gets the public part of the CD key that the player used when logging in.
    /// </summary>
    public string CDKey
    {
      get => NWScript.GetPCPublicCDKey(ControlledCreature, true.ToInt());
    }

    /// <summary>
    /// Gets the connecting IP address for the player.
    /// </summary>
    public string IPAddress
    {
      get => NWScript.GetPCIPAddress(ControlledCreature);
    }

    /// <summary>
    /// Gets a value indicating whether the player has connected to the server over a relay (instead of directly).
    /// </summary>
    public bool IsConnectionRelayed
    {
      get => NWScript.GetIsPlayerConnectionRelayed(ControlledCreature).ToBool();
    }

    /// <summary>
    /// Gets or sets the movement rate factor for the cutscene camera following ControlledCreature 'camera man'.
    /// </summary>
    public float CutsceneCameraMoveRate
    {
      get => NWScript.GetCutsceneCameraMoveRate(ControlledCreature);
      set => NWScript.SetCutsceneCameraMoveRate(ControlledCreature, value);
    }

    /// <summary>
    /// Sets the camera height for this player.
    /// </summary>
    public float CameraHeight
    {
      set => NWScript.SetCameraHeight(ControlledCreature, value);
    }

    /// <summary>
    /// Gets or sets the location that this player will spawn at when logging in to the server.
    /// </summary>
    public Location SpawnLocation
    {
      get
      {
        if (LoginCreature == null)
        {
          return null;
        }

        CNWSCreature creature = LoginCreature.Creature;
        return Location.Create(creature.m_oidDesiredArea.ToNwObject<NwArea>(), creature.m_vDesiredAreaLocation.ToManagedVector(), LoginCreature.Rotation);
      }
      set
      {
        if (LoginCreature == null)
        {
          return;
        }

        Player.m_bFromTURD = true.ToInt();

        CNWSCreature creature = LoginCreature.Creature;
        creature.m_oidDesiredArea = value.Area;
        creature.m_vDesiredAreaLocation = value.Position.ToNativeVector();
        creature.m_bDesiredAreaUpdateComplete = false.ToInt();
        LoginCreature.Rotation = value.Rotation;
      }
    }

    /// <summary>
    /// Gets a value indicating whether ControlledCreature creature is currently in "Cutscene" mode.
    /// </summary>
    public bool IsInCutsceneMode
    {
      get => NWScript.GetCutsceneMode(ControlledCreature).ToBool();
    }

    /// <summary>
    /// Gets the name of the player's .bic file.
    /// </summary>
    public string BicFileName
    {
      get => Player.m_resFileName.ToString();
    }

    /// <summary>
    /// Gets the members in the player's party.
    /// </summary>
    public IEnumerable<NwPlayer> PartyMembers
    {
      get
      {
        for (uint member = NWScript.GetFirstFactionMember(ControlledCreature); member != NwObject.Invalid; member = NWScript.GetNextFactionMember(ControlledCreature))
        {
          yield return member.ToNwPlayer();
        }
      }
    }

    /// <summary>
    /// Boots the player from the server.
    /// </summary>
    /// <param name="reason">An optional message to show to the player.</param>
    public void BootPlayer(string reason = "")
    {
      NWScript.BootPC(ControlledCreature, reason);
    }

    /// <summary>
    /// Adds the player to the specified party leader's party.
    /// </summary>
    /// <param name="partyLeader">The party leader of the party to join.</param>
    public void AddToParty(NwPlayer partyLeader)
    {
      NWScript.AddToParty(ControlledCreature, partyLeader.ControlledCreature);
    }

    /// <summary>
    /// Removes the player from their current party.
    /// </summary>
    public void RemoveFromCurrentParty()
    {
      NWScript.RemoveFromParty(ControlledCreature);
    }

    /// <summary>
    /// Attaches the specified creature to the player as a henchmen.
    /// </summary>
    /// <param name="henchmen">The henchmen to attach to the player.</param>
    public void AddHenchmen(NwCreature henchmen)
    {
      NWScript.AddHenchman(ControlledCreature, henchmen);
    }

    /// <summary>
    /// Adds an entry to the player's journal.
    /// </summary>
    /// <param name="categoryTag">The tag of the Journal category (case-sensitive).</param>
    /// <param name="entryId">The ID of the Journal entry.</param>
    /// <param name="allPartyMembers">If true, ControlledCreature entry is added to all players in the player's party.</param>
    /// <param name="allowOverrideHigher">If true, disables the default restriction that requires journal entry numbers to increase.</param>
    public void AddJournalQuestEntry(string categoryTag, int entryId, bool allPartyMembers = true, bool allowOverrideHigher = false)
    {
      NWScript.AddJournalQuestEntry(categoryTag, entryId, ControlledCreature, allPartyMembers.ToInt(), false.ToInt(), allowOverrideHigher.ToInt());
    }

    /// <summary>
    /// Instructs the player to open their inventory.
    /// </summary>
    public void OpenInventory()
    {
      NWScript.OpenInventory(ControlledCreature, ControlledCreature);
    }

    /// <summary>
    /// Opens the specified creatures inventory, and shows it to the player.<br/>
    /// </summary>
    /// <remarks>
    /// DMs can see any player or creature's inventory. Players can only view their own inventory, or that of a henchmen.
    /// </remarks>
    /// <param name="target">The target creature's inventory to view.</param>
    public void OpenInventory(NwCreature target)
    {
      NWScript.OpenInventory(target, ControlledCreature);
    }

    /// <summary>
    /// Forces the player to open the inventory of the specified placeable.
    /// </summary>
    /// <param name="target">The placeable inventory to be viewed.</param>
    public void OpenInventory(NwPlaceable target)
    {
      target.Placeable.m_bHasInventory = 1;
      target.Placeable.OpenInventory(ControlledCreature);
    }

    /// <summary>
    /// Gets the specified campaign variable for the player.
    /// </summary>
    /// <param name="campaign">The name of the campaign.</param>
    /// <param name="name">The variable name.</param>
    /// <typeparam name="T">The variable type.</typeparam>
    /// <returns>A CampaignVariable instance for getting/setting the variable's value.</returns>
    public T GetCampaignVariable<T>(string campaign, string name) where T : CampaignVariable, new()
    {
      return CampaignVariable.Create<T>(campaign, name, this);
    }

    /// <summary>
    /// Sends a server message to the player.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="color">A color to apply to the message.</param>
    public void SendServerMessage(string message, Color color)
    {
      NWScript.SendMessageToPC(ControlledCreature, message.ColorString(color));
    }

    /// <inheritdoc cref="SendServerMessage(string,Color)"/>
    public void SendServerMessage(string message)
    {
      NWScript.SendMessageToPC(ControlledCreature, message);
    }

    /// <summary>
    /// Sets if the player should like, or unlike the specified player.
    /// </summary>
    /// <param name="like">true if the player should like the target, false if the player should dislike the target.</param>
    /// <param name="target">The target to like/dislike.</param>
    public void SetPCReputation(bool like, NwPlayer target)
    {
      if (like)
      {
        NWScript.SetPCLike(ControlledCreature, target.ControlledCreature);
      }
      else
      {
        NWScript.SetPCDislike(ControlledCreature, target.ControlledCreature);
      }
    }

    /// <summary>
    /// Starts a conversation with another object, typically a creature.
    /// </summary>
    /// <param name="converseWith">The target object to converse with.</param>
    /// <param name="dialogResRef">The dialogue to start. If ControlledCreature is unset, the target's own dialogue file will be used.</param>
    /// <param name="isPrivate">Whether ControlledCreature dialogue should be visible to all nearby players, or visible to the player only.</param>
    /// <param name="playHello">Whether the hello/greeting should be played once the dialogue starts.</param>
    public async Task ActionStartConversation(NwGameObject converseWith, string dialogResRef = "", bool isPrivate = false, bool playHello = true)
    {
      await ControlledCreature.WaitForObjectContext();
      NWScript.ActionStartConversation(converseWith, dialogResRef, isPrivate.ToInt(), playHello.ToInt());
    }

    /// <summary>
    /// Shows an examine dialog for the specified target.
    /// </summary>
    /// <param name="target">The target to examine.</param>
    public async Task ActionExamine(NwGameObject target)
    {
      await ControlledCreature.WaitForObjectContext();
      NWScript.ActionExamine(target);
    }

    /// <summary>
    /// Restores the camera mode and facing to what they where when StoreCameraFacing was last called.<br/>
    /// RestoreCameraFacing can only be used once, and must correspond to a previous call to StoreCameraFacing.
    /// </summary>
    public async Task RestoreCameraFacing()
    {
      await ControlledCreature.WaitForObjectContext();
      NWScript.RestoreCameraFacing();
    }

    /// <summary>
    /// Stores (bookmarks) the camera's facing and position so it can be restored later with RestoreCameraFacing.<br/>
    /// </summary>
    public async Task StoreCameraFacing()
    {
      await ControlledCreature.WaitForObjectContext();
      NWScript.StoreCameraFacing();
    }

    /// <summary>
    /// Changes the direction the player's camera is facing.
    /// </summary>
    /// <param name="direction">Horizontal angle from East in degrees. -1 to leave the angle unmodified.</param>
    /// <param name="pitch">Vertical angle of the camera in degrees. -1 to leave the angle unmodified.</param>
    /// <param name="distance">Distance (zoom) of the camera. -1 to leave the distance unmodified.</param>
    /// <param name="transitionType">The transition to use for moving the camera.</param>
    public async Task SetCameraFacing(float direction, float pitch = -1.0f, float distance = -1.0f, CameraTransitionType transitionType = CameraTransitionType.Snap)
    {
      await ControlledCreature.WaitForObjectContext();
      NWScript.SetCameraFacing(direction, distance, pitch, (int)transitionType);
    }

    /// <summary>
    /// Forces the player's character to saved and exported to its respective directory (LocalVault, ServerVault, etc).
    /// </summary>
    public void ExportCharacter()
    {
      NWScript.ExportSingleCharacter(ControlledCreature);
    }

    /// <summary>
    /// Sends the player to a new server, where the player's character will connect and log in.
    /// </summary>
    /// <param name="ipAddress">DNS name or the IP address + port of the destination server.</param>
    /// <param name="password">The player password to connect to the destination server.</param>
    /// <param name="waypointTag">The custom waypoint tag on the destination server for the player to jump to. Defaults to the module's start location.</param>
    /// <param name="seamless">If true, the player will not be prompted with information about the new server, and they will not be allowed to save a copy of their character (if it is a local vault character).</param>
    public void SendToServer(string ipAddress = "", string password = "", string waypointTag = "", bool seamless = false)
    {
      NWScript.ActivatePortal(ControlledCreature, ipAddress, password, waypointTag, seamless.ToInt());
    }

    /// <summary>
    /// Sets whether the player has explored an area.
    /// </summary>
    /// <param name="area">The area to explore.</param>
    /// <param name="explored">true if ControlledCreature area has been explored, otherwise false to (re)hide the map.</param>
    public void SetAreaExplorationState(NwArea area, bool explored)
    {
      NWScript.ExploreAreaForPlayer(area, ControlledCreature, explored.ToInt());
    }

    /// <summary>
    /// Vibrates the player's device or controller. Does nothing if vibration is not supported.
    /// </summary>
    /// <param name="motor">Which motors to vibrate.</param>
    /// <param name="strength">The intensity of the vibration.</param>
    /// <param name="duration">How long to vibrate for.</param>
    public void Vibrate(VibratorMotor motor, float strength, TimeSpan duration)
    {
      NWScript.Vibrate(ControlledCreature, (int)motor, strength, (float)duration.TotalSeconds);
    }

    /// <summary>
    /// Unlock an achievement for the player who must be logged in.
    /// </summary>
    /// <param name="achievementId">The achievement ID on the remote server.</param>
    /// <param name="lastValue">The previous value of the associated achievement stat.</param>
    /// <param name="currentValue">The current value of the associated achievement stat.</param>
    /// <param name="maxValue">The maximum value of the associate achievement stat.</param>
    public void UnlockAchievement(string achievementId, int lastValue = 0, int currentValue = 0, int maxValue = 0)
    {
      NWScript.UnlockAchievement(ControlledCreature, achievementId, lastValue, currentValue, maxValue);
    }

    /// <summary>
    /// Makes ControlledCreature PC load a new texture instead of another.
    /// </summary>
    /// <param name="oldTexName">The existing texture to replace.</param>
    /// <param name="newTexName">The new override texture.</param>
    public void SetTextureOverride(string oldTexName, string newTexName)
    {
      NWScript.SetTextureOverride(oldTexName, newTexName, ControlledCreature);
    }

    /// <summary>
    /// Removes the override for the specified texture, reverting to the original texture.
    /// </summary>
    /// <param name="texName">The name of the original texture.</param>
    public void ClearTextureOverride(string texName)
    {
      NWScript.SetTextureOverride(texName, string.Empty, ControlledCreature);
    }

    /// <summary>
    /// Toggles the CutsceneMode state for the player.
    /// </summary>
    /// <param name="inCutscene">True if cutscene mode should be enabled, otherwise false.</param>
    /// <param name="leftClickEnabled">True if ControlledCreature user should be allowed to interact with the game with the left mouse button. False to prevent interaction.</param>
    public void SetCutsceneMode(bool inCutscene = true, bool leftClickEnabled = false)
    {
      NWScript.SetCutsceneMode(ControlledCreature, inCutscene.ToInt(), leftClickEnabled.ToInt());
    }

    /// <summary>
    /// Displays a message on the player's screen.<br/>
    /// The message is always displayed on top of whatever is on the screen, including UI elements.
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="xPos">The x coordinate relative to anchor.</param>
    /// <param name="yPos">The y coordinate relative to anchor.</param>
    /// <param name="anchor">The screen anchor/origin point.</param>
    /// <param name="life">Duration to show ControlledCreature string in seconds.</param>
    /// <param name="start">The starting color of ControlledCreature text (default: white).</param>
    /// <param name="end">The color of the text to fade to as it nears the end of the lifetime (default: white).</param>
    /// <param name="id">An optional numeric ID for ControlledCreature string. If not set to 0, subsequent calls to PostString will remove the text with the same ID.</param>
    /// <param name="font">If specified, the message will be rendered with the specified font instead of the default console font.</param>
    public void PostString(string message, int xPos, int yPos, ScreenAnchor anchor, float life, Color? start = null, Color? end = null, int id = 0, string font = "")
    {
      start ??= ColorConstants.White;
      end ??= ColorConstants.White;

      NWScript.PostString(ControlledCreature, message, xPos, yPos, (int)anchor, life, start.Value.ToInt(), end.Value.ToInt(), id, font);
    }

    /// <summary>
    /// Briefly displays a floating text message above the player's head using the specified string reference.
    /// </summary>
    /// <param name="strRef">The string ref index to use.</param>
    /// <param name="broadcastToParty">If true, shows the floating message to all players in the same party.</param>
    public void FloatingTextStrRef(int strRef, bool broadcastToParty = true)
    {
      NWScript.FloatingTextStrRefOnCreature(strRef, ControlledCreature, broadcastToParty.ToInt());
    }

    /// <summary>
    /// Briefly displays a floating text message above the player's head.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="broadcastToParty">If true, shows the floating message to all players in the same party.</param>
    public void FloatingTextString(string message, bool broadcastToParty = true)
    {
      NWScript.FloatingTextStringOnCreature(message, ControlledCreature, broadcastToParty.ToInt());
    }

    /// <summary>
    /// Enters "Cutscene" mode, disabling GUI and camera controls for the player and marking them as plot object (invulnerable).<br/>
    /// See <see cref="Effect.CutsceneGhost"/>, and other Cutscene* effects for hiding and controlling the player creature during cutscene mode.
    /// </summary>
    /// <param name="allowLeftClick">If true, allows the player to interact with the game world using the left mouse button only. Otherwise, prevents all interactions.</param>
    public void EnterCutsceneMode(bool allowLeftClick = false)
    {
      // Prevent permanent invulnerability.
      if (IsInCutsceneMode)
      {
        return;
      }

      NWScript.SetCutsceneMode(ControlledCreature, true.ToInt(), allowLeftClick.ToInt());
    }

    /// <summary>
    /// Exits "Cutscene" mode, restoring standard GUI and camera controls to the player, and restoring their plot flag.
    /// </summary>
    public void ExitCutsceneMode()
    {
      if (!IsInCutsceneMode)
      {
        return;
      }

      NWScript.SetCutsceneMode(ControlledCreature, false.ToInt(), true.ToInt());
    }

    /// <summary>
    /// Gives the specified XP to the player, adjusted by any multiclass penalty.
    /// </summary>
    /// <param name="xPAmount">Amount of experience to give.</param>
    public void GiveXp(int xPAmount)
    {
      NWScript.GiveXPToCreature(ControlledCreature, xPAmount);
    }

    /// <summary>
    /// Locks the player's camera direction to its current direction,
    /// or unlocks the player's camera direction to enable it to move freely again.
    /// </summary>
    public void LockCameraDirection(bool isLocked = true)
    {
      NWScript.LockCameraDirection(ControlledCreature, isLocked.ToInt());
    }

    /// <summary>
    /// Locks the player's camera pitch to its current pitch setting,
    /// or unlocks the player's camera pitch.
    /// </summary>
    public void LockCameraPitch(bool isLocked = true)
    {
      NWScript.LockCameraPitch(ControlledCreature, isLocked.ToInt());
    }

    /// <summary>
    /// Disable a specific gui panel for this player.<br/>
    /// Will close the GUI panel if it is currently open.<br/>
    /// Will fire a <see cref="GuiEventType.DisabledPanelAttemptOpen"/> event for some panels if a player attempts to open them while disabled.
    /// </summary>
    /// <param name="panel">The panel type to disable.</param>
    /// <param name="disabled">True to disable the panel, false to re-enable the panel.</param>
    public void SetGuiPanelDisabled(GUIPanel panel, bool disabled)
    {
      NWScript.SetGuiPanelDisabled(ControlledCreature, (int)panel, disabled.ToInt());
    }

    /// <summary>
    /// Locks the player's camera distance to its current distance setting,
    /// or unlocks the player's camera distance.
    /// </summary>
    public void LockCameraDistance(bool isLocked = true)
    {
      NWScript.LockCameraDistance(ControlledCreature, isLocked.ToInt());
    }

    /// <summary>
    /// Changes the current Day/Night cycle for the player to daylight.
    /// </summary>
    /// <param name="delayTransitionTime">Time it takes for the daylight to fade in.</param>
    public void NightToDay(TimeSpan delayTransitionTime = default)
    {
      NWScript.NightToDay(ControlledCreature, (float)delayTransitionTime.TotalSeconds);
    }

    /// <summary>
    /// Displays a death panel that can turn off the "Respawn" or "Wait for Help" buttons.<br/>
    /// The "Wait for Help" button is only enabled when the game is running in multiplayer mode.<br/>
    ///  By default if helpString isn't specified, the value used is "Choose an option below.<br/>
    /// Respawning will incur a penalty of 50 XP per level of your character and the loss of 10% of your gold."<br/>
    /// (strref 66219 single player, 6600 for multiplayer).<br/>
    /// </summary>
    /// <param name="respawnButton">If true the "Respawn" button will be enabled.</param>
    /// <param name="waitForHelp">If true the "Wait For Help" button will be enabled.</param>
    /// <param name="helpStringRef">String reference to display for hel.</param>
    /// <param name="helpString">String to display for help which appears in the top of the panel.</param>
    public void PopUpDeathPanel(bool respawnButton = true, bool waitForHelp = true, int helpStringRef = 0, string helpString = "")
    {
      NWScript.PopUpDeathGUIPanel(ControlledCreature, respawnButton.ToInt(), waitForHelp.ToInt(), helpStringRef, helpString);
    }

    /// <summary>
    /// Displays a GUI panel to a player.
    /// </summary>
    /// <param name="panel">The panel type to display.</param>
    public void PopUpGUIPanel(GUIPanel panel = GUIPanel.Death)
    {
      NWScript.PopUpGUIPanel(ControlledCreature, (int)panel);
    }

    /// <summary>
    /// Fades the screen for a given player from black to regular screen.
    /// </summary>
    /// <param name="fadeSpeed">Determines how fast the fade occurs.</param>
    public void FadeFromBlack(float fadeSpeed)
    {
      NWScript.FadeFromBlack(ControlledCreature, fadeSpeed);
    }

    /// <summary>
    /// Fades the screen for a given player from a regular screen to black.
    /// </summary>
    /// <param name="fadeSpeed">Determines how fast the fade occurs.</param>
    public void FadeToBlack(float fadeSpeed)
    {
      NWScript.FadeToBlack(ControlledCreature, fadeSpeed);
    }

    /// <summary>
    /// Removes any current fading effects or black screen from the monitor of the player.
    /// </summary>
    public void StopFade()
    {
      NWScript.StopFade(ControlledCreature);
    }

    /// <summary>
    /// Forces the player to examine the specified placeable.
    /// </summary>
    /// <param name="target">The placeable to examine.</param>
    public void ForceExamine(NwPlaceable target)
    {
      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      message?.SendServerToPlayerExamineGui_PlaceableData(Player, target);
    }

    /// <summary>
    /// Immediately kicks the player and deletes their character file (.bic).
    /// </summary>
    /// <param name="kickMessage">The kick message to show to the player.</param>
    /// <param name="preserveBackup">If true, instead of being deleted it will be renamed to be hidden from the character list, but remain in the vault directory.</param>
    public async Task Delete(string kickMessage, bool preserveBackup = true)
    {
      string bicName = BicFileName;
      string serverVault = NwServer.Instance.GetAliasPath("SERVERVAULT");
      string playerDir = NwServer.Instance.ServerInfo.PersistentWorldOptions.ServerVaultByPlayerName ? PlayerName : CDKey;
      string characterName = LoginCreature.Name;
      string playerName = PlayerName;

      string fileName = $"{serverVault}{playerDir}/{bicName}.bic";
      if (!File.Exists(fileName))
      {
        Log.Error("Character file {FileName} not found", fileName);
        return;
      }

      await NwTask.NextFrame();

      // Boot the player
      LowLevel.ServerExoApp.GetNetLayer().DisconnectPlayer(PlayerId, 10392, 1, kickMessage.ToExoString());

      await NwTask.NextFrame();

      // Delete their character's TURD
      bool turdDeleted = NwServer.Instance.DeletePlayerTURD(playerName, characterName);
      if (!turdDeleted)
      {
        Log.Warn("Could not delete the TURD for deleted character {Character}", characterName);
      }

      if (preserveBackup)
      {
        string backupName = $"{fileName}.deleted";
        int i = 0;

        while (File.Exists(backupName + i))
        {
          i++;
        }

        File.Move(fileName, backupName + i);
      }
      else
      {
        File.Delete(fileName);
      }
    }

    /// <summary>
    /// Overrides the specified string from the TlkTable using the specified override for the player only.<br/>
    /// Overrides will not persist through re-logging.
    /// </summary>
    /// <param name="strRef">The string reference to be overridden.</param>
    /// <param name="strOverride">The new string to assign.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid string ref is specified (&lt; 0).</exception>
    public void SetTlkOverride(int strRef, string strOverride)
    {
      if (strRef < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(strRef), "StrRef must not be less than 0.");
      }

      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      message?.SendServerToPlayerSetTlkOverride(Player.m_nPlayerID, strRef, strOverride.ToExoString());
    }

    /// <summary>
    /// Clears the specified TlkTable override for the player, optionally restoring the global override.
    /// </summary>
    /// <param name="strRef">The overridden string reference to restore.</param>
    /// <param name="restoreGlobal">If true, restores the global override current set for ControlledCreature string ref.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid string ref is specified (&lt; 0).</exception>
    public void ClearTlkOverride(int strRef, bool restoreGlobal = true)
    {
      if (strRef < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(strRef), "StrRef must not be less than 0.");
      }

      string strOverride = string.Empty;
      if (restoreGlobal && NWNXLib.TlkTable().m_overrides.TryGetValue((uint)strRef, out CExoString globalOverride))
      {
        strOverride = globalOverride.ToString();
      }

      SetTlkOverride(strRef, strOverride);
    }

    public byte[] Serialize(bool stripPCFlags = false)
    {
      if (!stripPCFlags)
      {
        return ControlledCreature.Serialize();
      }

      ControlledCreature.Creature.m_bPlayerCharacter = (!ControlledCreature.Creature.m_bPlayerCharacter.ToBool()).ToInt();
      ControlledCreature.Creature.m_pStats.m_bIsPC = (!ControlledCreature.Creature.m_pStats.m_bIsPC.ToBool()).ToInt();

      try
      {
        return ControlledCreature.Serialize();
      }
      finally
      {
        ControlledCreature.Creature.m_bPlayerCharacter = (!ControlledCreature.Creature.m_bPlayerCharacter.ToBool()).ToInt();
        ControlledCreature.Creature.m_pStats.m_bIsPC = (!ControlledCreature.Creature.m_pStats.m_bIsPC.ToBool()).ToInt();
      }
    }

    public void DisplayFloatingTextStringOnCreature(NwCreature target, string text)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(target), "Target cannot be null.");
      }

      if (text == null)
      {
        throw new ArgumentNullException(nameof(text), "Text cannot be null.");
      }

      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      if (message == null)
      {
        return;
      }

      CNWCCMessageData messageData = new CNWCCMessageData();
      messageData.SetObjectID(0, target);
      messageData.SetInteger(9, 94);
      messageData.SetString(0, text.ToExoString());

      message.SendServerToPlayerCCMessage(Player.m_nPlayerID, (byte)MessageClientSideMsgMinor.Feedback, messageData, null);
    }

    /// <summary>
    /// Plays the specified VFX at the target position in the current area for this player only.
    /// </summary>
    /// <param name="effectType">The effect to play.</param>
    /// <param name="position">Where to play the effect.</param>
    public void ShowVisualEffect(VfxType effectType, Vector3 position)
    {
      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      if (message == null)
      {
        return;
      }

      ObjectVisualTransformData visualTransformData = new ObjectVisualTransformData();
      message.SendServerToPlayerArea_VisualEffect(Player, (ushort)effectType, position.ToNativeVector(), visualTransformData);
    }

    /// <summary>
    /// Plays the specified "instant" VFX (Com*, FNF*, IMP*) on the target for this player only.
    /// </summary>
    /// <param name="visualEffect">The effect to play.</param>
    /// <param name="target">The target object to play the effect upon.</param>
    public void ApplyInstantVisualEffectToObject(VfxType visualEffect, NwGameObject target)
    {
      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      if (message == null)
      {
        return;
      }

      Vector vTargetPosition = new Vector();
      message.SendServerToPlayerGameObjUpdateVisEffect(Player, (ushort)visualEffect, target, NwModule.Instance,
        0, 0, vTargetPosition, 0.0f);
    }

    /// <summary>
    /// Plays the specified sound at the target in the current area for this player only.
    /// </summary>
    /// <param name="sound">The sound resref.</param>
    /// <param name="target">The target object for the sound to originate. Defaults to the location of the player.</param>
    public void PlaySound(string sound, NwGameObject target = null)
    {
      if (target == null)
      {
        target = ControlledCreature;
      }

      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      message?.SendServerToPlayerAIActionPlaySound(PlayerId, target, sound.ToExoString());
    }

    /// <summary>
    /// Gets an existing journal entry with the specified quest tag.
    /// </summary>
    /// <param name="questTag">The quest tag you wish to get the journal entry for.</param>
    /// <returns>A <see cref="JournalEntry"/> structure containing the journal entry data, null if an entry with the specified tag cannot be found.</returns>
    public JournalEntry GetJournalEntry(string questTag)
    {
      NwCreature creature = ControlledCreature;
      if (creature == null)
      {
        return null;
      }

      CNWSJournal journal = creature.Creature.GetJournal();
      CExoArrayListSJournalEntry entries = journal.m_lstEntries;

      for (int i = entries.Count - 1; i >= 0; i--)
      {
        SJournalEntry entry = entries[i];
        if (entry.szPlot_Id.ToString() == questTag)
        {
          return new JournalEntry
          {
            Name = entry.szName.ExtractLocString(),
            Text = entry.szText.ExtractLocString(),
            CalendarDay = entry.nCalendarDay,
            TimeOfDay = entry.nTimeOfDay,
            QuestTag = questTag,
            State = entry.nState,
            Priority = entry.nPriority,
            QuestCompleted = entry.bQuestCompleted.ToBool(),
            QuestDisplayed = entry.bQuestDisplayed.ToBool(),
            Updated = entry.bUpdated.ToBool(),
          };
        }
      }

      return null;
    }

    /// <summary>
    /// Adds/updates a journal entry.
    /// </summary>
    /// <param name="entryData">The new/updated journal entry.</param>
    /// <param name="silentUpdate">false = Notify player via sound effects and feedback message, true = Suppress sound effects and feedback message</param>
    /// <returns>A positive number to indicate the new amount of journal entries on the player, -1 on an error.</returns>
    public int AddCustomJournalEntry(JournalEntry entryData, bool silentUpdate = false)
    {
      int retVal = -1;

      NwCreature creature = ControlledCreature;
      if (creature == null)
      {
        return retVal;
      }

      CNWSMessage message = LowLevel.ServerExoApp.GetNWSMessage();
      if (message == null)
      {
        return retVal;
      }

      CNWSJournal journal = creature.Creature.GetJournal();
      CExoArrayListSJournalEntry entries = journal.m_lstEntries;

      SJournalEntry newJournal = new SJournalEntry
      {
        szName = entryData.Name.ToExoLocString(),
        szText = entryData.Text.ToExoLocString(),
        nCalendarDay = entryData.CalendarDay,
        nTimeOfDay = entryData.TimeOfDay,
        szPlot_Id = entryData.QuestTag.ToExoString(),
        nState = entryData.State,
        nPriority = entryData.Priority,
        nPictureIndex = 0,
        bQuestCompleted = entryData.QuestCompleted.ToInt(),
        bQuestDisplayed = entryData.QuestDisplayed.ToInt(),
        bUpdated = entryData.Updated.ToInt(),
      };

      int overwrite = -1;
      if (entries.Count > 0)
      {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
          SJournalEntry entry = entries[i];
          if (entry.szPlot_Id.ToString() == entryData.QuestTag)
          {
            entries.RemoveAt(i);
            entries.Insert(i, entry);
            overwrite = i;
            break;
          }
        }
      }

      if (overwrite == -1)
      {
        journal.m_lstEntries.Add(newJournal);
      }

      retVal = journal.m_lstEntries.Count;

      message.SendServerToPlayerJournalAddQuest(Player,
        newJournal.szPlot_Id,
        (int)newJournal.nState,
        newJournal.nPriority,
        newJournal.nPictureIndex,
        newJournal.bQuestCompleted,
        newJournal.nCalendarDay,
        newJournal.nTimeOfDay,
        newJournal.szName,
        newJournal.szText);

      if (!silentUpdate)
      {
        message.SendServerToPlayerJournalUpdated(Player, 1, newJournal.bQuestCompleted, newJournal.szName);
      }

      return retVal;
    }

    /// <summary>
    /// Gets this player's area exploration state for the specified area.
    /// </summary>
    /// <param name="area">The area to query.</param>
    /// <returns>A byte array representing the tiles explored for the area.</returns>
    public unsafe byte[] GetAreaExplorationState(NwArea area)
    {
      NwCreature creature = LoginCreature;
      if (area == null || creature == null)
      {
        return null;
      }

      for (int i = 0; i < creature.Creature.m_oidAutoMapAreaList.Count; i++)
      {
        uint oidArea = creature.Creature.m_oidAutoMapAreaList[i];
        if (oidArea != area)
        {
          continue;
        }

        byte* tileData = *(creature.Creature.m_nAutoMapTileData + i);
        if (tileData != null)
        {
          byte[] retVal = new byte[area.Area.m_nMapSize];
          Marshal.Copy((IntPtr)tileData, retVal, 0, area.Area.m_nMapSize);
          return retVal;
        }

        break;
      }

      return null;
    }

    /// <summary>
    /// Sets this player's area exploration state for the specified area.
    /// </summary>
    /// <param name="area">The area to modify.</param>
    /// <param name="newState">A byte array representing the tiles explored for the area, as returned by <see cref="GetAreaExplorationState"/>.</param>
    public unsafe void SetAreaExplorationState(NwArea area, byte[] newState)
    {
      NwCreature creature = LoginCreature;
      if (area == null || creature == null || newState == null)
      {
        return;
      }

      for (int i = 0; i < creature.Creature.m_oidAutoMapAreaList.Count; i++)
      {
        uint oidArea = creature.Creature.m_oidAutoMapAreaList[i];
        if (oidArea != area)
        {
          continue;
        }

        byte* tileData = *(creature.Creature.m_nAutoMapTileData + i);
        if (tileData != null)
        {
          Marshal.Copy(newState, 0, (IntPtr)tileData, area.Area.m_nMapSize);
        }

        break;
      }
    }

    /// <summary>
    /// Sets up a SQL Query for this player.<br/>
    /// This will NOT run the query; only make it available for parameter binding.<br/>
    /// To run the query, you need to call <see cref="SQLQuery.Execute"/> even if you do not expect result data.<br/>
    /// </summary>
    /// <param name="query">The query to be prepared.</param>
    /// <returns>A <see cref="SQLQuery"/> object.</returns>
    public SQLQuery PrepareSQLQuery(string query)
    {
      return NWScript.SqlPrepareQueryObject(ControlledCreature, query);
    }

    /// <summary>
    /// Immediately destroys the database attached to this player, clearing out all data and schema.<br/>
    /// This operation is _immediate_ and _irreversible_, even when inside a transaction or running query.<br/>
    /// Existing active/prepared sqlqueries will remain functional, but any references to stored data or schema members will be invalidated.
    /// </summary>
    public void DestroySQLDatabase()
    {
      NWScript.SqlDestroyDatabase(ControlledCreature);
    }

    /// <summary>
    /// Create a NUI window inline for this player.
    /// </summary>
    /// <param name="window">The window to create.</param>
    /// <param name="windowId">A unique alphanumeric ID identifying this window. Re-creating a window with the same id of one already open will immediately close the old one.</param>
    /// <returns>The window token on success (!= 0), or 0 on error.</returns>
    public int CreateNuiWindow(NuiWindow window, string windowId = "")
    {
      string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(window);
      Json json = Json.Parse(jsonString);
      return NWScript.NuiCreate(ControlledCreature, json, windowId);
    }
  }
}
