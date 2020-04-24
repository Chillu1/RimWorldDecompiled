using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalMustKeepLyingDown : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.CurJob == null || !pawn.GetPosture().Laying())
			{
				return false;
			}
			if (!pawn.Downed)
			{
				if (RestUtility.DisturbancePreventsLyingDown(pawn))
				{
					return false;
				}
				if (!pawn.CurJob.restUntilHealed || !HealthAIUtility.ShouldSeekMedicalRest(pawn))
				{
					if (!pawn.jobs.curDriver.asleep)
					{
						return false;
					}
					if (!pawn.CurJob.playerForced && RestUtility.TimetablePreventsLayDown(pawn))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
