using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonBiocodedWeapons : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!t.def.IsWeapon)
			{
				return false;
			}
			return !EquipmentUtility.IsBiocoded(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.IsWeapon;
		}
	}
}
