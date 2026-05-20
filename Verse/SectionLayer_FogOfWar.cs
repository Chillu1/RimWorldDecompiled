using RimWorld;
using Unity.Collections;
using UnityEngine;

namespace Verse;

public class SectionLayer_FogOfWar : SectionLayer
{
	private readonly bool[] vertsCovered = new bool[9];

	public override bool Visible => DebugViewSettings.drawFog;

	public SectionLayer_FogOfWar(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.FogOfWar;
	}

	public override void Regenerate()
	{
		LayerSubMesh subMesh = GetSubMesh(MatBases.FogOfWar);
		if (subMesh.mesh.vertexCount == 0)
		{
			SectionLayerGeometryMaker_Solid.MakeBaseGeometry(section, subMesh, AltitudeLayer.FogOfWar);
		}
		subMesh.Clear(MeshParts.Colors);
		NativeBitArray fogGrid_Unsafe = base.Map.fogGrid.FogGrid_Unsafe;
		CellRect cellRect = section.CellRect;
		int num = base.Map.Size.z - 1;
		int num2 = base.Map.Size.x - 1;
		bool flag = false;
		CellIndices cellIndices = base.Map.cellIndices;
		for (int i = cellRect.minX; i <= cellRect.maxX; i++)
		{
			for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
			{
				if (fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i, j)))
				{
					for (int k = 0; k < 9; k++)
					{
						vertsCovered[k] = true;
					}
				}
				else
				{
					for (int l = 0; l < 9; l++)
					{
						vertsCovered[l] = false;
					}
					if (j < num && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i, j + 1)))
					{
						vertsCovered[2] = true;
						vertsCovered[3] = true;
						vertsCovered[4] = true;
					}
					if (j > 0 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i, j - 1)))
					{
						vertsCovered[6] = true;
						vertsCovered[7] = true;
						vertsCovered[0] = true;
					}
					if (i < num2 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i + 1, j)))
					{
						vertsCovered[4] = true;
						vertsCovered[5] = true;
						vertsCovered[6] = true;
					}
					if (i > 0 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i - 1, j)))
					{
						vertsCovered[0] = true;
						vertsCovered[1] = true;
						vertsCovered[2] = true;
					}
					if (j > 0 && i > 0 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i - 1, j - 1)))
					{
						vertsCovered[0] = true;
					}
					if (j < num && i > 0 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i - 1, j + 1)))
					{
						vertsCovered[2] = true;
					}
					if (j < num && i < num2 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i + 1, j + 1)))
					{
						vertsCovered[4] = true;
					}
					if (j > 0 && i < num2 && fogGrid_Unsafe.IsSet(cellIndices.CellToIndex(i + 1, j - 1)))
					{
						vertsCovered[6] = true;
					}
				}
				for (int m = 0; m < 9; m++)
				{
					byte a;
					if (vertsCovered[m])
					{
						a = byte.MaxValue;
						flag = true;
					}
					else
					{
						a = 0;
					}
					subMesh.colors.Add(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a));
				}
			}
		}
		if (flag)
		{
			subMesh.disabled = false;
			subMesh.FinalizeMesh(MeshParts.Colors);
		}
		else
		{
			subMesh.disabled = true;
		}
	}
}
