using Verse;

namespace RimWorld;

public static class FlagDefUtility
{
	public static bool HasFlag(this ulong flags, RenderSkipFlagDef flagDef)
	{
		return (flags & (ulong)flagDef) != 0;
	}

	public static void SetMaskFromIndex(Def def, ref ulong mask)
	{
		if (def.defName == "None")
		{
			mask = 0uL;
		}
		else
		{
			mask = (ulong)(1L << (int)def.index);
		}
	}
}
