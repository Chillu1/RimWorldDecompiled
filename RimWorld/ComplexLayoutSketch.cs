using Verse;

namespace RimWorld;

public class ComplexLayoutSketch : LayoutSketch
{
	protected override ThingDef GetWallStuff(int roomId)
	{
		if (roomId % 2 != 0)
		{
			return ThingDefOf.Steel;
		}
		return wallStuff;
	}
}
