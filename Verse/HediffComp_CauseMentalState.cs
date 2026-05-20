using RimWorld;

namespace Verse;

public class HediffComp_CauseMentalState : HediffComp
{
	public HediffCompProperties_CauseMentalState Props => (HediffCompProperties_CauseMentalState)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (parent.Severity < Props.minSeverity || !base.Pawn.IsHashIntervalTick(60, delta))
		{
			return;
		}
		if (base.Pawn.RaceProps.Humanlike)
		{
			if (base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.humanMentalState && Rand.MTBEventOccurs(Props.mtbDaysToCauseMentalState, 60000f, 60f) && base.Pawn.Awake() && base.Pawn.Spawned && base.Pawn.mindState.mentalStateHandler.TryStartMentalState(Props.humanMentalState, parent.def.LabelCap, Props.forced, forceWake: false, causedByMood: false, null, transitionSilently: true))
			{
				TriggeredMentalBreak(Props.humanMentalState);
			}
		}
		else if (base.Pawn.RaceProps.Animal && base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.animalMentalState && (Props.animalMentalStateAlias == null || base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.animalMentalStateAlias) && Rand.MTBEventOccurs(Props.mtbDaysToCauseMentalState, 60000f, 60f) && base.Pawn.Awake() && base.Pawn.Spawned && base.Pawn.mindState.mentalStateHandler.TryStartMentalState(Props.animalMentalState, parent.def.LabelCap, forced: false, forceWake: false, causedByMood: false, null, transitionSilently: true))
		{
			TriggeredMentalBreak(Props.animalMentalState);
		}
	}

	public override void CompPostPostRemoved()
	{
		if (Props.endMentalStateOnCure && ((base.Pawn.RaceProps.Humanlike && base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.humanMentalState) || (base.Pawn.RaceProps.Animal && (base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalState || base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalStateAlias))) && !base.Pawn.mindState.mentalStateHandler.CurState.causedByMood)
		{
			base.Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
		}
	}

	private void TriggeredMentalBreak(MentalStateDef mentalStateDef)
	{
		SendLetter(mentalStateDef);
		if (Props.removeOnTriggered)
		{
			base.Pawn.health.RemoveHediff(parent);
		}
	}

	private void SendLetter(MentalStateDef mentalStateDef)
	{
		string text = (string.IsNullOrEmpty(Props.overrideLetterLabel) ? ((mentalStateDef.beginLetterLabel ?? ((string)mentalStateDef.LabelCap)).CapitalizeFirst() + ": " + base.Pawn.LabelShortCap) : Props.overrideLetterLabel.Formatted(base.Pawn.Named("PAWN")).Resolve());
		string text2 = (string.IsNullOrEmpty(Props.overrideLetterDesc) ? string.Format("{0}\n\n{1}", base.Pawn.mindState.mentalStateHandler.CurState.GetBeginLetterText(), "CausedByHediff".Translate(parent.LabelCap)) : Props.overrideLetterDesc.Formatted(base.Pawn.Named("PAWN")).Resolve());
		Find.LetterStack.ReceiveLetter(text, text2, Props.letterDef ?? mentalStateDef.beginLetterDef, base.Pawn);
	}
}
