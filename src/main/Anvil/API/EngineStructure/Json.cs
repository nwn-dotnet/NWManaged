using System;
using NWN.Core;

namespace Anvil.API
{
  public sealed class Json : EngineStructure
  {
    internal Json(IntPtr handle) : base(handle) {}

    protected override int StructureId
    {
      get => NWScript.ENGINE_STRUCTURE_JSON;
    }

    public static implicit operator Json(IntPtr intPtr)
    {
      return new Json(intPtr);
    }

    public static Json Parse(string jsonString)
    {
      return NWScript.JsonParse(jsonString);
    }

    public string Dump()
    {
      return NWScript.JsonDump(this);
    }
  }
}
