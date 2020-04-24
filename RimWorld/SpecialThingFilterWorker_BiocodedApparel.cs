using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_BiocodedApparel : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!t.def.IsApparel)
			{
				return false;
			}
			return EquipmentUtility.IsBiocoded(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (def.IsApparel)
			{
				return def.HasComp(typeof(CompBiocodableApparel));
			}
			return false;
		}
	}
}
