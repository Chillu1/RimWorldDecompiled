using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalAimingAtCount : ThinkNode_Conditional
	{
		public int thresholdAiming = 1;

		protected override bool Satisfied(Pawn pawn)
		{
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			int num = 0;
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				if (potentialTargetsFor[i].TargetCurrentlyAimingAt == pawn)
				{
					num++;
					if (num >= thresholdAiming)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
