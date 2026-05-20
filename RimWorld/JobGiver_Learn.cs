using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Learn : ThinkNode_JobGiver
{
	private const float MaxLearning = 0.95f;

	private static List<LearningDesireDef> tmpRandomLearningDesires = new List<LearningDesireDef>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		if (pawn.needs.learning.CurLevelPercentage >= 0.95f)
		{
			return null;
		}
		List<LearningDesireDef> activeLearningDesires = pawn.learning.ActiveLearningDesires;
		if (activeLearningDesires.NullOrEmpty())
		{
			return null;
		}
		tmpRandomLearningDesires.Clear();
		tmpRandomLearningDesires.AddRange(activeLearningDesires.InRandomOrder());
		for (int i = 0; i < tmpRandomLearningDesires.Count; i++)
		{
			if (tmpRandomLearningDesires[i].Worker.CanDo(pawn))
			{
				Job job = tmpRandomLearningDesires[i].Worker.TryGiveJob(pawn);
				if (job != null)
				{
					tmpRandomLearningDesires.Clear();
					job.isLearningDesire = true;
					return job;
				}
			}
		}
		tmpRandomLearningDesires.Clear();
		return null;
	}
}
