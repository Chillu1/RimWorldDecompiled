namespace Verse;

public static class PawnGraphicUtils
{
	public static bool TryGetAlternate(this Pawn pawn, out AlternateGraphic ag, out int index)
	{
		ag = null;
		index = -1;
		Rand.PushState(pawn.thingIDNumber ^ 0xB415);
		if (Rand.Chance(pawn.kindDef.alternateGraphicChance) && pawn.kindDef.alternateGraphics.TryRandomElementByWeight((AlternateGraphic x) => x.Weight, out ag))
		{
			index = pawn.kindDef.alternateGraphics.IndexOf(ag);
		}
		Rand.PopState();
		return ag != null;
	}

	public static int GetGraphicIndex(this Pawn pawn)
	{
		if (pawn.TryGetAlternate(out var _, out var index))
		{
			return index;
		}
		return -1;
	}
}
