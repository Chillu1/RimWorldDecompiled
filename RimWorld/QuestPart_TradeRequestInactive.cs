using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_TradeRequestInactive : QuestPartActivable
{
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

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (settlement == null || !settlement.Spawned)
		{
			Complete();
			return;
		}
		TradeRequestComp component = settlement.GetComponent<TradeRequestComp>();
		if (component == null || !component.ActiveRequest)
		{
			Complete(settlement.Named("SUBJECT"));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref settlement, "settlement");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		settlement = Find.WorldObjects.Settlements.FirstOrDefault();
	}
}
