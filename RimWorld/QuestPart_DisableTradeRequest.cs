using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_DisableTradeRequest : QuestPart
{
	public string inSignal;

	public Settlement settlement;

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
			if (settlement.Faction != null)
			{
				yield return settlement.Faction;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal)
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
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		settlement = Find.WorldObjects.Settlements.Where((Settlement x) => x.GetComponent<TradeRequestComp>()?.ActiveRequest ?? false).RandomElementWithFallback();
		if (settlement == null)
		{
			settlement = Find.WorldObjects.Settlements.RandomElementWithFallback();
		}
	}
}
