using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalHashIntervalTick : ThinkNode_Conditional
{
	public int interval;

	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.IsHashIntervalTick(interval);
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalHashIntervalTick obj = (ThinkNode_ConditionalHashIntervalTick)base.DeepCopy(resolve);
		obj.interval = interval;
		return obj;
	}
}
