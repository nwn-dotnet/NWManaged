using System;
using Anvil.Internal;
using NWN.Core;
using NWN.Native.API;

namespace Anvil.API
{
  public static class IntegerExtensions
  {
    /// <summary>
    /// Reinterprets the specified value as an unsigned byte.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static byte AsByte(this sbyte value)
    {
      return unchecked((byte)value);
    }

    /// <summary>
    /// Reinterprets the specified value as an int.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static int AsInt(this uint value)
    {
      return unchecked((int)value);
    }

    /// <summary>
    /// Reinterprets the specified value as a long.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static long AsLong(this ulong value)
    {
      return unchecked((long)value);
    }

    /// <summary>
    /// Reinterprets the specified value as a signed byte.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static sbyte AsSByte(this byte value)
    {
      return unchecked((sbyte)value);
    }

    /// <summary>
    /// Reinterprets the specified value as a signed short.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static short AsShort(this ushort value)
    {
      return unchecked((short)value);
    }

    /// <summary>
    /// Reinterprets the specified value as an unsigned int.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static uint AsUInt(this int value)
    {
      return unchecked((uint)value);
    }

    /// <summary>
    /// Reinterprets the specified value as an unsigned long.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static ulong AsULong(this long value)
    {
      return unchecked((ulong)value);
    }

    /// <summary>
    /// Reinterprets the specified value as an unsigned short.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>The reinterpreted value.</returns>
    public static ushort AsUShort(this short value)
    {
      return unchecked((ushort)value);
    }

    /// <summary>
    /// Reinterprets the specified value as a boolean.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>False if the value is 0, true for any non-0 value.</returns>
    public static bool ToBool(this int value)
    {
      return value != NWScript.FALSE;
    }

    /// <summary>
    /// Reinterprets the specified value as a integer.
    /// </summary>
    /// <param name="value">The value to reinterpret.</param>
    /// <returns>1 if true, 0 if false.</returns>
    public static int ToInt(this bool value)
    {
      return value ? NWScript.TRUE : NWScript.FALSE;
    }

    /// <summary>
    /// Converts the specified object ID value into a managed game object.
    /// </summary>
    /// <param name="objectId">The object ID to convert.</param>
    /// <typeparam name="T">The expected object type.</typeparam>
    /// <returns>The associated object if it exists, otherwise null.</returns>
    /// <exception cref="InvalidCastException">Thrown if the object associated with the object ID is not of type T.</exception>
    public static T? ToNwObject<T>(this uint objectId) where T : NwObject
    {
      return (T?)NwObject.CreateInternal(objectId);
    }

    /// <summary>
    /// Converts the specified object ID value into a managed game object.
    /// </summary>
    /// <param name="objectId">The object ID to convert.</param>
    /// <returns>The associated object if it exists, otherwise null.</returns>
    public static NwObject? ToNwObject(this uint objectId)
    {
      return NwObject.CreateInternal(objectId);
    }

    /// <summary>
    /// Converts the specified object ID value into a managed game object.
    /// </summary>
    /// <param name="objectId">The object ID to convert.</param>
    /// <typeparam name="T">The expected object type.</typeparam>
    /// <returns>The associated object if it exists and is of type T, otherwise null.</returns>
    public static T? ToNwObjectSafe<T>(this uint objectId) where T : NwObject
    {
      return NwObject.CreateInternal(objectId) as T;
    }

    /// <summary>
    /// Converts the specified object ID value into a managed player object.
    /// </summary>
    /// <param name="objectId">The object ID to convert.</param>
    /// <param name="playerSearch">Methods to use to resolve the player.</param>
    /// <returns>The associated player for this object, otherwise null.</returns>
    public static NwPlayer? ToNwPlayer(this uint objectId, PlayerSearch playerSearch = PlayerSearch.All)
    {
      if (objectId == NwObject.Invalid)
      {
        return null;
      }

      if (playerSearch.HasFlag(PlayerSearch.Controlled))
      {
        CNWSPlayer? player = LowLevel.ServerExoApp.GetClientObjectByObjectId(objectId);
        if (player != null)
        {
          return new NwPlayer(player);
        }
      }

      if (playerSearch.HasFlag(PlayerSearch.Login))
      {
        CExoArrayListCNWSPlayerPtr? players = LowLevel.ServerExoApp.m_pcExoAppInternal.m_lstPlayerList;
        foreach (CNWSPlayer player in players)
        {
          if (player.m_oidPCObject == objectId)
          {
            return new NwPlayer(player);
          }
        }
      }

      return null;
    }
  }
}
