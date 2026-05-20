using System;
using System.Collections.Generic;
using RimWorld;
using Unity.Collections;

namespace Verse;

public sealed class FogGrid : IExposable, IDisposable
{
	private readonly Map map;

	private NativeBitArray fogGrid;

	private const int AlwaysSendLetterIfUnfoggedMoreCellsThan = 600;

	internal NativeBitArray FogGrid_Unsafe => fogGrid;

	public FogGrid(Map map)
	{
		this.map = map;
		fogGrid = new NativeBitArray(map.cellIndices.NumGridCells, Allocator.Persistent);
	}

	public void ExposeData()
	{
		DataExposeUtility.LookBitArray(ref fogGrid, map.Area, "fogGrid");
	}

	public void Unfog(IntVec3 c)
	{
		UnfogWorker(c);
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (thing.def.Fillage == FillCategory.Full)
			{
				foreach (IntVec3 cell in thing.OccupiedRect().Cells)
				{
					UnfogWorker(cell);
				}
			}
			thingList[i].Notify_Unfogged();
		}
		map.events.Notify_CellFogChanged(c, fogged: false);
	}

	private void UnfogWorker(IntVec3 c)
	{
		int pos = map.cellIndices.CellToIndex(c);
		if (fogGrid.IsSet(pos))
		{
			fogGrid.Set(pos, value: false);
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.mapDrawer.MapMeshDirty(c, (ulong)MapMeshFlagDefOf.FogOfWar | (ulong)MapMeshFlagDefOf.Things);
			}
			Designation designation = map.designationManager.DesignationAt(c, DesignationDefOf.Mine);
			if (designation != null && c.GetFirstMineable(map) == null)
			{
				designation.Delete();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.roofGrid.Drawer.SetDirty();
				map.mapTemperature.Drawer.SetDirty();
			}
		}
	}

	public bool IsFogged(IntVec3 c)
	{
		if (!fogGrid.IsCreated || !c.InBounds(map))
		{
			return false;
		}
		return fogGrid.IsSet(map.cellIndices.CellToIndex(c));
	}

	public bool IsFogged(int index)
	{
		if (!fogGrid.IsCreated)
		{
			return false;
		}
		return fogGrid.IsSet(index);
	}

	public void ClearAllFog()
	{
		for (int i = 0; i < map.Size.x; i++)
		{
			for (int j = 0; j < map.Size.z; j++)
			{
				Unfog(new IntVec3(i, 0, j));
			}
		}
	}

	public void Refog(CellRect rect)
	{
		foreach (IntVec3 item in rect)
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			int pos = map.cellIndices.CellToIndex(item);
			if (!fogGrid.IsSet(pos))
			{
				fogGrid.Set(pos, value: true);
				if (Current.ProgramState == ProgramState.Playing)
				{
					map.mapDrawer.MapMeshDirty(item, (ulong)MapMeshFlagDefOf.FogOfWar | (ulong)MapMeshFlagDefOf.Things);
				}
				map.events.Notify_CellFogChanged(item, fogged: true);
			}
		}
	}

	public void Notify_FogBlockerRemoved(Thing thing)
	{
		if (Current.ProgramState != ProgramState.Playing)
		{
			return;
		}
		bool flag = false;
		foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(thing))
		{
			if (item.InBounds(map))
			{
				Building edifice = item.GetEdifice(map);
				if (!IsFogged(item) && ((edifice != null && edifice.def.IsDoor) || edifice == null || !edifice.def.MakeFog))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			FloodUnfogAdjacent(thing, !map.generatorDef.ignoreAreaRevealedLetter);
		}
	}

	public void Notify_PawnEnteringDoor(Building_Door door, Pawn pawn)
	{
		if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
		{
			FloodUnfogAdjacent(door.Position, sendLetters: false);
		}
	}

	internal void SetAllFogged()
	{
		CellIndices cellIndices = map.cellIndices;
		foreach (IntVec3 allCell in map.AllCells)
		{
			fogGrid.Set(cellIndices.CellToIndex(allCell), value: true);
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			map.roofGrid.Drawer.SetDirty();
		}
		map.events.Notify_MapFogged();
	}

	public void FloodUnfogAdjacent(IntVec3 c, bool sendLetters = true)
	{
		Unfog(c);
		bool flag = false;
		FloodUnfogResult unfogResult = default(FloodUnfogResult);
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = c + GenAdj.CardinalDirections[i];
			if (intVec.InBounds(map) && intVec.Fogged(map))
			{
				Building edifice = intVec.GetEdifice(map);
				if (edifice == null || !edifice.def.MakeFog)
				{
					flag = true;
					unfogResult = FloodFillerFog.FloodUnfog(intVec, map);
				}
				else
				{
					Unfog(intVec);
				}
			}
		}
		for (int j = 0; j < 8; j++)
		{
			IntVec3 c2 = c + GenAdj.AdjacentCells[j];
			if (c2.InBounds(map))
			{
				Building edifice2 = c2.GetEdifice(map);
				if (edifice2 != null && edifice2.def.MakeFog)
				{
					Unfog(c2);
				}
			}
		}
		if (flag && sendLetters)
		{
			NotifyAreaRevealed(c, unfogResult);
		}
	}

	public void FloodUnfogAdjacent(Thing thing, bool sendLetters = true)
	{
		Unfog(thing.Position);
		bool flag = false;
		FloodUnfogResult unfogResult = default(FloodUnfogResult);
		foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(thing))
		{
			if (item.InBounds(map) && item.Fogged(map))
			{
				Building edifice = item.GetEdifice(map);
				if (edifice == null || !edifice.def.MakeFog)
				{
					flag = true;
					unfogResult = FloodFillerFog.FloodUnfog(item, map);
				}
				else
				{
					Unfog(item);
				}
			}
		}
		foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(thing))
		{
			if (item2.InBounds(map))
			{
				Building edifice2 = item2.GetEdifice(map);
				if (edifice2 != null && edifice2.def.MakeFog)
				{
					Unfog(item2);
				}
			}
		}
		if (flag && sendLetters)
		{
			NotifyAreaRevealed(thing.Position, unfogResult);
		}
	}

	private void NotifyAreaRevealed(IntVec3 c, FloodUnfogResult unfogResult)
	{
		if (unfogResult.mechanoidFound)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, new TargetInfo(c, map));
		}
		else if (!unfogResult.allOnScreen || unfogResult.cellsUnfogged >= 600)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(c, map));
		}
	}

	public void Dispose()
	{
		fogGrid.Dispose();
	}
}
