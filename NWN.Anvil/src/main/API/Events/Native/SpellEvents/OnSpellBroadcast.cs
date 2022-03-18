using System;
using System.Runtime.InteropServices;
using Anvil.API.Events;
using Anvil.Services;
using NWN.Native.API;

namespace Anvil.API.Events
{
  public sealed class OnSpellBroadcast : IEvent
  {
    public NwCreature Caster { get; private init; }

    public int ClassIndex { get; private init; }

    public NwFeat Feat { get; private init; }
    public bool PreventSpellCast { get; set; }

    public NwSpell Spell { get; private init; }

    NwObject IEvent.Context => Caster;

    internal sealed unsafe class Factory : HookEventFactory
    {
      internal delegate void BroadcastSpellCastHook(void* pCreature, uint nSpellId, byte nMultiClass, ushort nFeat);

      private static FunctionHook<BroadcastSpellCastHook> Hook { get; set; }

      protected override IDisposable[] RequestHooks()
      {
        delegate* unmanaged<void*, uint, byte, ushort, void> pHook = &OnBroadcastSpellCast;
        Hook = HookService.RequestHook<BroadcastSpellCastHook>(pHook, FunctionsLinux._ZN12CNWSCreature18BroadcastSpellCastEjht, HookOrder.Early);
        return new IDisposable[] { Hook };
      }

      [UnmanagedCallersOnly]
      private static void OnBroadcastSpellCast(void* pCreature, uint nSpellId, byte nMultiClass, ushort nFeat)
      {
        CNWSCreature creature = CNWSCreature.FromPointer(pCreature);

        OnSpellBroadcast eventData = ProcessEvent(new OnSpellBroadcast
        {
          Caster = creature.ToNwObject<NwCreature>(),
          Spell = NwSpell.FromSpellId((int)nSpellId),
          ClassIndex = nMultiClass,
          Feat = NwFeat.FromFeatId(nFeat),
        });

        if (!eventData.PreventSpellCast)
        {
          Hook.CallOriginal(pCreature, nSpellId, nMultiClass, nFeat);
        }
      }
    }
  }
}

namespace Anvil.API
{
  public abstract partial class NwGameObject
  {
    /// <inheritdoc cref="Events.OnSpellBroadcast"/>
    public event Action<OnSpellBroadcast> OnSpellBroadcast
    {
      add => EventService.Subscribe<OnSpellBroadcast, OnSpellBroadcast.Factory>(this, value);
      remove => EventService.Unsubscribe<OnSpellBroadcast, OnSpellBroadcast.Factory>(this, value);
    }
  }

  public sealed partial class NwModule
  {
    /// <inheritdoc cref="Events.OnSpellBroadcast"/>
    public event Action<OnSpellBroadcast> OnSpellBroadcast
    {
      add => EventService.SubscribeAll<OnSpellBroadcast, OnSpellBroadcast.Factory>(value);
      remove => EventService.UnsubscribeAll<OnSpellBroadcast, OnSpellBroadcast.Factory>(value);
    }
  }
}
