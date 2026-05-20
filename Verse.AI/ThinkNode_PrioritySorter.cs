using System;
using System.Collections.Generic;

namespace Verse.AI;

public class ThinkNode_PrioritySorter : ThinkNode
{
	public float minPriority;

	private static List<ThinkNode> workingNodes = new List<ThinkNode>();

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_PrioritySorter obj = (ThinkNode_PrioritySorter)base.DeepCopy(resolve);
		obj.minPriority = minPriority;
		return obj;
	}

	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		workingNodes.Clear();
		int count = subNodes.Count;
		for (int i = 0; i < count; i++)
		{
			workingNodes.Insert(Rand.Range(0, workingNodes.Count - 1), subNodes[i]);
		}
		while (workingNodes.Count > 0)
		{
			float num = 0f;
			int num2 = -1;
			for (int j = 0; j < workingNodes.Count; j++)
			{
				float num3 = 0f;
				try
				{
					num3 = workingNodes[j].GetPriority(pawn);
				}
				catch (Exception ex)
				{
					Log.Error("Exception in " + GetType()?.ToString() + " GetPriority: " + ex.ToString());
				}
				if (!(num3 <= 0f) && !(num3 < minPriority) && num3 > num)
				{
					num = num3;
					num2 = j;
				}
			}
			if (num2 == -1)
			{
				break;
			}
			ThinkResult result = ThinkResult.NoJob;
			try
			{
				result = workingNodes[num2].TryIssueJobPackage(pawn, jobParams);
			}
			catch (Exception ex2)
			{
				Log.Error("Exception in " + GetType()?.ToString() + " TryIssueJobPackage: " + ex2.ToString());
			}
			if (result.IsValid)
			{
				return result;
			}
			workingNodes.RemoveAt(num2);
		}
		return ThinkResult.NoJob;
	}
}
