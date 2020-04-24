namespace Verse.AI
{
	public class ThinkNode_ForbidOutsideFlagRadius : ThinkNode_Priority
	{
		public float maxDistToSquadFlag = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ForbidOutsideFlagRadius obj = (ThinkNode_ForbidOutsideFlagRadius)base.DeepCopy(resolve);
			obj.maxDistToSquadFlag = maxDistToSquadFlag;
			return obj;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			try
			{
				if (maxDistToSquadFlag > 0f)
				{
					if (pawn.mindState.maxDistToSquadFlag > 0f)
					{
						Log.Error("Squad flag was not reset properly; raiders may behave strangely");
					}
					pawn.mindState.maxDistToSquadFlag = maxDistToSquadFlag;
				}
				return base.TryIssueJobPackage(pawn, jobParams);
			}
			finally
			{
				pawn.mindState.maxDistToSquadFlag = -1f;
			}
		}
	}
}
