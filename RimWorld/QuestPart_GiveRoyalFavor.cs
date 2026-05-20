using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_GiveRoyalFavor : QuestPart
{
	public Pawn giveTo;

	public bool giveToAccepter;

	public string inSignal;

	public int amount;

	public Faction faction;

	public override bool RequiresAccepter => giveToAccepter;

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
		if (signal.tag == inSignal)
		{
			Pawn arg = (giveToAccepter ? quest.AccepterPawn : giveTo);
			if (arg == null)
			{
				signal.args.TryGetArg("CHOSEN", out arg);
			}
			if (arg != null && arg.royalty != null)
			{
				arg.royalty.GainFavor(faction, amount);
			}
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		giveTo = PawnsFinder.AllMaps_FreeColonists.RandomElement();
		inSignal = "DebugSignal" + Rand.Int;
		faction = Find.FactionManager.RandomEnemyFaction();
		amount = 10;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref giveTo, "giveTo");
		Scribe_Values.Look(ref giveToAccepter, "giveToAccepter", defaultValue: false);
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref amount, "amount", 0);
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (giveTo == replace)
		{
			giveTo = with;
		}
	}
}
