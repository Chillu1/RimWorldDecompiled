using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_GiveToCaravan : QuestPart
{
	public string inSignal;

	public Caravan caravan;

	private List<Thing> items = new List<Thing>();

	private List<Pawn> pawns = new List<Pawn>();

	public IEnumerable<Thing> Things
	{
		get
		{
			return items.Concat(pawns.Cast<Thing>());
		}
		set
		{
			items.Clear();
			pawns.Clear();
			if (value == null)
			{
				return;
			}
			foreach (Thing item2 in value)
			{
				if (item2 is Pawn item)
				{
					pawns.Add(item);
				}
				else
				{
					items.Add(item2);
				}
			}
		}
	}

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (caravan != null)
			{
				yield return caravan;
			}
			foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
			{
				yield return questLookTarget2;
			}
		}
	}

	public override bool IncreasesPopulation => PawnsArriveQuestPartUtility.IncreasesPopulation(pawns, joinPlayer: true, makePrisoners: false);

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		pawns.RemoveAll((Pawn x) => x.Destroyed);
		Caravan arg = caravan;
		if (arg == null)
		{
			signal.args.TryGetArg("CARAVAN", out arg);
		}
		if (arg == null || !Things.Any())
		{
			return;
		}
		for (int num = 0; num < pawns.Count; num++)
		{
			if (pawns[num].Faction != Faction.OfPlayer)
			{
				pawns[num].SetFaction(Faction.OfPlayer);
			}
			arg.AddPawn(pawns[num], addCarriedPawnToWorldPawnsIfAny: true);
		}
		for (int num2 = 0; num2 < items.Count; num2++)
		{
			CaravanInventoryUtility.GiveThing(arg, items[num2]);
		}
		items.Clear();
	}

	public override void PostQuestAdded()
	{
		base.PostQuestAdded();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].def == ThingDefOf.PsychicAmplifier)
			{
				Find.History.Notify_PsylinkAvailable();
				break;
			}
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		return pawns.Contains(p);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		for (int i = 0; i < items.Count; i++)
		{
			items[i].Destroy();
		}
		items.Clear();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref caravan, "caravan");
		Scribe_Collections.Look(ref items, "items", LookMode.Deep);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			items.RemoveAll((Thing x) => x == null);
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
