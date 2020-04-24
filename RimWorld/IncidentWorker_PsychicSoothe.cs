using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class IncidentWorker_PsychicSoothe : IncidentWorker_PsychicEmanation
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (base.TryExecuteWorker(parms))
			{
				SoundDefOf.PsychicSootheGlobal.PlayOneShotOnCamera((Map)parms.target);
				return true;
			}
			return false;
		}

		protected override void DoConditionAndLetter(IncidentParms parms, Map map, int duration, Gender gender, float points)
		{
			GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration);
			gameCondition_PsychicEmanation.gender = gender;
			map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
			SendStandardLetter(gameCondition_PsychicEmanation.LabelCap, gameCondition_PsychicEmanation.LetterText, gameCondition_PsychicEmanation.def.letterDef, parms, LookTargets.Invalid);
		}
	}
}
