using System;

namespace Verse.AI;

public class ThinkNode_Priority : ThinkNode
{
	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		int count = subNodes.Count;
		for (int i = 0; i < count; i++)
		{
			ThinkResult result = ThinkResult.NoJob;
			try
			{
				result = subNodes[i].TryIssueJobPackage(pawn, jobParams);
			}
			catch (Exception ex)
			{
				Log.Error("Exception in " + GetType()?.ToString() + " TryIssueJobPackage: " + ex.ToString());
			}
			if (result.IsValid)
			{
				return result;
			}
		}
		return ThinkResult.NoJob;
	}
}
