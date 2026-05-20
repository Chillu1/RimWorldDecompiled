namespace Verse.AI;

public class ThinkNode_ConditionalDoingJob : ThinkNode_Conditional
{
	public JobDef jobDef;

	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.CurJobDef == jobDef;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalDoingJob obj = (ThinkNode_ConditionalDoingJob)base.DeepCopy(resolve);
		obj.jobDef = jobDef;
		return obj;
	}
}
