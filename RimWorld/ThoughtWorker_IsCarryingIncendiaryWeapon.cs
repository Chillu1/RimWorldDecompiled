using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsCarryingIncendiaryWeapon : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.equipment.Primary == null)
			{
				return false;
			}
			List<Verb> allVerbs = p.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i].IsIncendiary())
				{
					return true;
				}
			}
			return false;
		}
	}
}
