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
			if (CompBiocodable.IsBiocoded(t))
			{
				return t.TryGetComp<CompBladelinkWeapon>() != null;
			}
			return true;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.IsWeapon;
		}
	}
}
