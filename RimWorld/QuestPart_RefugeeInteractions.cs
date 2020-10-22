using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
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

		public string inSignalRecruited;

		public string inSignalKidnapped;

		public string inSignalAssaultColony;

		public string inSignalLeftMap;

		public string inSignalBanished;

		public string outSignalDestroyed_AssaultColony;

		public string outSignalDestroyed_LeaveColony;

		public string outSignalDestroyed_BadThought;

		public string outSignalArrested_AssaultColony;

		public string outSignalArrested_LeaveColony;

		public string outSignalArrested_BadThought;

		public string outSignalSurgeryViolation_AssaultColony;

		public string outSignalSurgeryViolation_LeaveColony;

		public string outSignalSurgeryViolation_BadThought;

		public string outSignalLast_Arrested;

		public string outSignalLast_Destroyed;

		public string outSignalLast_Kidnapped;

		public string outSignalLast_Recruited;

		public string outSignalLast_LeftMapAllHealthy;

		public string outSignalLast_LeftMapAllNotHealthy;

		public string outSignalLast_Banished;

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
				if (pawns.Count == 0)
				{
					if (pawnsLeftUnhealthy > 0)
					{
						Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllNotHealthy, signal.args));
					}
					else
					{
						Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllHealthy, signal.args));
					}
				}
			}
			if (signal.tag == inSignalDestroyed && signal.args.TryGetArg("SUBJECT", out Pawn arg5) && pawns.Contains(arg5))
			{
				pawns.Remove(arg5);
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
						AssaultColony();
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
			if (signal.tag == inSignalArrested && signal.args.TryGetArg("SUBJECT", out Pawn arg6) && pawns.Contains(arg6))
			{
				pawns.Remove(arg6);
				arg6.SetFaction(faction);
				if (pawns.Count == 0)
				{
					Find.SignalManager.SendSignal(new Signal(outSignalLast_Arrested, signal.args));
				}
				else
				{
					signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
					switch (ChooseRandomInteraction())
					{
					case InteractionResponseType.AssaultColony:
						AssaultColony();
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
			if (signal.tag == inSignalSurgeryViolation && signal.args.TryGetArg("SUBJECT", out Pawn arg7) && pawns.Contains(arg7))
			{
				signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
				switch (ChooseRandomInteraction())
				{
				case InteractionResponseType.AssaultColony:
					AssaultColony();
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
			if (inSignalAssaultColony != null && signal.tag == inSignalAssaultColony)
			{
				AssaultColony();
			}
		}

		private void LeavePlayer()
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				pawns[i].SetFaction(faction);
			}
			LeaveQuestPartUtility.MakePawnsLeave(pawns, sendLetter: false, quest);
			Complete();
		}

		private void AssaultColony()
		{
			faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: false);
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
			Lord lord = LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction), mapParent.Map);
			for (int k = 0; k < pawns.Count; k++)
			{
				if (!pawns[k].Dead)
				{
					lord.AddPawn(pawns[k]);
				}
			}
			Complete();
		}

		private InteractionResponseType ChooseRandomInteraction()
		{
			Array values = Enum.GetValues(typeof(InteractionResponseType));
			return (InteractionResponseType)values.GetValue(Rand.Range(0, values.Length));
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
			Scribe_Values.Look(ref inSignalRecruited, "inSignalRecruited");
			Scribe_Values.Look(ref inSignalKidnapped, "inSignalKidnapped");
			Scribe_Values.Look(ref inSignalAssaultColony, "inSignalAssaultColony");
			Scribe_Values.Look(ref inSignalLeftMap, "inSignalLeftMap");
			Scribe_Values.Look(ref inSignalBanished, "inSignalBanished");
			Scribe_Values.Look(ref outSignalDestroyed_AssaultColony, "outSignalDestroyed_AssaultColony");
			Scribe_Values.Look(ref outSignalDestroyed_LeaveColony, "outSignalDestroyed_LeaveColony");
			Scribe_Values.Look(ref outSignalDestroyed_BadThought, "outSignalDestroyed_BadThought");
			Scribe_Values.Look(ref outSignalArrested_AssaultColony, "outSignalArrested_AssaultColony");
			Scribe_Values.Look(ref outSignalArrested_LeaveColony, "outSignalArrested_LeaveColony");
			Scribe_Values.Look(ref outSignalArrested_BadThought, "outSignalArrested_BadThought");
			Scribe_Values.Look(ref outSignalSurgeryViolation_AssaultColony, "outSignalSurgeryViolation_AssaultColony");
			Scribe_Values.Look(ref outSignalSurgeryViolation_LeaveColony, "outSignalSurgeryViolation_LeaveColony");
			Scribe_Values.Look(ref outSignalSurgeryViolation_BadThought, "outSignalSurgeryViolation_BadThought");
			Scribe_Values.Look(ref outSignalLast_Arrested, "outSignalLastArrested");
			Scribe_Values.Look(ref outSignalLast_Destroyed, "outSignalLastDestroyed");
			Scribe_Values.Look(ref outSignalLast_Kidnapped, "outSignalLastKidnapped");
			Scribe_Values.Look(ref outSignalLast_Recruited, "outSignalLastRecruited");
			Scribe_Values.Look(ref outSignalLast_LeftMapAllHealthy, "outSignalLastLeftMapAllHealthy");
			Scribe_Values.Look(ref outSignalLast_LeftMapAllNotHealthy, "outSignalLastLeftMapAllNotHealthy");
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
}
