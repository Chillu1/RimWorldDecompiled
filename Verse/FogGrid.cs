using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public sealed class FogGrid : IExposable
	{
		private Map map;

		public bool[] fogGrid;

		private const int AlwaysSendLetterIfUnfoggedMoreCellsThan = 600;

		public FogGrid(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			DataExposeUtility.BoolArray(ref fogGrid, map.Area, "fogGrid");
		}

		public void Unfog(IntVec3 c)
		{
			UnfogWorker(c);
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.Fillage != FillCategory.Full)
				{
					continue;
				}
				foreach (IntVec3 cell in thing.OccupiedRect().Cells)
				{
					UnfogWorker(cell);
				}
			}
		}

		private void UnfogWorker(IntVec3 c)
		{
			int num = map.cellIndices.CellToIndex(c);
			if (fogGrid[num])
			{
				fogGrid[num] = false;
				if (Current.ProgramState == ProgramState.Playing)
				{
					map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Things | MapMeshFlag.FogOfWar);
				}
				Designation designation = map.designationManager.DesignationAt(c, DesignationDefOf.Mine);
				if (designation != null && c.GetFirstMineable(map) == null)
				{
					designation.Delete();
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					map.roofGrid.Drawer.SetDirty();
				}
			}
		}

		public bool IsFogged(IntVec3 c)
		{
			if (!c.InBounds(map) || fogGrid == null)
			{
				return false;
			}
			return fogGrid[map.cellIndices.CellToIndex(c)];
		}

		public bool IsFogged(int index)
		{
			return fogGrid[index];
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

		public void Notify_FogBlockerRemoved(IntVec3 c)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds(map) && !IsFogged(c2))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				FloodUnfogAdjacent(c);
			}
		}

		public void Notify_PawnEnteringDoor(Building_Door door, Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
			{
				FloodUnfogAdjacent(door.Position);
			}
		}

		internal void SetAllFogged()
		{
			CellIndices cellIndices = map.cellIndices;
			if (fogGrid == null)
			{
				fogGrid = new bool[cellIndices.NumGridCells];
			}
			foreach (IntVec3 allCell in map.AllCells)
			{
				fogGrid[cellIndices.CellToIndex(allCell)] = true;
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				map.roofGrid.Drawer.SetDirty();
			}
		}

		private void FloodUnfogAdjacent(IntVec3 c)
		{
			Unfog(c);
			bool flag = false;
			FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = c + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(map) && intVec.Fogged(map))
				{
					Building edifice = intVec.GetEdifice(map);
					if (edifice == null || !edifice.def.MakeFog)
					{
						flag = true;
						floodUnfogResult = FloodFillerFog.FloodUnfog(intVec, map);
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
			if (flag)
			{
				if (floodUnfogResult.mechanoidFound)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, new TargetInfo(c, map));
				}
				else if (!floodUnfogResult.allOnScreen || floodUnfogResult.cellsUnfogged >= 600)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(c, map));
				}
			}
		}
	}
}
