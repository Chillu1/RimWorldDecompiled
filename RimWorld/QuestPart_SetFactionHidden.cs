using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_SetFactionHidden : QuestPart
{
	public string inSignal;

	public Faction faction;

	public bool hidden;

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
		if (signal.tag == inSignal && faction != null && faction.Hidden != hidden)
		{
			faction.hidden = hidden;
		}
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
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref faction, "faction");
	}
}
