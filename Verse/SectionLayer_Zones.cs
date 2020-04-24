using RimWorld;
using UnityEngine;

namespace Verse
{
	internal class SectionLayer_Zones : SectionLayer
	{
		public override bool Visible => DebugViewSettings.drawWorldOverlays;

		public SectionLayer_Zones(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Zone;
		}

		public override void DrawLayer()
		{
			if (OverlayDrawHandler.ShouldDrawZones)
			{
				base.DrawLayer();
			}
		}

		public override void Regenerate()
		{
			float y = AltitudeLayer.Zone.AltitudeFor();
			ZoneManager zoneManager = base.Map.zoneManager;
			CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
			cellRect.ClipInsideMap(base.Map);
			ClearSubMeshes(MeshParts.All);
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					Zone zone = zoneManager.ZoneAt(new IntVec3(i, 0, j));
					if (zone != null && !zone.hidden)
					{
						LayerSubMesh subMesh = GetSubMesh(zone.Material);
						int count = subMesh.verts.Count;
						subMesh.verts.Add(new Vector3(i, y, j));
						subMesh.verts.Add(new Vector3(i, y, j + 1));
						subMesh.verts.Add(new Vector3(i + 1, y, j + 1));
						subMesh.verts.Add(new Vector3(i + 1, y, j));
						subMesh.tris.Add(count);
						subMesh.tris.Add(count + 1);
						subMesh.tris.Add(count + 2);
						subMesh.tris.Add(count);
						subMesh.tris.Add(count + 2);
						subMesh.tris.Add(count + 3);
					}
				}
			}
			FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
	}
}
