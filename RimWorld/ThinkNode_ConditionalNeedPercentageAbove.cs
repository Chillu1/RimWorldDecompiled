using Verse;
using Verse.AI;

namespace RimWorld;

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
		if (!pawn.needs.TryGetNeed(this.need, out var need))
		{
			return false;
		}
		if (need is Need_Seeker)
		{
			return need.CurInstantLevelPercentage > threshold;
		}
		return need.CurLevelPercentage > threshold;
	}
}
