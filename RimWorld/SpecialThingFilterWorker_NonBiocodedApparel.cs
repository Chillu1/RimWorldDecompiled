using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonBiocodedApparel : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!t.def.IsApparel)
			{
				return false;
			}
			return !EquipmentUtility.IsBiocoded(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.IsApparel;
		}
	}
}
