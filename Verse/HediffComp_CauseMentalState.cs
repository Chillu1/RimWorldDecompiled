using RimWorld;

namespace Verse
{
	public class HediffComp_CauseMentalState : HediffComp
	{
		public HediffCompProperties_CauseMentalState Props => (HediffCompProperties_CauseMentalState)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (!base.Pawn.IsHashIntervalTick(60))
			{
				return;
			}
			if (base.Pawn.RaceProps.Humanlike)
			{
				if (base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.humanMentalState && Rand.MTBEventOccurs(Props.mtbDaysToCauseMentalState, 60000f, 60f) && base.Pawn.Awake() && base.Pawn.mindState.mentalStateHandler.TryStartMentalState(Props.humanMentalState, parent.def.LabelCap, forceWake: false, causedByMood: false, null, transitionSilently: true) && base.Pawn.Spawned)
				{
					SendLetter(Props.humanMentalState);
				}
			}
			else if (base.Pawn.RaceProps.Animal && base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.animalMentalState && (Props.animalMentalStateAlias == null || base.Pawn.mindState.mentalStateHandler.CurStateDef != Props.animalMentalStateAlias) && Rand.MTBEventOccurs(Props.mtbDaysToCauseMentalState, 60000f, 60f) && base.Pawn.Awake() && base.Pawn.mindState.mentalStateHandler.TryStartMentalState(Props.animalMentalState, parent.def.LabelCap, forceWake: false, causedByMood: false, null, transitionSilently: true) && base.Pawn.Spawned)
			{
				SendLetter(Props.animalMentalState);
			}
		}

		public override void CompPostPostRemoved()
		{
			if (Props.endMentalStateOnCure && ((base.Pawn.RaceProps.Humanlike && base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.humanMentalState) || (base.Pawn.RaceProps.Animal && (base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalState || base.Pawn.mindState.mentalStateHandler.CurStateDef == Props.animalMentalStateAlias))) && !base.Pawn.mindState.mentalStateHandler.CurState.causedByMood)
			{
				base.Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
			}
		}

		private void SendLetter(MentalStateDef mentalStateDef)
		{
			Find.LetterStack.ReceiveLetter((mentalStateDef.beginLetterLabel ?? ((string)mentalStateDef.LabelCap)).CapitalizeFirst() + ": " + base.Pawn.LabelShortCap, base.Pawn.mindState.mentalStateHandler.CurState.GetBeginLetterText() + "\n\n" + "CausedByHediff".Translate(parent.LabelCap), Props.letterDef, base.Pawn);
		}
	}
}
