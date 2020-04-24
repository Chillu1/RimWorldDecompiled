using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_MarriageCeremony : LordToilData
	{
		public CellRect spectateRect;

		public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref spectateRect, "spectateRect");
			Scribe_Values.Look(ref spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.None);
		}
	}
}
