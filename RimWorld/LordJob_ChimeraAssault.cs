using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_ChimeraAssault : LordJob
{
	private class Trigger_ChimeraHarmed : Trigger_PawnHarmed
	{
		public Trigger_ChimeraHarmed(float chance = 1f, bool requireInstigatorWithFaction = false, Faction requireInstigatorWithSpecificFaction = null, DutyDef skipDuty = null, int? minTicks = null)
			: base(chance, requireInstigatorWithFaction, requireInstigatorWithSpecificFaction, skipDuty, minTicks)
		{
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (lord.LordJob is LordJob_ChimeraAssault { CanSwitchMode: false })
			{
				return false;
			}
			return base.ActivateOn(lord, signal);
		}
	}

	private int currentModeStartedTick;

	private const float StalkToAttackMTBDays = 0.7f;

	private const float AttackToStalkMTBHours = 8f;

	private const float ChanceToFleeOnDown = 0.075f;

	private const float MinTimeInModeTicks = 7500f;

	private const int MinTicksFleeing = 2500;

	public const string StalkToAttackMemo = "StalkToAttack";

	private const string AttackToStalkMemo = "AttackToStalk";

	private bool InAttackMode => lord.CurLordToil is LordToil_ChimeraAttack;

	private bool CanSwitchMode => (float)(Find.TickManager.TicksGame - currentModeStartedTick) > 7500f;

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_ChimeraStalk lordToil_ChimeraStalk = (LordToil_ChimeraStalk)(stateGraph.StartingToil = new LordToil_ChimeraStalk());
		LordToil_ChimeraAttack lordToil_ChimeraAttack = new LordToil_ChimeraAttack();
		stateGraph.AddToil(lordToil_ChimeraAttack);
		Transition transition = new Transition(lordToil_ChimeraStalk, lordToil_ChimeraAttack);
		transition.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			currentModeStartedTick = Find.TickManager.TicksGame;
			SendAttackingLetter();
		}));
		transition.triggers.Add(new Trigger_Memo("StalkToAttack"));
		transition.triggers.Add(new Trigger_ChimeraHarmed(1f, requireInstigatorWithFaction: true, null, DutyDefOf.ChimeraStalkFlee, 2500));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_ChimeraAttack, lordToil_ChimeraStalk);
		transition2.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			currentModeStartedTick = Find.TickManager.TicksGame;
			SendModeChangeMessage("MessageChimeraWithdrawing".Translate());
		}));
		transition2.triggers.Add(new Trigger_Memo("AttackToStalk"));
		stateGraph.AddTransition(transition2);
		return stateGraph;
	}

	public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		return reason != PawnLostCondition.Incapped;
	}

	public override void LordJobTick()
	{
		base.LordJobTick();
		if (CanSwitchMode)
		{
			if (InAttackMode && Rand.MTBEventOccurs(8f, 2500f, 1f))
			{
				lord.ReceiveMemo("AttackToStalk");
			}
			else if (!InAttackMode && Rand.MTBEventOccurs(0.7f, 60000f, 1f))
			{
				lord.ReceiveMemo("StalkToAttack");
			}
		}
	}

	public void SwitchMode()
	{
		if (lord == null)
		{
			return;
		}
		LordToil curLordToil = lord.CurLordToil;
		if (!(curLordToil is LordToil_ChimeraStalk))
		{
			if (curLordToil is LordToil_ChimeraAttack)
			{
				lord.ReceiveMemo("AttackToStalk");
			}
			else
			{
				Log.Error($"Chimera lord job tried switching from a toil which is not handled {lord.CurLordToil}");
			}
		}
		else
		{
			lord.ReceiveMemo("StalkToAttack");
		}
		currentModeStartedTick = Find.TickManager.TicksGame;
	}

	public override void Notify_PawnDowned(Pawn p)
	{
		base.Notify_PawnDowned(p);
		if (CanSwitchMode && Rand.Chance(0.075f) && lord.CurLordToil is LordToil_ChimeraAttack)
		{
			SwitchMode();
		}
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		if (CanSwitchMode && condition == PawnLostCondition.Killed && Rand.Chance(0.075f) && lord.CurLordToil is LordToil_ChimeraAttack)
		{
			SwitchMode();
		}
	}

	private void SendModeChangeMessage(string verb)
	{
		if (!NoActivePawns())
		{
			Messages.Message((lord.ownedPawns.Count > 1) ? "MessageChimeraModeChangePlural".Translate(verb) : "MessageChimeraModeChangeSingular".Translate(verb), new LookTargets(lord.ownedPawns), MessageTypeDefOf.NeutralEvent);
		}
	}

	private void SendAttackingLetter()
	{
		if (!NoActivePawns())
		{
			TaggedString label = "LetterChimerasAttackingLabel".Translate();
			TaggedString text = "LetterChimerasAttacking".Translate();
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatBig, new LookTargets(lord.ownedPawns));
		}
	}

	private bool NoActivePawns()
	{
		if (lord.ownedPawns.Count == 0)
		{
			return true;
		}
		bool flag = false;
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			flag = !ownedPawn.Downed;
		}
		if (!flag)
		{
			return true;
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref currentModeStartedTick, "currentModeStartedTick", 0);
	}
}
