using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_WeaponTraitKillNeed : ThoughtWorker_WeaponTrait
{
	public const int TicksToGiveMemory = 1200000;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!base.CurrentStateInternal(p).Active)
		{
			return ThoughtState.Inactive;
		}
		CompBladelinkWeapon compBladelinkWeapon = p.equipment.bondedWeapon.TryGetComp<CompBladelinkWeapon>();
		List<WeaponTraitDef> traitsListForReading = compBladelinkWeapon.TraitsListForReading;
		for (int i = 0; i < traitsListForReading.Count; i++)
		{
			if (traitsListForReading[i] == WeaponTraitDefOf.NeedKill)
			{
				return compBladelinkWeapon.TicksSinceLastKill > 1200000;
			}
		}
		return ThoughtState.Inactive;
	}
}
