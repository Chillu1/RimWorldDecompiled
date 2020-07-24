using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ThinkNode_Duty : ThinkNode
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (pawn.GetLord() == null)
			{
				Log.Error(string.Concat(pawn, " doing ThinkNode_Duty with no Lord."));
				return ThinkResult.NoJob;
			}
			if (pawn.mindState.duty == null)
			{
				Log.Error(string.Concat(pawn, " doing ThinkNode_Duty with no duty."));
				return ThinkResult.NoJob;
			}
			return subNodes[pawn.mindState.duty.def.index].TryIssueJobPackage(pawn, jobParams);
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
}
