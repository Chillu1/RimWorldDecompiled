using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ThinkNode_Duty : ThinkNode
{
	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		Lord lord = pawn.GetLord();
		if (lord == null)
		{
			Log.Error(pawn?.ToString() + " doing ThinkNode_Duty with no Lord.");
			return ThinkResult.NoJob;
		}
		if (pawn.mindState.duty == null)
		{
			Log.Error(pawn?.ToString() + " doing ThinkNode_Duty with no duty.");
			return ThinkResult.NoJob;
		}
		ThinkResult result = subNodes[pawn.mindState.duty.def.index].TryIssueJobPackage(pawn, jobParams);
		result = lord.Notify_DutyResult(result, pawn, jobParams);
		if (result.Job != null)
		{
			result.Job.lord = pawn.GetLord();
			result.Job.source = pawn.mindState.duty.source;
			result.Job.dutyTag = pawn.mindState.duty.tag;
		}
		return result;
	}

	protected override void ResolveSubnodes()
	{
		foreach (DutyDef allDef in DefDatabase<DutyDef>.AllDefs)
		{
			allDef.thinkNode.ResolveSubnodesAndRecur();
			subNodes.Add(allDef.thinkNode.DeepCopy());
		}
	}
}
