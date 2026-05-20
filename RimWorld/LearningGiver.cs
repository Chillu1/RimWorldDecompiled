using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LearningGiver
	{
		public const float NeedThresholdOffsetStart = 0.1f;

		public const float NeedThresholdOffsetStop = -0.05f;

		public LearningDesireDef def;

		public virtual bool CanGiveDesire => true;

		public virtual bool CanDo(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (pawn.DevelopmentalStage.Child() && !pawn.Downed && !PawnUtility.WillSoonHaveBasicNeed(pawn, 0.1f) && pawn.needs.learning != null)
			{
				return pawn.needs.learning.CurLevel < 0.9f;
			}
			return false;
		}

		public virtual Job TryGiveJob(Pawn pawn)
		{
			return JobMaker.MakeJob(def.jobDef);
		}
	}
}
