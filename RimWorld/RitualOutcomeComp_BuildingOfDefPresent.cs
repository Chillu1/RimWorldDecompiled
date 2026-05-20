using Verse;

namespace RimWorld
{
	public class RitualOutcomeComp_BuildingOfDefPresent : RitualOutcomeComp_BuildingsPresent
	{
		public ThingDef def;

		protected override string LabelForDesc => def.LabelCap;

		protected override Thing LookForBuilding(IntVec3 cell, Map map, Precept_Ritual ritual)
		{
			return cell.GetFirstThing(map, def);
		}
	}
}
