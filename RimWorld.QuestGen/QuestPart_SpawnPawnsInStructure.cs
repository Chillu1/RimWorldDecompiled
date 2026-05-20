using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_SpawnPawnsInStructure : QuestPart
{
	private List<Pawn> pawns;

	private string inSignal;

	public QuestPart_SpawnPawnsInStructure()
	{
	}

	public QuestPart_SpawnPawnsInStructure(List<Pawn> pawns, string inSignal)
	{
		this.pawns = pawns;
		this.inSignal = inSignal;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep);
		Scribe_Values.Look(ref inSignal, "inSignal");
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag != inSignal)
		{
			return;
		}
		try
		{
			signal.args.TryGetArg("SUBJECT", out MapParent arg);
			Map map = arg.Map;
			CellRect var = MapGenerator.GetVar<CellRect>("SpawnRect");
			foreach (Pawn pawn in pawns)
			{
				var.TryFindRandomCell(out var cell, Validator);
				GenSpawn.Spawn(pawn, cell, map);
			}
			LordMaker.MakeNewLord(Faction.OfAncientsHostile, new LordJob_SitePawns(pawns.First().Faction, var.CenterCell, 180000), map, pawns);
			pawns.Clear();
			bool Validator(IntVec3 x)
			{
				if (!x.Standable(map))
				{
					return false;
				}
				if (!x.Roofed(map))
				{
					return false;
				}
				if (!map.generatorDef.isUnderground && !map.Tile.LayerDef.isSpace && !map.reachability.CanReachMapEdge(x, TraverseParms.For(TraverseMode.PassDoors)))
				{
					return false;
				}
				return true;
			}
		}
		catch (Exception arg2)
		{
			Log.Error($"Failed to generate pawns in structure: {arg2}");
		}
	}
}
