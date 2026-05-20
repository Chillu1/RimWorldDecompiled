using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_RefugeeInteractions : QuestPartActivable
{
	private enum InteractionResponseType
	{
		AssaultColony,
		Leave,
		BadThought
	}

	public string inSignalDestroyed;

	public string inSignalArrested;

	public string inSignalSurgeryViolation;

	public string inSignalPsychicRitualTarget;

	public string inSignalRecruited;

	public string inSignalKidnapped;

	public string inSignalAssaultColony;

	public string inSignalLeftMap;

	public string inSignalLeftBehind;

	public string inSignalBanished;

	public string outSignalDestroyed_AssaultColony;

	public string outSignalDestroyed_LeaveColony;

	public string outSignalDestroyed_BadThought;

	public string outSignalArrested_AssaultColony;

	public string outSignalArrested_LeaveColony;

	public string outSignalArrested_BadThought;

	public string outSignalLeftBehind_LeaveColony;

	public string outSignalLeftBehind_BadThought;

	public string outSignalSurgeryViolation_AssaultColony;

	public string outSignalSurgeryViolation_LeaveColony;

	public string outSignalSurgeryViolation_BadThought;

	public string outSignalPsychicRitualTarget_AssaultColony;

	public string outSignalPsychicRitualTarget_LeaveColony;

	public string outSignalPsychicRitualTarget_BadThought;

	public string outSignalLast_Arrested;

	public string outSignalLast_Destroyed;

	public string outSignalLast_Kidnapped;

	public string outSignalLast_Recruited;

	public string outSignalLast_LeftMapAllHealthy;

	public string outSignalLast_LeftMapAllNotHealthy;

	public string outSignalLast_Banished;

	public string outSignalLast_LeftBehind;

	public List<Pawn> pawns = new List<Pawn>();

	public Faction faction;

	public MapParent mapParent;

	public int pawnsLeftUnhealthy;

	protected override void ProcessQuestSignal(Signal signal)
	{
		if (signal.tag == inSignalRecruited && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
		{
			pawns.Remove(arg);
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_Recruited, signal.args));
			}
		}
		if (signal.tag == inSignalKidnapped && signal.args.TryGetArg("SUBJECT", out Pawn arg2) && pawns.Contains(arg2))
		{
			pawns.Remove(arg2);
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_Kidnapped, signal.args));
			}
		}
		if (signal.tag == inSignalBanished && signal.args.TryGetArg("SUBJECT", out Pawn arg3) && pawns.Contains(arg3))
		{
			pawns.Remove(arg3);
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_Banished, signal.args));
			}
		}
		if (signal.tag == inSignalLeftMap && signal.args.TryGetArg("SUBJECT", out Pawn arg4) && pawns.Contains(arg4))
		{
			pawns.Remove(arg4);
			if (arg4.Destroyed || arg4.InMentalState || arg4.health.hediffSet.BleedRateTotal > 0.001f)
			{
				pawnsLeftUnhealthy++;
			}
			int num = pawns.Count((Pawn p) => p.Downed);
			if (pawns.Count - num <= 0)
			{
				if (pawnsLeftUnhealthy > 0 || num > 0)
				{
					pawns.Clear();
					pawnsLeftUnhealthy += num;
					Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllNotHealthy, signal.args));
				}
				else
				{
					Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllHealthy, signal.args));
				}
			}
		}
		if (signal.tag == inSignalLeftBehind && signal.args.TryGetArg("SUBJECT", out Pawn arg5) && pawns.Contains(arg5))
		{
			pawns.Remove(arg5);
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftBehind, signal.args));
			}
			else
			{
				signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
				switch (ChooseRandomInteraction())
				{
				case InteractionResponseType.AssaultColony:
				case InteractionResponseType.Leave:
					LeavePlayer();
					Find.SignalManager.SendSignal(new Signal(outSignalLeftBehind_LeaveColony, signal.args));
					break;
				case InteractionResponseType.BadThought:
					Find.SignalManager.SendSignal(new Signal(outSignalLeftBehind_BadThought, signal.args));
					break;
				}
			}
		}
		if (signal.tag == inSignalDestroyed && signal.args.TryGetArg("SUBJECT", out Pawn arg6) && pawns.Contains(arg6))
		{
			pawns.Remove(arg6);
			arg6.SetFaction(faction);
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_Destroyed, signal.args));
			}
			else
			{
				signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
				switch (ChooseRandomInteraction())
				{
				case InteractionResponseType.AssaultColony:
					AssaultColony(HistoryEventDefOf.QuestPawnLost);
					Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_AssaultColony, signal.args));
					break;
				case InteractionResponseType.Leave:
					LeavePlayer();
					Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_LeaveColony, signal.args));
					break;
				case InteractionResponseType.BadThought:
					Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_BadThought, signal.args));
					break;
				}
			}
		}
		if (signal.tag == inSignalArrested && signal.args.TryGetArg("SUBJECT", out Pawn arg7) && pawns.Contains(arg7))
		{
			pawns.Remove(arg7);
			bool inAggroMentalState = arg7.InAggroMentalState;
			if (pawns.Count == 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalLast_Arrested, signal.args));
			}
			else if (!inAggroMentalState)
			{
				signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
				switch (ChooseRandomInteraction())
				{
				case InteractionResponseType.AssaultColony:
					AssaultColony(HistoryEventDefOf.QuestPawnArrested);
					Find.SignalManager.SendSignal(new Signal(outSignalArrested_AssaultColony, signal.args));
					break;
				case InteractionResponseType.Leave:
					LeavePlayer();
					Find.SignalManager.SendSignal(new Signal(outSignalArrested_LeaveColony, signal.args));
					break;
				case InteractionResponseType.BadThought:
					Find.SignalManager.SendSignal(new Signal(outSignalArrested_BadThought, signal.args));
					break;
				}
			}
		}
		if (signal.tag == inSignalSurgeryViolation && signal.args.TryGetArg("SUBJECT", out Pawn arg8) && pawns.Contains(arg8))
		{
			signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
			switch (ChooseRandomInteraction())
			{
			case InteractionResponseType.AssaultColony:
				AssaultColony(HistoryEventDefOf.PerformedHarmfulSurgery);
				Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_AssaultColony, signal.args));
				break;
			case InteractionResponseType.Leave:
				LeavePlayer();
				Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_LeaveColony, signal.args));
				break;
			case InteractionResponseType.BadThought:
				Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_BadThought, signal.args));
				break;
			}
		}
		if (signal.tag == inSignalPsychicRitualTarget && signal.args.TryGetArg("SUBJECT", out Pawn arg9) && pawns.Contains(arg9))
		{
			signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
			switch (ChooseRandomInteraction())
			{
			case InteractionResponseType.AssaultColony:
				AssaultColony(HistoryEventDefOf.WasPsychicRitualTarget);
				Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_AssaultColony, signal.args));
				break;
			case InteractionResponseType.Leave:
				LeavePlayer();
				Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_LeaveColony, signal.args));
				break;
			case InteractionResponseType.BadThought:
				Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_BadThought, signal.args));
				break;
			}
		}
		if (inSignalAssaultColony != null && signal.tag == inSignalAssaultColony)
		{
			AssaultColony(null);
		}
	}

	private void LeavePlayer()
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			if (faction != pawns[i].Faction)
			{
				pawns[i].SetFaction(faction);
			}
		}
		LeaveQuestPartUtility.MakePawnsLeave(pawns, sendLetter: false, quest);
		Complete();
	}

	private void AssaultColony(HistoryEventDef reason)
	{
		if (faction.HasGoodwill)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(faction, Faction.OfPlayer.GoodwillToMakeHostile(faction), canSendMessage: true, canSendHostilityLetter: false, reason);
		}
		else
		{
			faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, canSendHostilityLetter: false);
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedByQuest);
		}
		for (int j = 0; j < pawns.Count; j++)
		{
			pawns[j].SetFaction(faction);
			if (!pawns[j].Awake())
			{
				RestUtility.WakeUp(pawns[j]);
			}
		}
		Dictionary<Map, Lord> dictionary = new Dictionary<Map, Lord>();
		for (int k = 0; k < pawns.Count; k++)
		{
			Pawn pawn = pawns[k];
			Map mapHeld = pawn.MapHeld;
			if (!pawn.Dead)
			{
				if (!dictionary.TryGetValue(mapHeld, out var value))
				{
					value = (dictionary[mapHeld] = LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, canKidnap: true, canTimeoutOrFlee: true, sappers: false, useAvoidGridSmart: false, canSteal: true, breachers: false, canPickUpOpportunisticWeapons: true), mapHeld));
				}
				value.AddPawn(pawn);
			}
		}
		Complete();
	}

	private InteractionResponseType ChooseRandomInteraction()
	{
		return Gen.RandomEnumValue<InteractionResponseType>(disallowFirstValue: false);
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignalDestroyed, "inSignalDestroyed");
		Scribe_Values.Look(ref inSignalArrested, "inSignalArrested");
		Scribe_Values.Look(ref inSignalSurgeryViolation, "inSignalSurgeryViolation");
		Scribe_Values.Look(ref inSignalPsychicRitualTarget, "inSignalPsychicRitualTarget");
		Scribe_Values.Look(ref inSignalRecruited, "inSignalRecruited");
		Scribe_Values.Look(ref inSignalKidnapped, "inSignalKidnapped");
		Scribe_Values.Look(ref inSignalAssaultColony, "inSignalAssaultColony");
		Scribe_Values.Look(ref inSignalLeftMap, "inSignalLeftMap");
		Scribe_Values.Look(ref inSignalLeftBehind, "inSignalLeftBehind");
		Scribe_Values.Look(ref inSignalBanished, "inSignalBanished");
		Scribe_Values.Look(ref outSignalDestroyed_AssaultColony, "outSignalDestroyed_AssaultColony");
		Scribe_Values.Look(ref outSignalDestroyed_LeaveColony, "outSignalDestroyed_LeaveColony");
		Scribe_Values.Look(ref outSignalDestroyed_BadThought, "outSignalDestroyed_BadThought");
		Scribe_Values.Look(ref outSignalArrested_AssaultColony, "outSignalArrested_AssaultColony");
		Scribe_Values.Look(ref outSignalArrested_LeaveColony, "outSignalArrested_LeaveColony");
		Scribe_Values.Look(ref outSignalArrested_BadThought, "outSignalArrested_BadThought");
		Scribe_Values.Look(ref outSignalLeftBehind_LeaveColony, "outSignalLeftBehind_LeaveColony");
		Scribe_Values.Look(ref outSignalLeftBehind_BadThought, "outSignalLeftBehind_BadThought");
		Scribe_Values.Look(ref outSignalSurgeryViolation_AssaultColony, "outSignalSurgeryViolation_AssaultColony");
		Scribe_Values.Look(ref outSignalSurgeryViolation_LeaveColony, "outSignalSurgeryViolation_LeaveColony");
		Scribe_Values.Look(ref outSignalSurgeryViolation_BadThought, "outSignalSurgeryViolation_BadThought");
		Scribe_Values.Look(ref outSignalPsychicRitualTarget_AssaultColony, "outSignalPsychicRitualTarget_AssaultColony");
		Scribe_Values.Look(ref outSignalPsychicRitualTarget_LeaveColony, "outSignalPsychicRitualTarget_LeaveColony");
		Scribe_Values.Look(ref outSignalPsychicRitualTarget_BadThought, "outSignalPsychicRitualTarget_BadThought");
		Scribe_Values.Look(ref outSignalLast_Arrested, "outSignalLastArrested");
		Scribe_Values.Look(ref outSignalLast_Destroyed, "outSignalLastDestroyed");
		Scribe_Values.Look(ref outSignalLast_Kidnapped, "outSignalLastKidnapped");
		Scribe_Values.Look(ref outSignalLast_Recruited, "outSignalLastRecruited");
		Scribe_Values.Look(ref outSignalLast_LeftMapAllHealthy, "outSignalLastLeftMapAllHealthy");
		Scribe_Values.Look(ref outSignalLast_LeftMapAllNotHealthy, "outSignalLastLeftMapAllNotHealthy");
		Scribe_Values.Look(ref outSignalLast_LeftBehind, "outSignalLast_LeftBehind");
		Scribe_Values.Look(ref outSignalLast_Banished, "outSignalLast_Banished");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref faction, "faction");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref pawnsLeftUnhealthy, "pawnsLeftUnhealthy", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
