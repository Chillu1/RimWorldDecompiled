using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_BiocodedWeapons : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!t.def.IsWeapon)
			{
				return false;
			}
			return EquipmentUtility.IsBiocoded(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (def.IsWeapon)
			{
				return def.HasComp(typeof(CompBiocodableWeapon));
			}
			return false;
		}
	}
}
