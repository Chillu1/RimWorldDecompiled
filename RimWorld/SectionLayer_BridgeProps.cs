using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class SectionLayer_BridgeProps : SectionLayer
	{
		private static readonly Material PropsLoopMat = MaterialPool.MatFrom("Terrain/Misc/BridgeProps_Loop", ShaderDatabase.Transparent);

		private static readonly Material PropsRightMat = MaterialPool.MatFrom("Terrain/Misc/BridgeProps_Right", ShaderDatabase.Transparent);

		public override bool Visible => DebugViewSettings.drawTerrain;

		public SectionLayer_BridgeProps(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			Map map = base.Map;
			TerrainGrid terrainGrid = map.terrainGrid;
			CellRect cellRect = section.CellRect;
			float y = AltitudeLayer.TerrainScatter.AltitudeFor();
			foreach (IntVec3 item in cellRect)
			{
				if (ShouldDrawPropsBelow(item, terrainGrid))
				{
					IntVec3 c = item;
					c.x++;
					Material material = (!c.InBounds(map) || !ShouldDrawPropsBelow(c, terrainGrid)) ? PropsRightMat : PropsLoopMat;
					LayerSubMesh subMesh = GetSubMesh(material);
					int count = subMesh.verts.Count;
					subMesh.verts.Add(new Vector3(item.x, y, item.z - 1));
					subMesh.verts.Add(new Vector3(item.x, y, item.z));
					subMesh.verts.Add(new Vector3(item.x + 1, y, item.z));
					subMesh.verts.Add(new Vector3(item.x + 1, y, item.z - 1));
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

		private bool ShouldDrawPropsBelow(IntVec3 c, TerrainGrid terrGrid)
		{
			TerrainDef terrainDef = terrGrid.TerrainAt(c);
			if (terrainDef == null || terrainDef != TerrainDefOf.Bridge)
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
			if (terrainDef2 == TerrainDefOf.Bridge)
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
