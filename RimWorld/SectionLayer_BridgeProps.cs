using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class SectionLayer_BridgeProps : SectionLayer
	{
		private Dictionary<TerrainDef, (Material loop, Material right)> propsMatCache = new Dictionary<TerrainDef, (Material, Material)>();

		public override bool Visible => DebugViewSettings.drawTerrain;

		protected virtual bool UseSpaceGraphics => base.Map.generatorDef.renderWorld;

		public SectionLayer_BridgeProps(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlagDefOf.Terrain;
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			Map map = base.Map;
			TerrainGrid terrainGrid = map.terrainGrid;
			CellRect cellRect = section.CellRect;
			float y = AltitudeLayer.TerrainScatter.AltitudeFor();
			foreach (IntVec3 item3 in cellRect)
			{
				if (ShouldDrawPropsBelow(item3, terrainGrid))
				{
					(Material loop, Material right) materials = GetMaterials(terrainGrid.FoundationAt(item3));
					Material item = materials.loop;
					Material item2 = materials.right;
					IntVec3 c = item3;
					c.x++;
					Material material = ((!c.InBounds(map) || !ShouldDrawPropsBelow(c, terrainGrid)) ? item2 : item);
					LayerSubMesh subMesh = GetSubMesh(material);
					int count = subMesh.verts.Count;
					subMesh.verts.Add(new Vector3(item3.x, y, item3.z - 1));
					subMesh.verts.Add(new Vector3(item3.x, y, item3.z));
					subMesh.verts.Add(new Vector3(item3.x + 1, y, item3.z));
					subMesh.verts.Add(new Vector3(item3.x + 1, y, item3.z - 1));
					subMesh.uvs.Add(new Vector2(0f, 0f));
					subMesh.uvs.Add(new Vector2(0f, 1f));
					subMesh.uvs.Add(new Vector2(1f, 1f));
					subMesh.uvs.Add(new Vector2(1f, 0f));
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 1);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count + 3);
				}
			}
			FinalizeMesh(MeshParts.All);
		}

		public (Material loop, Material right) GetMaterials(TerrainDef def)
		{
			if (!propsMatCache.TryGetValue(def, out (Material, Material) value))
			{
				Graphic graphic = (UseSpaceGraphics ? def.spaceBridgePropsLoopGraphic : def.bridgePropsLoopGraphic);
				Graphic graphic2 = (UseSpaceGraphics ? def.spaceBridgePropsRightGraphic : def.bridgePropsRightGraphic);
				value = (propsMatCache[def] = (graphic.MatSingle, graphic2.MatSingle));
			}
			return value;
		}

		private bool ShouldDrawPropsBelow(IntVec3 c, TerrainGrid terrGrid)
		{
			TerrainDef terrainDef = terrGrid.FoundationAt(c);
			if (terrainDef == null || !terrainDef.bridge)
			{
				return false;
			}
			IntVec3 c2 = c;
			c2.z--;
			Map map = base.Map;
			if (!c2.InBounds(map))
			{
				return false;
			}
			TerrainDef terrainDef2 = terrGrid.TerrainAt(c2);
			if (terrGrid.FoundationAt(c2) != null)
			{
				return false;
			}
			if (terrainDef2.passability != Traversability.Impassable && !c2.SupportsStructureType(map, TerrainDefOf.Bridge.terrainAffordanceNeeded))
			{
				return false;
			}
			return true;
		}
	}
}
