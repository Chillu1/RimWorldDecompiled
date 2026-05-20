using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class QuestPart_FactionGoodwillChange : QuestPart
{
	public HistoryEventDef historyEvent;

	public string inSignal;

	public int change;

	public Faction faction;

	public bool canSendMessage = true;

	public bool canSendHostilityLetter = true;

	public bool getLookTargetFromSignal = true;

	public GlobalTargetInfo lookTarget;

	public bool ensureMakesHostile;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			yield return lookTarget;
		}
	}

	public override IEnumerable<Faction> InvolvedFactions
	{
		get
		{
			foreach (Faction involvedFaction in base.InvolvedFactions)
			{
				yield return involvedFaction;
			}
			if (faction != null)
			{
				yield return faction;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal) || faction == null || faction == Faction.OfPlayer)
		{
			return;
		}
		if (lookTarget.IsValid)
		{
			_ = lookTarget;
		}
		else if (getLookTargetFromSignal)
		{
			if (SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out var lookTargets))
			{
				lookTargets.TryGetPrimaryTarget();
			}
			else
			{
				_ = GlobalTargetInfo.Invalid;
			}
		}
		else
		{
			_ = GlobalTargetInfo.Invalid;
		}
		FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
		int arg = 0;
		if (!signal.args.TryGetArg("GOODWILL", out arg))
		{
			arg = change;
		}
		if (ensureMakesHostile)
		{
			arg = Mathf.Min(arg, Faction.OfPlayer.GoodwillToMakeHostile(faction));
		}
		Faction.OfPlayer.TryAffectGoodwillWith(faction, arg, canSendMessage, canSendHostilityLetter, (arg >= 0) ? (historyEvent ?? HistoryEventDefOf.QuestGoodwillReward) : historyEvent);
		TaggedString text = "";
		faction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, faction.PlayerRelationKind);
		if (!text.NullOrEmpty())
		{
			text = "\n\n" + text;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref historyEvent, "historyEvent");
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref change, "change", 0);
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref canSendMessage, "canSendMessage", defaultValue: true);
		Scribe_Values.Look(ref canSendHostilityLetter, "canSendHostilityLetter", defaultValue: true);
		Scribe_Values.Look(ref getLookTargetFromSignal, "getLookTargetFromSignal", defaultValue: true);
		Scribe_TargetInfo.Look(ref lookTarget, "lookTarget");
		Scribe_Values.Look(ref ensureMakesHostile, "ensureMakesHostile", defaultValue: false);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		change = -15;
		faction = Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
	}
}
