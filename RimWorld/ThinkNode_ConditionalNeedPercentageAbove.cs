using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalNeedPercentageAbove : ThinkNode_Conditional
	{
		private NeedDef need;

		private float threshold;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalNeedPercentageAbove obj = (ThinkNode_ConditionalNeedPercentageAbove)base.DeepCopy(resolve);
			obj.need = need;
			obj.threshold = threshold;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.needs.TryGetNeed(need).CurLevelPercentage > threshold;
		}
	}
}
