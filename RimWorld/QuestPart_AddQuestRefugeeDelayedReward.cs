using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class QuestPart_AddQuestRefugeeDelayedReward : QuestPart_AddQuest
{
	public List<Pawn> lodgers = new List<Pawn>();

	public FloatRange marketValueRange;

	public Faction faction;

	public string inSignalRemovePawn;

	public override QuestScriptDef QuestDef => QuestScriptDefOf.RefugeeDelayedReward;

	public override Slate GetSlate()
	{
		Slate slate = new Slate();
		slate.Set("marketValueRange", marketValueRange);
		slate.Set("faction", faction);
		for (int i = 0; i < lodgers.Count; i++)
		{
			if (!lodgers[i].Dead && lodgers[i].Faction != Faction.OfPlayer && !lodgers[i].IsPrisoner)
			{
				slate.Set("rewardGiver", lodgers[i]);
				break;
			}
		}
		return slate;
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && lodgers.Contains(arg))
		{
			lodgers.Remove(arg);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref faction, "faction");
		Scribe_Collections.Look(ref lodgers, "lodgers", LookMode.Reference);
		Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		Scribe_Values.Look(ref marketValueRange, "marketValueRange");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			lodgers.RemoveAll((Pawn x) => x == null);
		}
	}
}
