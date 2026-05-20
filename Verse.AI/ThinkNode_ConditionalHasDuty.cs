namespace Verse.AI;

public class ThinkNode_ConditionalHasDuty : ThinkNode_Conditional
{
	public DutyDef dutyDef;

	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.mindState?.duty?.def == dutyDef;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalHasDuty obj = (ThinkNode_ConditionalHasDuty)base.DeepCopy(resolve);
		obj.dutyDef = dutyDef;
		return obj;
	}
}
