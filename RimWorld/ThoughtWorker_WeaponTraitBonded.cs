using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_WeaponTraitBonded : ThoughtWorker_WeaponTrait
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!base.CurrentStateInternal(p).Active)
		{
			return ThoughtState.Inactive;
		}
		List<WeaponTraitDef> traitsListForReading = p.equipment.bondedWeapon.TryGetComp<CompBladelinkWeapon>().TraitsListForReading;
		for (int i = 0; i < traitsListForReading.Count; i++)
		{
			if (traitsListForReading[i].bondedThought == def)
			{
				return true;
			}
		}
		return ThoughtState.Inactive;
	}
}
