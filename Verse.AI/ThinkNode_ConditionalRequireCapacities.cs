using System.Collections.Generic;

namespace Verse.AI
{
	public class ThinkNode_ConditionalRequireCapacities : ThinkNode_Conditional
	{
		public List<PawnCapacityDef> requiredCapacities;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalRequireCapacities obj = (ThinkNode_ConditionalRequireCapacities)base.DeepCopy(resolve);
			obj.requiredCapacities = requiredCapacities;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.health != null && pawn.health.capacities != null)
			{
				foreach (PawnCapacityDef requiredCapacity in requiredCapacities)
				{
					if (!pawn.health.capacities.CapableOf(requiredCapacity))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
