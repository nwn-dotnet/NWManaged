/*
 * Find a trigger with the tag "mytrigger" and create a handler for its "OnEnter" event.
 */

using System.Linq;
using NLog;
using NWN.API;
using NWN.API.Events;
using NWN.Services;

[ServiceBinding(typeof(TriggerHandlerService))]
public class TriggerHandlerService
{
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();

  public TriggerHandlerService()
  {
    NwTrigger trigger = NwObject.FindObjectsWithTag<NwTrigger>("mytrigger").FirstOrDefault();
    trigger.OnEnter += OnTriggerEnter;
  }

  private void OnTriggerEnter(TriggerEvents.OnEnter obj)
  {
    if (obj.EnteringObject is NwPlayer player)
    {
      Log.Info("Player entered trigger: " + player?.PlayerName);
    }
  }
}
