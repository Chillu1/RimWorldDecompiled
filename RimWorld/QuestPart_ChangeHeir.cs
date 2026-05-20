using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_ChangeHeir : QuestPart
{
	public Faction faction;

	public Pawn holder;

	public Pawn heir;

	public string inSignal;

	public bool done;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			yield return holder;
			yield return heir;
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
		if (signal.tag == inSignal && faction != null)
		{
			holder.royalty.SetHeir(heir, faction);
			done = true;
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (holder == replace)
		{
			holder = with;
		}
		if (heir == replace)
		{
			heir = with;
		}
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_References.Look(ref holder, "holder");
		Scribe_References.Look(ref heir, "heir");
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref done, "done", defaultValue: false);
	}
}
