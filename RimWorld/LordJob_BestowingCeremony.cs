using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class LordJob_BestowingCeremony : LordJob_Ritual
{
	public const int ExpirationTicks = 30000;

	public static readonly string MemoCeremonyStarted = "CeremonyStarted";

	private const string MemoCeremonyFinished = "CeremonyFinished";

	public const int WaitTimeTicks = 600;

	public Pawn bestower;

	public Pawn target;

	public LocalTargetInfo targetSpot;

	public IntVec3 spotCell;

	public Thing shuttle;

	public string questEndedSignal;

	public List<Pawn> colonistParticipants = new List<Pawn>();

	public bool ceremonyStarted;

	private LordToil_BestowingCeremony_Perform ceremonyToil;

	private LordToil exitToil;

	private RitualOutcomeEffectWorker_Bestowing outcome;

	private const float HeatstrokeHypothermiaMinSeverityForLeaving = 0.35f;

	private const int HeatstrokeHypothermiaGoodwillOffset = -50;

	private const float GasExposireSeverityForLeaving = 0.9f;

	private Texture2D icon;

	private Dictionary<Pawn, int> totalPresenceTmp = new Dictionary<Pawn, int>();

	public override bool AlwaysShowWeapon => true;

	public override IntVec3 Spot => targetSpot.Cell;

	public override string RitualLabel => "BestowingCeremonyLabel".Translate().CapitalizeFirst();

	public override bool AllowStartNewGatherings => lord.CurLordToil != ceremonyToil;

	public Texture2D Icon
	{
		get
		{
			if (icon == null)
			{
				icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/BestowCeremony");
			}
			return icon;
		}
	}

	public override IEnumerable<Pawn> PawnsToCountTowardsPresence => lord.ownedPawns.Where((Pawn p) => p != bestower && p != target && p.IsColonist);

	public LordJob_BestowingCeremony()
	{
	}

	public LordJob_BestowingCeremony(Pawn bestower, Pawn target, LocalTargetInfo targetSpot, IntVec3 spotCell, Thing shuttle = null, string questEndedSignal = null)
	{
		this.bestower = bestower;
		this.target = target;
		this.targetSpot = targetSpot;
		this.spotCell = spotCell;
		this.shuttle = shuttle;
		this.questEndedSignal = questEndedSignal;
	}

	public override AcceptanceReport AllowsDrafting(Pawn pawn)
	{
		if (lord.CurLordToil == ceremonyToil)
		{
			return new AcceptanceReport("ParticipatingInRitual".Translate(pawn, RitualLabel));
		}
		return true;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
		if (!ModLister.CheckRoyalty("Bestowing ceremony"))
		{
			return stateGraph;
		}
		outcome = (RitualOutcomeEffectWorker_Bestowing)RitualOutcomeEffectDefOf.BestowingCeremony.GetInstance();
		LordToil_Wait lordToil_Wait = new LordToil_Wait();
		stateGraph.AddToil(lordToil_Wait);
		LordToil_Wait lordToil_Wait2 = new LordToil_Wait();
		stateGraph.AddToil(lordToil_Wait2);
		LordToil_Wait lordToil_Wait3 = new LordToil_Wait();
		stateGraph.AddToil(lordToil_Wait3);
		LordToil_Wait lordToil_Wait4 = new LordToil_Wait();
		stateGraph.AddToil(lordToil_Wait4);
		LordToil_BestowingCeremony_MoveInPlace lordToil_BestowingCeremony_MoveInPlace = new LordToil_BestowingCeremony_MoveInPlace(spotCell, target);
		stateGraph.AddToil(lordToil_BestowingCeremony_MoveInPlace);
		LordToil_BestowingCeremony_Wait lordToil_BestowingCeremony_Wait = new LordToil_BestowingCeremony_Wait(target, bestower);
		stateGraph.AddToil(lordToil_BestowingCeremony_Wait);
		ceremonyToil = new LordToil_BestowingCeremony_Perform(target, bestower);
		stateGraph.AddToil(ceremonyToil);
		exitToil = ((shuttle == null) ? ((LordToil)new LordToil_ExitMap(LocomotionUrgency.Jog)) : ((LordToil)new LordToil_EnterShuttleOrLeave(shuttle, LocomotionUrgency.Jog, canDig: true, interruptCurrentJob: true)));
		stateGraph.AddToil(exitToil);
		TransitionAction_Custom action = new TransitionAction_Custom((Action)delegate
		{
			lord.RemovePawns(colonistParticipants);
		});
		Transition transition = new Transition(lordToil_Wait, lordToil_Wait2);
		transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Spawned));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_Wait2, lordToil_BestowingCeremony_MoveInPlace);
		transition2.AddTrigger(new Trigger_TicksPassed(600));
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_BestowingCeremony_MoveInPlace, lordToil_BestowingCeremony_Wait);
		transition3.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Position == spotCell));
		stateGraph.AddTransition(transition3);
		Transition transition4 = new Transition(lordToil_BestowingCeremony_Wait, ceremonyToil);
		transition4.AddTrigger(new Trigger_Memo(MemoCeremonyStarted));
		transition4.postActions.Add(new TransitionAction_Custom((Action)delegate
		{
			ceremonyStarted = true;
		}));
		stateGraph.AddTransition(transition4);
		Transition transition5 = new Transition(ceremonyToil, lordToil_Wait4);
		transition5.AddTrigger(new Trigger_Custom((TriggerSignal s) => s.type == TriggerSignalType.Tick && bestower.InMentalState));
		transition5.AddPreAction(action);
		transition5.postActions.Add(new TransitionAction_Custom((Action)delegate
		{
			ceremonyStarted = false;
			lord.RemovePawn(target);
		}));
		transition5.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			Messages.Message("MessageBestowingInterrupted".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
		}));
		stateGraph.AddTransition(transition5);
		Transition transition6 = new Transition(lordToil_Wait4, lordToil_BestowingCeremony_Wait);
		transition6.AddTrigger(new Trigger_Custom((TriggerSignal s) => s.type == TriggerSignalType.Tick && !bestower.InMentalState));
		stateGraph.AddTransition(transition6);
		Transition transition7 = new Transition(lordToil_BestowingCeremony_Wait, exitToil);
		transition7.AddTrigger(new Trigger_TicksPassed(30000));
		transition7.AddPreAction(action);
		transition7.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyExpired", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition7);
		Transition transition8 = new Transition(ceremonyToil, exitToil);
		transition8.AddTrigger(new Trigger_Signal(questEndedSignal));
		transition8.AddPreAction(action);
		transition8.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			Messages.Message("MessageBestowingInterrupted".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
		}));
		stateGraph.AddTransition(transition8);
		Transition transition9 = new Transition(ceremonyToil, lordToil_Wait3);
		transition9.AddTrigger(new Trigger_Memo("CeremonyFinished"));
		transition9.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyDone", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition9);
		Transition transition10 = new Transition(lordToil_Wait3, exitToil);
		transition10.AddPreAction(action);
		transition10.AddTrigger(new Trigger_TicksPassed(600));
		stateGraph.AddTransition(transition10);
		Transition transition11 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
		transition11.AddSource(lordToil_BestowingCeremony_Wait);
		transition11.AddTrigger(new Trigger_BecamePlayerEnemy());
		transition11.AddPreAction(action);
		stateGraph.AddTransition(transition11);
		Transition transition12 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
		transition12.AddSource(lordToil_BestowingCeremony_Wait);
		transition12.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && bestower.Spawned && !bestower.CanReach(spotCell, PathEndMode.OnCell, Danger.Deadly)));
		transition12.AddPreAction(action);
		transition12.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			Messages.Message("MessageBestowingSpotUnreachable".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
			QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyFailed", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition12);
		if (!questEndedSignal.NullOrEmpty())
		{
			Transition transition13 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
			transition13.AddSource(lordToil_BestowingCeremony_Wait);
			transition13.AddSource(lordToil_Wait);
			transition13.AddSource(lordToil_Wait2);
			transition13.AddTrigger(new Trigger_Signal(questEndedSignal));
			transition13.AddPreAction(action);
			stateGraph.AddTransition(transition13);
		}
		Transition transition14 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
		transition14.AddSource(lordToil_BestowingCeremony_Wait);
		transition14.AddTrigger(new Trigger_Custom(delegate(TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && !bestower.Dead)
			{
				Hediff firstHediffOfDef = bestower.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
				if (firstHediffOfDef != null && firstHediffOfDef.Severity >= 0.35f)
				{
					return true;
				}
				Hediff firstHediffOfDef2 = bestower.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke);
				if (firstHediffOfDef2 != null && firstHediffOfDef2.Severity >= 0.35f)
				{
					return true;
				}
			}
			return false;
		}));
		transition14.AddPreAction(action);
		transition14.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			compShuttle?.SetPawnToLeaveBehind((Pawn p) => p != bestower);
			bestower.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -50);
			Messages.Message("MessageBestowingDangerTemperature".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
			QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition14);
		Transition transition15 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
		transition15.AddSource(lordToil_BestowingCeremony_Wait);
		transition15.AddTrigger(new Trigger_Custom(delegate(TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && !bestower.Dead)
			{
				if (ModsConfig.BiotechActive)
				{
					Hediff firstHediffOfDef = bestower.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxGasExposure);
					if (firstHediffOfDef != null && firstHediffOfDef.Severity >= 0.9f)
					{
						return true;
					}
				}
				Hediff firstHediffOfDef2 = bestower.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.LungRotExposure);
				if (firstHediffOfDef2 != null && firstHediffOfDef2.Severity >= 0.9f)
				{
					return true;
				}
			}
			return false;
		}));
		transition15.AddPreAction(action);
		transition15.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			compShuttle?.SetPawnToLeaveBehind((Pawn p) => p != bestower);
			bestower.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -50);
			Messages.Message("MessageBestowingDangerGas".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
			QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition15);
		Transition transition16 = new Transition(lordToil_BestowingCeremony_MoveInPlace, exitToil);
		transition16.AddSource(lordToil_BestowingCeremony_Wait);
		transition16.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: true, base.Map.ParentFaction));
		transition16.AddPreAction(action);
		transition16.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			compShuttle?.SetPawnToLeaveBehind((Pawn p) => p != bestower);
			Messages.Message("MessageBestowingDanger".Translate(), bestower, MessageTypeDefOf.NegativeEvent);
			QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition16);
		return stateGraph;
	}

	public override void Notify_InMentalState(Pawn pawn, MentalStateDef stateDef)
	{
		if (stateDef != MentalStateDefOf.SocialFighting)
		{
			lord.Notify_PawnLost(pawn, PawnLostCondition.InMentalState);
		}
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		if (p == bestower || p == target)
		{
			MakeCeremonyFail();
		}
	}

	public void MakeCeremonyFail()
	{
		QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyFailed", lord.Named("SUBJECT"));
		outcome.ResetCompDatas();
	}

	public override void LordJobTick()
	{
		if (ritual != null && ritual.behavior != null)
		{
			ritual.behavior.Tick(this);
		}
		if (ceremonyStarted)
		{
			outcome?.Tick(this);
		}
		if (lord.ownedPawns.Count == 0)
		{
			base.Map.lordManager.RemoveLord(lord);
		}
	}

	private static bool CanUseSpot(Pawn bestower, IntVec3 spot)
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
		if (!bestower.CanReach(spot, PathEndMode.OnCell, Danger.Deadly))
		{
			return false;
		}
		return true;
	}

	public static IntVec3 TryGetUsableSpotAdjacentToBestower(Pawn bestower)
	{
		foreach (int item in Enumerable.Range(1, 4).InRandomOrder())
		{
			IntVec3 result = bestower.Position + GenRadial.ManualRadialPattern[item];
			if (CanUseSpot(bestower, result))
			{
				return result;
			}
		}
		return IntVec3.Invalid;
	}

	public IntVec3 GetSpot()
	{
		IntVec3 result = IntVec3.Invalid;
		if (targetSpot.Thing != null)
		{
			IntVec3 interactionCell = targetSpot.Thing.InteractionCell;
			IntVec3 intVec = spotCell;
			foreach (IntVec3 item in GenSight.PointsOnLineOfSight(interactionCell, intVec))
			{
				if (!(item == interactionCell) && !(item == intVec) && CanUseSpot(bestower, item))
				{
					result = item;
					break;
				}
			}
		}
		if (result.IsValid)
		{
			return result;
		}
		return TryGetUsableSpotAdjacentToBestower(bestower);
	}

	public override string GetReport(Pawn pawn)
	{
		return "LordReportAttending".Translate("BestowingCeremonyLabel".Translate());
	}

	public void FinishCeremony(Pawn pawn)
	{
		lord.ReceiveMemo("CeremonyFinished");
		totalPresenceTmp.Clear();
		foreach (KeyValuePair<Pawn, int> presentForTick in ceremonyToil.Data.presentForTicks)
		{
			if (presentForTick.Key != null && !presentForTick.Key.Dead)
			{
				if (!totalPresenceTmp.ContainsKey(presentForTick.Key))
				{
					totalPresenceTmp.Add(presentForTick.Key, presentForTick.Value);
				}
				else
				{
					totalPresenceTmp[presentForTick.Key] += presentForTick.Value;
				}
			}
		}
		totalPresenceTmp.RemoveAll((KeyValuePair<Pawn, int> tp) => tp.Value < 2500);
		outcome.Apply(1f, totalPresenceTmp, this);
		outcome.ResetCompDatas();
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
	{
		if (p != bestower && p != target)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandLeaveBestowingCeremony".Translate();
			command_Action.defaultDesc = "CommandLeaveBestowingCeremonyDesc".Translate();
			command_Action.icon = Icon;
			command_Action.action = delegate
			{
				lord.Notify_PawnLost(p, PawnLostCondition.ForcedByPlayerAction);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			};
			command_Action.hotKey = KeyBindingDefOf.Misc5;
			yield return command_Action;
		}
		else
		{
			if (!ceremonyStarted)
			{
				yield break;
			}
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "CommandCancelBestowingCeremony".Translate();
			command_Action2.defaultDesc = "CommandCancelBestowingCeremonyDesc".Translate();
			command_Action2.icon = Icon;
			command_Action2.action = delegate
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandCancelBestowingCeremonyConfirm".Translate(), delegate
				{
					QuestUtility.SendQuestTargetSignals(lord.questTags, "CeremonyFailed", lord.Named("SUBJECT"));
				}));
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			};
			command_Action2.hotKey = KeyBindingDefOf.Misc6;
			yield return command_Action2;
		}
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref bestower, "bestower");
		Scribe_References.Look(ref target, "target");
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_TargetInfo.Look(ref targetSpot, "targetSpot");
		Scribe_Values.Look(ref questEndedSignal, "questEndedSignal");
		Scribe_Values.Look(ref spotCell, "spotCell");
		Scribe_Values.Look(ref ceremonyStarted, "ceremonyStarted", defaultValue: false);
		Scribe_Collections.Look(ref colonistParticipants, "colonistParticipants", LookMode.Reference);
	}
}
