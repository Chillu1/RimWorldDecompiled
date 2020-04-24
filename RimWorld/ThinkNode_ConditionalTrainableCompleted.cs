using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalTrainableCompleted : ThinkNode_Conditional
	{
		private TrainableDef trainable;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalTrainableCompleted obj = (ThinkNode_ConditionalTrainableCompleted)base.DeepCopy(resolve);
			obj.trainable = trainable;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.training != null)
			{
				return pawn.training.HasLearned(trainable);
			}
			return false;
		}
	}
}
