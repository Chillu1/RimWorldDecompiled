using Verse;

namespace RimWorld;

public class TraitMentalStateGiver
{
	public TraitDegreeData traitDegreeData;

	public virtual bool CheckGive(Pawn pawn, int checkInterval)
	{
		if (traitDegreeData.forcedMentalState != null)
		{
			float forcedMentalStateMtbDays = traitDegreeData.forcedMentalStateMtbDays;
			if (forcedMentalStateMtbDays > 0f && Rand.MTBEventOccurs(forcedMentalStateMtbDays, 60000f, checkInterval) && traitDegreeData.forcedMentalState.Worker.StateCanOccur(pawn))
			{
				return pawn.mindState.mentalStateHandler.TryStartMentalState(traitDegreeData.forcedMentalState, "MentalStateReason_Trait".Translate(traitDegreeData.label));
			}
		}
		if (traitDegreeData.randomMentalState != null)
		{
			float curMood = pawn.mindState.mentalBreaker.CurMood;
			float num = traitDegreeData.randomMentalStateMtbDaysMoodCurve.Evaluate(curMood);
			if (num > 0f && Rand.MTBEventOccurs(num, 60000f, checkInterval) && traitDegreeData.randomMentalState.Worker.StateCanOccur(pawn))
			{
				return pawn.mindState.mentalStateHandler.TryStartMentalState(traitDegreeData.randomMentalState, "MentalStateReason_Trait".Translate(traitDegreeData.GetLabelFor(pawn)));
			}
		}
		return false;
	}
}
