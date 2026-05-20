using Verse;
using Verse.Sound;

namespace RimWorld;

public class IncidentWorker_PsychicDrone : IncidentWorker_PsychicEmanation
{
	private const float MaxPointsDroneLow = 800f;

	private const float MaxPointsDroneMedium = 2000f;

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (base.TryExecuteWorker(parms))
		{
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera((Map)parms.target);
			return true;
		}
		return false;
	}

	protected override void DoConditionAndLetter(IncidentParms parms, Map map, int duration, Gender gender, float points)
	{
		if (points < 0f)
		{
			points = StorytellerUtility.DefaultThreatPointsNow(map);
		}
		PsychicDroneLevel level = ((points < 800f) ? PsychicDroneLevel.BadLow : ((!(points < 2000f)) ? PsychicDroneLevel.BadHigh : PsychicDroneLevel.BadMedium));
		GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration);
		gameCondition_PsychicEmanation.gender = gender;
		gameCondition_PsychicEmanation.level = level;
		map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
		SendStandardLetter(gameCondition_PsychicEmanation.LabelCap, gameCondition_PsychicEmanation.LetterText, gameCondition_PsychicEmanation.def.letterDef, parms, LookTargets.Invalid);
	}
}
