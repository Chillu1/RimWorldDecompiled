using Verse;

namespace RimWorld
{
	public class RitualPosition_BesideTree : RitualPosition_ThingDef
	{
		protected override ThingDef ThingDef => ThingDefOf.Plant_TreeGauranlen;

		public override IntVec3 PositionForThing(Thing t)
		{
			return t.Position - new IntVec3(1, 0, 0);
		}

		protected override Rot4 FacingDir(Thing t)
		{
			return Rot4.East;
		}
	}
}
