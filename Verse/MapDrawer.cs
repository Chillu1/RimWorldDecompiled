using RimWorld;
using UnityEngine;

namespace Verse
{
	public sealed class MapDrawer
	{
		private Map map;

		private Section[,] sections;

		private IntVec2 SectionCount
		{
			get
			{
				IntVec2 result = default(IntVec2);
				result.x = Mathf.CeilToInt((float)map.Size.x / 17f);
				result.z = Mathf.CeilToInt((float)map.Size.z / 17f);
				return result;
			}
		}

		private CellRect VisibleSections
		{
			get
			{
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				CellRect sunShadowsViewRect = GetSunShadowsViewRect(currentViewRect);
				sunShadowsViewRect.ClipInsideMap(map);
				IntVec2 intVec = SectionCoordsAt(sunShadowsViewRect.BottomLeft);
				IntVec2 intVec2 = SectionCoordsAt(sunShadowsViewRect.TopRight);
				if (intVec2.x < intVec.x || intVec2.z < intVec.z)
				{
					return CellRect.Empty;
				}
				return CellRect.FromLimits(intVec.x, intVec.z, intVec2.x, intVec2.z);
			}
		}

		public MapDrawer(Map map)
		{
			this.map = map;
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags)
		{
			bool regenAdjacentCells = (dirtyFlags & (MapMeshFlag.FogOfWar | MapMeshFlag.Buildings)) != 0;
			bool regenAdjacentSections = (dirtyFlags & MapMeshFlag.GroundGlow) != 0;
			MapMeshDirty(loc, dirtyFlags, regenAdjacentCells, regenAdjacentSections);
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags, bool regenAdjacentCells, bool regenAdjacentSections)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			SectionAt(loc).dirtyFlags |= dirtyFlags;
			if (regenAdjacentCells)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = loc + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(map))
					{
						SectionAt(intVec).dirtyFlags |= dirtyFlags;
					}
				}
			}
			if (!regenAdjacentSections)
			{
				return;
			}
			IntVec2 a = SectionCoordsAt(loc);
			for (int j = 0; j < 8; j++)
			{
				IntVec3 intVec2 = GenAdj.AdjacentCells[j];
				IntVec2 intVec3 = a + new IntVec2(intVec2.x, intVec2.z);
				IntVec2 sectionCount = SectionCount;
				if (intVec3.x >= 0 && intVec3.z >= 0 && intVec3.x <= sectionCount.x - 1 && intVec3.z <= sectionCount.z - 1)
				{
					sections[intVec3.x, intVec3.z].dirtyFlags |= dirtyFlags;
				}
			}
		}

		public void MapMeshDrawerUpdate_First()
		{
			CellRect visibleSections = VisibleSections;
			bool flag = false;
			foreach (IntVec3 item in visibleSections)
			{
				Section sect = sections[item.x, item.z];
				if (TryUpdateSection(sect))
				{
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			for (int i = 0; i < SectionCount.x; i++)
			{
				for (int j = 0; j < SectionCount.z; j++)
				{
					if (TryUpdateSection(sections[i, j]))
					{
						return;
					}
				}
			}
		}

		private bool TryUpdateSection(Section sect)
		{
			if (sect.dirtyFlags == MapMeshFlag.None)
			{
				return false;
			}
			for (int i = 0; i < MapMeshFlagUtility.allFlags.Count; i++)
			{
				MapMeshFlag mapMeshFlag = MapMeshFlagUtility.allFlags[i];
				if ((sect.dirtyFlags & mapMeshFlag) != 0)
				{
					sect.RegenerateLayers(mapMeshFlag);
				}
			}
			sect.dirtyFlags = MapMeshFlag.None;
			return true;
		}

		public void DrawMapMesh()
		{
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			currentViewRect.minX -= 17;
			currentViewRect.minZ -= 17;
			foreach (IntVec3 visibleSection in VisibleSections)
			{
				Section section = sections[visibleSection.x, visibleSection.z];
				section.DrawSection(!currentViewRect.Contains(section.botLeft));
			}
		}

		private IntVec2 SectionCoordsAt(IntVec3 loc)
		{
			return new IntVec2(Mathf.FloorToInt(loc.x / 17), Mathf.FloorToInt(loc.z / 17));
		}

		public Section SectionAt(IntVec3 loc)
		{
			IntVec2 intVec = SectionCoordsAt(loc);
			return sections[intVec.x, intVec.z];
		}

		public void RegenerateEverythingNow()
		{
			if (sections == null)
			{
				sections = new Section[SectionCount.x, SectionCount.z];
			}
			for (int i = 0; i < SectionCount.x; i++)
			{
				for (int j = 0; j < SectionCount.z; j++)
				{
					if (sections[i, j] == null)
					{
						sections[i, j] = new Section(new IntVec3(i, 0, j), map);
					}
					sections[i, j].RegenerateAllLayers();
				}
			}
		}

		public void WholeMapChanged(MapMeshFlag change)
		{
			for (int i = 0; i < SectionCount.x; i++)
			{
				for (int j = 0; j < SectionCount.z; j++)
				{
					sections[i, j].dirtyFlags |= change;
				}
			}
		}

		private CellRect GetSunShadowsViewRect(CellRect rect)
		{
			GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(map, GenCelestial.LightType.Shadow);
			if (lightSourceInfo.vector.x < 0f)
			{
				rect.maxX -= Mathf.FloorToInt(lightSourceInfo.vector.x);
			}
			else
			{
				rect.minX -= Mathf.CeilToInt(lightSourceInfo.vector.x);
			}
			if (lightSourceInfo.vector.y < 0f)
			{
				rect.maxZ -= Mathf.FloorToInt(lightSourceInfo.vector.y);
			}
			else
			{
				rect.minZ -= Mathf.CeilToInt(lightSourceInfo.vector.y);
			}
			return rect;
		}
	}
}
