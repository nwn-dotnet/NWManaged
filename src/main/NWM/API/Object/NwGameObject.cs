using System.Collections.Generic;
using NWM.API.Constants;
using NWN;
using Vector3 = System.Numerics.Vector3;

namespace NWM.API
{
  public abstract class NwGameObject : NwObject
  {
    internal NwGameObject(uint objectId) : base(objectId) {}

    public VisualTransform VisualTransform
    {
      get => new VisualTransform(this);
      set => value?.Apply(this);
    }

    public virtual Location Location
    {
      get => NWScript.GetLocation(this);
      set => ExecuteOnSelf(() => NWScript.JumpToLocation(value));
    }

    /// <summary>
    /// The local area position of this GameObject.
    /// </summary>
    public Vector3 Position
    {
      get => NWScript.GetPosition(this);
      set => ExecuteOnSelf(() => NWScript.JumpToLocation(NWScript.Location(Area, value, Rotation)));
    }

    /// <summary>
    /// The world rotation for this object
    /// </summary>
    public virtual float Rotation
    {
      get => NWScript.GetFacing(this) % 360;
      set => ExecuteOnSelf(() => NWScript.SetFacing(value % 360));
    }

    public int HP
    {
      get => NWScript.GetCurrentHitPoints(this);
    }

    public void FaceToObject(NwGameObject nwObject)
    {
      FaceToPoint(nwObject.Position);
    }

    public virtual void FaceToPoint(Vector3 point)
    {
      AssignCommand(() => NWScript.SetFacingPoint(point));
    }

    public NwArea Area
    {
      get => NWScript.GetArea(this).ToNwObject<NwArea>();
    }

    public bool PlotFlag
    {
      get => NWScript.GetPlotFlag(this).ToBool();
      set => NWScript.SetPlotFlag(this, value.ToInt());
    }

    public IEnumerable<NwItem> Items
    {
      get
      {
        for (NwItem item = NWScript.GetFirstItemInInventory(this).ToNwObject<NwItem>(); item != INVALID; item = NWScript.GetNextItemInInventory(this).ToNwObject<NwItem>())
        {
          yield return item;
        }
      }
    }

    /// <summary>
    /// Plays the specified animation
    /// </summary>
    /// <param name="animation">Constant value representing the animation to play.</param>
    /// <param name="animSpeed">Speed to play the animation.</param>
    /// <param name="queueAsAction">If true, enqueues animation playback in the object's action queue.</param>
    /// <param name="duration">Duration to keep animating. Not used in fire and forget animations.</param>
    public void PlayAnimation(Animation animation, float animSpeed, bool queueAsAction = false, float duration = 0.0f)
    {
      if (!queueAsAction)
      {
        ExecuteOnSelf(() => NWScript.PlayAnimation((int) animation, animSpeed, duration));
      }
      else
      {
        ExecuteOnSelf(() => NWScript.ActionPlayAnimation((int) animation, animSpeed, duration));
      }

    }

    public void SpeakString(string message, TalkVolume talkVolume = TalkVolume.Talk, bool queueAsAction = false)
    {
      if (!queueAsAction)
      {
        ExecuteOnSelf(() => NWScript.SpeakString(message, (int) talkVolume));
      }
      else
      {
        ExecuteOnSelf(() => NWScript.ActionSpeakString(message, (int) talkVolume));
      }
    }

    public void Destroy(float delay = 0.0f)
    {
      NWScript.DestroyObject(this, delay);
    }
  }
}