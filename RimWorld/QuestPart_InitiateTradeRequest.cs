using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_InitiateTradeRequest : QuestPart
{
	public string inSignal;

	public Settlement settlement;

	public ThingDef requestedThingDef;

	public int requestedCount;

	public int requestDuration;

	public bool keepAfterQuestEnds;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (settlement != null)
			{
				yield return settlement;
			}
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
			if (settlement?.Faction != null)
			{
				yield return settlement.Faction;
			}
		}
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
			{
				yield return hyperlink;
			}
			yield return new Dialog_InfoCard.Hyperlink(requestedThingDef);
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		TradeRequestComp component = settlement.GetComponent<TradeRequestComp>();
		if (component != null)
		{
			if (component.ActiveRequest)
			{
				Log.Error("Settlement " + settlement.Label + " already has an active trade request.");
				return;
			}
			component.requestThingDef = requestedThingDef;
			component.requestCount = requestedCount;
			component.expiration = Find.TickManager.TicksGame + requestDuration;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (!keepAfterQuestEnds)
		{
			TradeRequestComp component = settlement.GetComponent<TradeRequestComp>();
			if (component != null && component.ActiveRequest)
			{
				component.Disable();
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref settlement, "settlement");
		Scribe_Defs.Look(ref requestedThingDef, "requestedThingDef");
		Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
		Scribe_Values.Look(ref requestDuration, "requestDuration", 0);
		Scribe_Values.Look(ref keepAfterQuestEnds, "keepAfterQuestEnds", defaultValue: false);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		settlement = Find.WorldObjects.Settlements.Where(delegate(Settlement x)
		{
			TradeRequestComp component = x.GetComponent<TradeRequestComp>();
			return component != null && !component.ActiveRequest && x.Faction != Faction.OfPlayer;
		}).RandomElementWithFallback();
		if (settlement == null)
		{
			settlement = Find.WorldObjects.Settlements.RandomElementWithFallback();
		}
		requestedThingDef = ThingDefOf.Silver;
		requestedCount = 100;
		requestDuration = 60000;
	}
}
