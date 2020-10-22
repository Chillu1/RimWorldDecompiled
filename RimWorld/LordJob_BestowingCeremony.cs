using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_BestowingCeremony : LordJob
	{
		public const int ExpirationTicks = 30000;

		private const string MemoCeremonyFinished = "CeremonyFinished";

		public const int WaitTimeTicks = 600;

		public Pawn bestower;

		public Pawn target;

		public LocalTargetInfo spot;

		public IntVec3 spotCell;

		public Thing shuttle;

		public string questEndedSignal;

		private LordToil exitToil;

		public override bool AlwaysShowWeapon => true;

		public LordJob_BestowingCeremony()
		{
		}

		public LordJob_BestowingCeremony(Pawn bestower, Pawn target, LocalTargetInfo spot, IntVec3 spotCell, Thing shuttle = null, string questEndedSignal = null)
		{
			this.bestower = bestower;
			this.target = target;
			this.spot = spot;
			this.spotCell = spotCell;
			this.shuttle = shuttle;
			this.questEndedSignal = questEndedSignal;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Bestowing ceremony is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 3454535);
				return stateGraph;
			}
			LordToil_Wait lordToil_Wait = new LordToil_Wait();
			stateGraph.AddToil(lordToil_Wait);
			LordToil_Wait lordToil_Wait2 = new LordToil_Wait();
			stateGraph.AddToil(lordToil_Wait2);
			LordToil_Wait lordToil_Wait3 = new LordToil_Wait();
			stateGraph.AddToil(lordToil_Wait3);
			LordToil_BestowingCeremony_MoveInPlace lordToil_BestowingCeremony_MoveInPlace = new LordToil_BestowingCeremony_MoveInPlace(spotCell, target);
			stateGraph.AddToil(lordToil_BestowingCeremony_MoveInPlace);
			LordToil_BestowingCeremony_Wait lordToil_BestowingCeremony_Wait = new LordToil_BestowingCeremony_Wait(target);
			stateGraph.AddToil(lordToil_BestowingCeremony_Wait);
			exitToil = ((shuttle == null) ? ((LordToil)new LordToil_ExitMap(LocomotionUrgency.Walk)) : ((LordToil)new LordToil_EnterShuttleOrLeave(shuttle, LocomotionUrgency.Walk, canDig: true, interruptCurrentJob: true)));
			stateGraph.AddToil(exitToil);
			Transition transition = new Transition(lordToil_Wait, lordToil_Wait2);
			transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Spawned));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Wait2, lordToil_BestowingCeremony_MoveInPlace);
			transition2.AddTrigger(new Trigger_TicksPassed(600));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_BestowingCeremony_MoveInPlace, lordToil_BestowingCeremony_Wait);
			transition3.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Position == spotCell));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_BestowingCeremony_Wait, exitToil);
			transition4.AddTrigger(new Trigger_TicksPassed(30000));
			transition4.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyExpired", lord.Named("SUBJECT"));
			}));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_BestowingCeremony_Wait, lordToil_Wait3);
			transition5.AddTrigger(new Trigger_Memo("CeremonyFinished"));
			transition5.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyDone", lord.Named("SUBJECT"));
			}));
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_Wait3, exitToil);
			transition6.AddTrigger(new Trigger_TicksPassed(600));
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
			transition7.AddSource(lordToil_BestowingCeremony_Wait);
			transition7.AddTrigger(new Trigger_BecamePlayerEnemy());
			stateGraph.AddTransition(transition7);
			Transition transition8 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
			transition8.AddSource(lordToil_BestowingCeremony_Wait);
			transition8.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Spawned && !bestower.CanReach(spotCell, PathEndMode.OnCell, Danger.Deadly)));
			transition8.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				Messages.Message("MessageBestowingSpotUnreachable".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
				QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyFailed", lord.Named("SUBJECT"));
			}));
			stateGraph.AddTransition(transition8);
			if (!questEndedSignal.NullOrEmpty())
			{
				Transition transition9 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
				transition9.AddSource(lordToil_BestowingCeremony_Wait);
				transition9.AddSource(lordToil_Wait);
				transition9.AddSource(lordToil_Wait2);
				transition9.AddTrigger(new Trigger_Signal(questEndedSignal));
				stateGraph.AddTransition(transition9);
			}
			return stateGraph;
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			if (p == bestower)
			{
				MakeCeremonyFail();
			}
		}

		public void MakeCeremonyFail()
		{
			QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyFailed", lord.Named("SUBJECT"));
		}

		private bool CanUseSpot(IntVec3 spot)
		{
			if (!spot.InBounds(bestower.Map))
			{
				return false;
			}
			if (!spot.Standable(bestower.Map))
			{
				return false;
			}
			if (!GenSight.LineOfSight(spot, bestower.Position, bestower.Map))
			{
				return false;
			}
			if (!bestower.CanReach(this.spot, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			return true;
		}

		private bool TryGetUsableSpotAdjacentToBestower(out IntVec3 pos)
		{
			foreach (int item in Enumerable.Range(1, 4).InRandomOrder())
			{
				IntVec3 intVec = bestower.Position + GenRadial.ManualRadialPattern[item];
				if (CanUseSpot(intVec))
				{
					pos = intVec;
					return true;
				}
			}
			pos = IntVec3.Zero;
			return false;
		}

		public void StartCeremony(Pawn pawn)
		{
			if (!JobDriver_BestowingCeremony.AnalyzeThroneRoom(bestower, target))
			{
				Messages.Message("BestowingCeremonyThroneroomRequirementsNotSatisfied".Translate(target.Named("PAWN"), target.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, target.royalty.GetFavor(bestower.Faction)).label.Named("TITLE")), target, MessageTypeDefOf.NegativeEvent);
				((LordJob_BestowingCeremony)bestower.GetLord().LordJob).MakeCeremonyFail();
			}
			IntVec3 pos = IntVec3.Invalid;
			if (spot.Thing != null)
			{
				IntVec3 interactionCell = spot.Thing.InteractionCell;
				IntVec3 intVec = spotCell;
				foreach (IntVec3 item in GenSight.PointsOnLineOfSight(interactionCell, intVec))
				{
					if (!(item == interactionCell) && !(item == intVec) && CanUseSpot(item))
					{
						pos = item;
						break;
					}
				}
			}
			if (!pos.IsValid && !TryGetUsableSpotAdjacentToBestower(out pos))
			{
				Messages.Message("MessageBestowerUnreachable".Translate(), bestower, MessageTypeDefOf.CautionInput);
				return;
			}
			Job job = JobMaker.MakeJob(JobDefOf.BestowingCeremony, bestower, pos);
			pawn.jobs.TryTakeOrderedJob(job);
		}

		public void FinishCeremony(Pawn pawn)
		{
			lord.ReceiveMemo("CeremonyFinished");
			RoyalTitleDef currentTitle = target.royalty.GetCurrentTitle(bestower.Faction);
			RoyalTitleDef titleAwardedWhenUpdating = target.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, target.royalty.GetFavor(bestower.Faction));
			Pawn_RoyaltyTracker.MakeLetterTextForTitleChange(target, bestower.Faction, currentTitle, titleAwardedWhenUpdating, out var headline, out var body);
			if (pawn.royalty != null)
			{
				pawn.royalty.TryUpdateTitle_NewTemp(bestower.Faction, sendLetter: false, titleAwardedWhenUpdating);
			}
			Hediff_Psylink mainPsylinkSource = target.GetMainPsylinkSource();
			List<AbilityDef> abilitiesPreUpdate = ((mainPsylinkSource == null) ? new List<AbilityDef>() : pawn.abilities.abilities.Select((Ability a) => a.def).ToList());
			ThingOwner<Thing> innerContainer = bestower.inventory.innerContainer;
			for (int i = pawn.GetPsylinkLevel(); i < pawn.GetMaxPsylinkLevelByTitle(); i++)
			{
				for (int num = innerContainer.Count - 1; num >= 0; num--)
				{
					if (innerContainer[num].def == ThingDefOf.PsychicAmplifier)
					{
						Thing thing = innerContainer[num];
						innerContainer.RemoveAt(num);
						thing.Destroy();
						break;
					}
				}
				pawn.ChangePsylinkLevel(1, sendLetter: false);
			}
			mainPsylinkSource = target.GetMainPsylinkSource();
			List<AbilityDef> newAbilities = ((mainPsylinkSource == null) ? new List<AbilityDef>() : (from a in pawn.abilities.abilities
				select a.def into def
				where !abilitiesPreUpdate.Contains(def)
				select def).ToList());
			string str = headline;
			str = str + "\n\n" + Hediff_Psylink.MakeLetterTextNewPsylinkLevel(target, pawn.GetPsylinkLevel(), newAbilities);
			str = str + "\n\n" + body;
			Find.LetterStack.ReceiveLetter("LetterLabelGainedRoyalTitle".Translate(titleAwardedWhenUpdating.GetLabelCapFor(pawn).Named("TITLE"), pawn.Named("PAWN")), str, LetterDefOf.PositiveEvent, pawn, bestower.Faction);
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref bestower, "bestower");
			Scribe_References.Look(ref target, "target");
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_TargetInfo.Look(ref spot, "spot");
			Scribe_Values.Look(ref questEndedSignal, "questEndedSignal");
			Scribe_Values.Look(ref spotCell, "spotCell");
		}
	}
}
