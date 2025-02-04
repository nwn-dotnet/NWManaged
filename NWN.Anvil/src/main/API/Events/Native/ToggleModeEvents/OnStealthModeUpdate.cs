using System;
using System.Runtime.InteropServices;
using Anvil.API.Events;
using Anvil.Native;
using Anvil.Services;
using NWN.Native.API;

namespace Anvil.API.Events
{
  public sealed class OnStealthModeUpdate : IEvent
  {
    public NwCreature Creature { get; private init; } = null!;

    /// <summary>
    /// Gets or sets an override behaviour to use if <see cref="EventType"/> is an Enter event.
    /// </summary>
    public StealthModeOverride EnterOverride { get; set; }

    public ToggleModeEventType EventType { get; private init; }

    /// <summary>
    /// Gets or sets a value indicating whether this creature should not be allowed to exit stealth mode, if <see cref="EventType"/> is an Exit event.
    /// </summary>
    public bool PreventExit { get; set; }

    NwObject IEvent.Context => Creature;

    public sealed unsafe class Factory : HookEventFactory
    {
      private static FunctionHook<Functions.CNWSCreature.SetStealthMode> Hook { get; set; } = null!;

      protected override IDisposable[] RequestHooks()
      {
        delegate* unmanaged<void*, byte, void> pHook = &OnSetStealthMode;
        Hook = HookService.RequestHook<Functions.CNWSCreature.SetStealthMode>(pHook, HookOrder.Early);
        return [Hook];
      }

      private static void ForceEnterStealth(CNWSCreature creature, byte nStealthMode)
      {
        bool noHIPS = false;
        if (!creature.m_pStats.HasFeat((ushort)Feat.HideInPlainSight).ToBool())
        {
          creature.m_pStats.AddFeat((ushort)Feat.HideInPlainSight);
          noHIPS = true;
        }

        Hook.CallOriginal(creature, nStealthMode);

        if (noHIPS)
        {
          creature.m_pStats.RemoveFeat((ushort)Feat.HideInPlainSight);
        }
      }

      private static void HandleEnterStealth(CNWSCreature creature, byte nStealthMode)
      {
        OnStealthModeUpdate eventData = ProcessEvent(EventCallbackType.Before, new OnStealthModeUpdate
        {
          Creature = creature.ToNwObject<NwCreature>()!,
          EventType = ToggleModeEventType.Enter,
        });

        switch (eventData.EnterOverride)
        {
          case StealthModeOverride.ForceEnter:
            ForceEnterStealth(creature, nStealthMode);
            break;
          case StealthModeOverride.PreventHIPSEnter:
            PreventHIPSEnterStealth(creature, nStealthMode);
            break;
          case StealthModeOverride.PreventEnter:
            creature.ClearActivities(1);
            break;
          default:
            Hook.CallOriginal(creature, nStealthMode);
            break;
        }

        ProcessEvent(EventCallbackType.After, eventData);
      }

      private static void HandleExitStealth(CNWSCreature creature, byte nStealthMode)
      {
        OnStealthModeUpdate eventData = ProcessEvent(EventCallbackType.Before, new OnStealthModeUpdate
        {
          Creature = creature.ToNwObject<NwCreature>()!,
          EventType = ToggleModeEventType.Exit,
        });

        if (eventData.PreventExit)
        {
          creature.SetActivity(1, true.ToInt());
        }
        else
        {
          Hook.CallOriginal(creature, nStealthMode);
        }

        ProcessEvent(EventCallbackType.After, eventData);
      }

      [UnmanagedCallersOnly]
      private static void OnSetStealthMode(void* pCreature, byte nStealthMode)
      {
        CNWSCreature creature = CNWSCreature.FromPointer(pCreature);

        bool willBeStealthed = nStealthMode != 0;
        bool currentlyStealthed = creature.m_nStealthMode != 0;

        switch (currentlyStealthed)
        {
          case false when willBeStealthed:
            HandleEnterStealth(creature, nStealthMode);
            break;
          case true when !willBeStealthed:
            HandleExitStealth(creature, nStealthMode);
            break;
          default:
            Hook.CallOriginal(pCreature, nStealthMode);
            break;
        }
      }

      private static void PreventHIPSEnterStealth(CNWSCreature creature, byte nStealthMode)
      {
        bool bHadHIPS = false;
        if (creature.m_pStats.HasFeat((ushort)Feat.HideInPlainSight).ToBool())
        {
          creature.m_pStats.RemoveFeat((ushort)Feat.HideInPlainSight);
          bHadHIPS = true;
        }

        Hook.CallOriginal(creature, nStealthMode);

        if (bHadHIPS)
        {
          creature.m_pStats.AddFeat((ushort)Feat.HideInPlainSight);
        }
      }
    }
  }
}

namespace Anvil.API
{
  public sealed partial class NwCreature
  {
    /// <inheritdoc cref="Events.OnStealthModeUpdate"/>
    public event Action<OnStealthModeUpdate> OnStealthModeUpdate
    {
      add => EventService.Subscribe<OnStealthModeUpdate, OnStealthModeUpdate.Factory>(this, value);
      remove => EventService.Unsubscribe<OnStealthModeUpdate, OnStealthModeUpdate.Factory>(this, value);
    }
  }

  public sealed partial class NwModule
  {
    /// <inheritdoc cref="Events.OnStealthModeUpdate"/>
    public event Action<OnStealthModeUpdate> OnStealthModeUpdate
    {
      add => EventService.SubscribeAll<OnStealthModeUpdate, OnStealthModeUpdate.Factory>(value);
      remove => EventService.UnsubscribeAll<OnStealthModeUpdate, OnStealthModeUpdate.Factory>(value);
    }
  }
}
