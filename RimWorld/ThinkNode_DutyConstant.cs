using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ThinkNode_DutyConstant : ThinkNode
	{
		private DefMap<DutyDef, int> dutyDefToSubNode;

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (pawn.GetLord() == null)
			{
				Log.Error(pawn + " doing ThinkNode_DutyConstant with no Lord.");
				return ThinkResult.NoJob;
			}
			if (pawn.mindState.duty == null)
			{
				Log.Error(pawn + " doing ThinkNode_DutyConstant with no duty.");
				return ThinkResult.NoJob;
			}
			if (dutyDefToSubNode == null)
			{
				Log.Error(pawn + " has null dutyDefToSubNode. Recovering by calling ResolveSubnodes() (though that should have been called already).");
				ResolveSubnodes();
			}
			int num = dutyDefToSubNode[pawn.mindState.duty.def];
			if (num < 0)
			{
				return ThinkResult.NoJob;
			}
			return subNodes[num].TryIssueJobPackage(pawn, jobParams);
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_DutyConstant thinkNode_DutyConstant = (ThinkNode_DutyConstant)base.DeepCopy(resolve);
			if (dutyDefToSubNode != null)
			{
				thinkNode_DutyConstant.dutyDefToSubNode = new DefMap<DutyDef, int>();
				thinkNode_DutyConstant.dutyDefToSubNode.SetAll(-1);
				{
					foreach (DutyDef allDef in DefDatabase<DutyDef>.AllDefs)
					{
						thinkNode_DutyConstant.dutyDefToSubNode[allDef] = dutyDefToSubNode[allDef];
					}
					return thinkNode_DutyConstant;
				}
			}
			return thinkNode_DutyConstant;
		}

		protected override void ResolveSubnodes()
		{
			dutyDefToSubNode = new DefMap<DutyDef, int>();
			dutyDefToSubNode.SetAll(-1);
			foreach (DutyDef allDef in DefDatabase<DutyDef>.AllDefs)
			{
				if (allDef.constantThinkNode != null)
				{
					dutyDefToSubNode[allDef] = subNodes.Count;
					allDef.constantThinkNode.ResolveSubnodesAndRecur();
					subNodes.Add(allDef.constantThinkNode.DeepCopy());
				}
			}
		}
	}
}
