using RimWorld;
using UnityEngine;

namespace Verse
{
	internal class SectionLayer_Watergen : SectionLayer_Terrain
	{
		public SectionLayer_Watergen(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public override Material GetMaterialFor(TerrainDef terrain)
		{
			return terrain.waterDepthMaterial;
		}

		public override void DrawLayer()
		{
			if (!Visible)
			{
				return;
			}
			int count = subMeshes.Count;
			for (int i = 0; i < count; i++)
			{
				LayerSubMesh layerSubMesh = subMeshes[i];
				if (layerSubMesh.finalized && !layerSubMesh.disabled)
				{
					Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, SubcameraDefOf.WaterDepth.LayerId);
				}
			}
		}
	}
}
