using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalCloseToDutyTarget : ThinkNode_Conditional
	{
		private float maxDistToDutyTarget = 10f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalCloseToDutyTarget obj = (ThinkNode_ConditionalCloseToDutyTarget)base.DeepCopy(resolve);
			obj.maxDistToDutyTarget = maxDistToDutyTarget;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty.focus.IsValid)
			{
				return pawn.Position.InHorDistOf(pawn.mindState.duty.focus.Cell, maxDistToDutyTarget);
			}
			return false;
		}
	}
}
