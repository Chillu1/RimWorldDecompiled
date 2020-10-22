using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class SectionLayer_TerrainScatter : SectionLayer
	{
		private class Scatterable
		{
			private Map map;

			public ScatterableDef def;

			public Vector3 loc;

			public float size;

			public float rotation;

			public bool IsOnValidTerrain
			{
				get
				{
					IntVec3 c = loc.ToIntVec3();
					TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
					if (def.scatterType == terrainDef.scatterType)
					{
						return !c.Filled(map);
					}
					return false;
				}
			}

			public Scatterable(ScatterableDef def, Vector3 loc, Map map)
			{
				this.def = def;
				this.loc = loc;
				this.map = map;
				size = Rand.Range(def.minSize, def.maxSize);
				rotation = Rand.Range(0f, 360f);
			}

			public void PrintOnto(SectionLayer layer)
			{
				Printer_Plane.PrintPlane(layer, loc, Vector2.one * size, def.mat, rotation);
			}
		}

		private List<Scatterable> scats = new List<Scatterable>();

		public override bool Visible => DebugViewSettings.drawTerrain;

		public SectionLayer_TerrainScatter(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			scats.RemoveAll((Scatterable scat) => !scat.IsOnValidTerrain);
			int num = 0;
			TerrainDef[] topGrid = base.Map.terrainGrid.topGrid;
			CellRect cellRect = section.CellRect;
			CellIndices cellIndices = base.Map.cellIndices;
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					if (topGrid[cellIndices.CellToIndex(j, i)].scatterType != null)
					{
						num++;
					}
				}
			}
			num /= 40;
			int num2 = 0;
			while (scats.Count < num && num2 < 200)
			{
				num2++;
				IntVec3 randomCell = section.CellRect.RandomCell;
				string terrScatType = base.Map.terrainGrid.TerrainAt(randomCell).scatterType;
				if (terrScatType != null && !randomCell.Filled(base.Map) && DefDatabase<ScatterableDef>.AllDefs.Where((ScatterableDef def) => def.scatterType == terrScatType).TryRandomElement(out var result))
				{
					Scatterable scatterable = new Scatterable(loc: new Vector3((float)randomCell.x + Rand.Value, randomCell.y, (float)randomCell.z + Rand.Value), def: result, map: base.Map);
					scats.Add(scatterable);
					scatterable.PrintOnto(this);
				}
			}
			for (int k = 0; k < scats.Count; k++)
			{
				scats[k].PrintOnto(this);
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
