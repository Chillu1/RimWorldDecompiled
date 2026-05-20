using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_SetupTransportShip : QuestPart
{
	public TransportShip transportShip;

	public List<Pawn> pawns;

	public List<Thing> items;

	public string inSignal;

	public override bool QuestPartReserves(Pawn p)
	{
		if (p == null)
		{
			return false;
		}
		if (pawns.NotNullAndContains(p))
		{
			return true;
		}
		if (transportShip?.TransporterComp?.innerContainer != null)
		{
			return transportShip.TransporterComp.innerContainer.Contains(p);
		}
		return false;
	}

	public override bool QuestPartReserves(TransportShip ship)
	{
		return ship == transportShip;
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns?.Replace(replace, with);
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag == inSignal))
		{
			return;
		}
		if (!pawns.NullOrEmpty())
		{
			foreach (Pawn pawn in pawns)
			{
				if (!pawn.IsWorldPawn())
				{
					Log.Error("Trying to transfer a non-world pawn to a transportShip.");
				}
			}
			transportShip.TransporterComp.innerContainer.TryAddRangeOrTransfer(pawns, canMergeWithExistingStacks: true, destroyLeftover: true);
			pawns.Clear();
		}
		if (!items.NullOrEmpty())
		{
			transportShip.TransporterComp.innerContainer.TryAddRangeOrTransfer(items, canMergeWithExistingStacks: true, destroyLeftover: true);
			items.Clear();
		}
		transportShip.Start();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		transportShip = null;
		items = null;
		pawns = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref transportShip, "transportShip");
		Scribe_Collections.Look(ref items, "items", LookMode.Deep);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (!items.NullOrEmpty())
		{
			items.RemoveAll((Thing x) => x == null);
		}
		if (!pawns.NullOrEmpty())
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
