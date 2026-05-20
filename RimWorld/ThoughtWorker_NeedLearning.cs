using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedLearning : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModLister.CheckBiotech("Learning"))
			{
				return ThoughtState.Inactive;
			}
			if (p.needs.learning == null)
			{
				return ThoughtState.Inactive;
			}
			return p.needs.learning.CurCategory switch
			{
				LearningCategory.Empty => ThoughtState.ActiveAtStage(0), 
				LearningCategory.VeryLow => ThoughtState.ActiveAtStage(1), 
				LearningCategory.Low => ThoughtState.ActiveAtStage(2), 
				LearningCategory.Satisfied => ThoughtState.Inactive, 
				LearningCategory.High => ThoughtState.ActiveAtStage(3), 
				LearningCategory.Extreme => ThoughtState.ActiveAtStage(4), 
				_ => throw new NotImplementedException(), 
			};
		}
	}
}
