using System;

namespace NWM.API.Events
{
  public interface IEvent<out T> : IEvent where T : IEvent<T>
  {
    event Action<T> Callbacks;
  }

  public interface IEvent
  {
    void BroadcastEvent(NwObject objSelf);
  }
}