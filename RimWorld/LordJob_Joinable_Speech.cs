using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Joinable_Speech : LordJob_Joinable_Gathering
	{
		public const float DurationHours = 4f;

		public static readonly Dictionary<ThoughtDef, float> OutcomeThoughtChances = new Dictionary<ThoughtDef, float>
		{
			{
				ThoughtDefOf.TerribleSpeech,
				0.05f
			},
			{
				ThoughtDefOf.UninspiringSpeech,
				0.15f
			},
			{
				ThoughtDefOf.EncouragingSpeech,
				0.6f
			},
			{
				ThoughtDefOf.InspirationalSpeech,
				0.2f
			}
		};

		private static readonly float InspirationChanceFromInspirationalSpeech = 0.05f;

		private static List<Tuple<ThoughtDef, float>> outcomeChancesTemp = new List<Tuple<ThoughtDef, float>>();

		public override bool AllowStartNewGatherings => false;

		public override bool OrganizerIsStartingPawn => true;

		public LordJob_Joinable_Speech()
		{
		}

		public LordJob_Joinable_Speech(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
			: base(spot, organizer, gatheringDef)
		{
		}

		protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
		{
			return new LordToil_Speech(spot, gatheringDef, organizer);
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil lordToil = CreateGatheringToil(spot, organizer, gatheringDef);
			stateGraph.AddToil(lordToil);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			float speechDuration = 10000f;
			Transition transition = new Transition(lordToil, lordToil_End);
			transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
			transition.AddTrigger(new Trigger_PawnKilled());
			transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.LeftVoluntarily, organizer));
			transition.AddPreAction(new TransitionAction_Custom((Action)delegate
			{
				ApplyOutcome((float)lord.ticksInToil / speechDuration);
			}));
			stateGraph.AddTransition(transition);
			timeoutTrigger = new Trigger_TicksPassedAfterConditionMet((int)speechDuration, () => GatheringsUtility.InGatheringArea(organizer.Position, spot, organizer.Map), 60);
			Transition transition2 = new Transition(lordToil, lordToil_End);
			transition2.AddTrigger(timeoutTrigger);
			transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
			{
				ApplyOutcome(1f);
			}));
			stateGraph.AddTransition(transition2);
			return stateGraph;
		}

		public override string GetReport(Pawn pawn)
		{
			if (pawn != organizer)
			{
				return "LordReportListeningSpeech".Translate(organizer.Named("ORGANIZER"));
			}
			return "LordReportGivingSpeech".Translate();
		}

		protected virtual void ApplyOutcome(float progress)
		{
			if (progress < 0.5f)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelSpeechCancelled".Translate(), "LetterSpeechCancelled".Translate(organizer.Named("ORGANIZER")).CapitalizeFirst(), LetterDefOf.NegativeEvent, organizer);
				return;
			}
			ThoughtDef key = OutcomeThoughtChances.RandomElementByWeight((KeyValuePair<ThoughtDef, float> t) => (!PositiveOutcome(t.Key)) ? t.Value : (t.Value * organizer.GetStatValue(StatDefOf.SocialImpact) * progress)).Key;
			string text = "";
			foreach (Pawn ownedPawn in lord.ownedPawns)
			{
				if (ownedPawn == organizer || !organizer.Position.InHorDistOf(ownedPawn.Position, 18f))
				{
					continue;
				}
				ownedPawn.needs.mood.thoughts.memories.TryGainMemory(key, organizer);
				if (key == ThoughtDefOf.InspirationalSpeech && Rand.Chance(InspirationChanceFromInspirationalSpeech))
				{
					InspirationDef randomAvailableInspirationDef = ownedPawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
					if (randomAvailableInspirationDef != null && ownedPawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(randomAvailableInspirationDef, "LetterSpeechInspiration".Translate(ownedPawn.Named("PAWN"), organizer.Named("SPEAKER"))))
					{
						text = text + "  - " + ownedPawn.NameShortColored.Resolve() + "\n";
					}
				}
			}
			TaggedString text2 = "LetterFinishedSpeech".Translate(organizer.Named("ORGANIZER")).CapitalizeFirst() + " " + ("Letter" + key.defName).Translate();
			if (!text.NullOrEmpty())
			{
				text2 += "\n\n" + "LetterSpeechInspiredListeners".Translate() + "\n\n" + text.TrimEndNewlines();
			}
			if (progress < 1f)
			{
				text2 += "\n\n" + "LetterSpeechInterrupted".Translate(progress.ToStringPercent(), organizer.Named("ORGANIZER"));
			}
			Find.LetterStack.ReceiveLetter(key.stages[0].LabelCap, text2, PositiveOutcome(key) ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent, organizer);
			Ability ability = organizer.abilities.GetAbility(AbilityDefOf.Speech);
			RoyalTitle mostSeniorTitle = organizer.royalty.MostSeniorTitle;
			if (ability != null && mostSeniorTitle != null)
			{
				ability.StartCooldown(mostSeniorTitle.def.speechCooldown.RandomInRange);
			}
		}

		private static bool PositiveOutcome(ThoughtDef outcome)
		{
			if (outcome != ThoughtDefOf.EncouragingSpeech)
			{
				return outcome == ThoughtDefOf.InspirationalSpeech;
			}
			return true;
		}

		public static IEnumerable<Tuple<ThoughtDef, float>> OutcomeChancesForPawn(Pawn p)
		{
			outcomeChancesTemp.Clear();
			float num = 1f / OutcomeThoughtChances.Sum((KeyValuePair<ThoughtDef, float> c) => (!PositiveOutcome(c.Key)) ? c.Value : (c.Value * p.GetStatValue(StatDefOf.SocialImpact)));
			foreach (KeyValuePair<ThoughtDef, float> outcomeThoughtChance in OutcomeThoughtChances)
			{
				outcomeChancesTemp.Add(new Tuple<ThoughtDef, float>(outcomeThoughtChance.Key, (PositiveOutcome(outcomeThoughtChance.Key) ? (outcomeThoughtChance.Value * p.GetStatValue(StatDefOf.SocialImpact)) : outcomeThoughtChance.Value) * num));
			}
			return outcomeChancesTemp;
		}
	}
}
