using Verse;

namespace RimWorld;

public class MapMeshFlagDef : Def
{
	private ulong mask;

	public override void PostSetIndices()
	{
		FlagDefUtility.SetMaskFromIndex(this, ref mask);
	}

	public static implicit operator ulong(MapMeshFlagDef def)
	{
		return def?.mask ?? 0;
	}

	public override string ToString()
	{
		return base.ToString() + $" ({mask})";
	}
}
