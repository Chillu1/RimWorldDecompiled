using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class SectionLayer_Gas : SectionLayer
{
	private MaterialPropertyBlock propertyBlock;

	private static Material GasMat = MaterialPool.MatFrom("Things/Gas/GasCloudThickA", ShaderDatabase.GasRotating, 3000);

	private static bool gasMatSet = false;

	protected virtual FloatRange VertexScaleOffsetRange => new FloatRange(0.4f, 0.6f);

	protected virtual FloatRange VertexPositionOffsetRange => new FloatRange(-0.2f, 0.2f);

	public override bool Visible => DebugViewSettings.drawGas;

	public virtual Material Mat => GasMat;

	public SectionLayer_Gas(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Gas;
		propertyBlock = new MaterialPropertyBlock();
		if (!gasMatSet)
		{
			gasMatSet = true;
			GasMat.SetTexture(ShaderPropertyIDs.ToxGasTex, ContentFinder<Texture2D>.Get("Things/Gas/GasCloudThickA"));
			GasMat.SetTexture(ShaderPropertyIDs.RotGasTex, ContentFinder<Texture2D>.Get("Things/Gas/GasCloudThickA"));
			GasMat.SetTexture(ShaderPropertyIDs.DeadlifeDustTex, ContentFinder<Texture2D>.Get("Things/Gas/DeadlifeDust"));
		}
	}

	public override void DrawLayer()
	{
		if (!Visible)
		{
			return;
		}
		propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, RealTime.UnpausedRealTime);
		int count = subMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			LayerSubMesh layerSubMesh = subMeshes[i];
			if (layerSubMesh.finalized && !layerSubMesh.disabled)
			{
				Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, 0, null, 0, propertyBlock);
			}
		}
	}

	public virtual Color ColorAt(IntVec3 cell)
	{
		return DensityAt(cell);
	}

	public virtual Vector4 DensityAt(IntVec3 cell)
	{
		return base.Map.gasGrid.DensitiesAt(cell);
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		LayerSubMesh subMesh = GetSubMesh(Mat);
		float altitude = AltitudeLayer.Gas.AltitudeFor();
		int num = section.botLeft.x;
		foreach (IntVec3 item in section.CellRect)
		{
			if (base.Map.gasGrid.AnyGasAt(item))
			{
				int count = subMesh.verts.Count;
				AddCell(item, num, count, subMesh, altitude);
			}
			num++;
		}
		if (subMesh.verts.Count > 0)
		{
			subMesh.FinalizeMesh(MeshParts.All);
		}
	}

	protected void AddCell(IntVec3 c, int index, int startVertIndex, LayerSubMesh sm, float altitude)
	{
		Rand.PushState(index);
		Color color = ColorAt(c);
		float randomInRange = VertexScaleOffsetRange.RandomInRange;
		float randomInRange2 = VertexPositionOffsetRange.RandomInRange;
		float randomInRange3 = VertexPositionOffsetRange.RandomInRange;
		float x = (float)c.x - randomInRange + randomInRange2;
		float x2 = (float)(c.x + 1) + randomInRange + randomInRange2;
		float z = (float)c.z - randomInRange + randomInRange3;
		float z2 = (float)(c.z + 1) + randomInRange + randomInRange3;
		float y = altitude + Rand.Range(-0.01f, 0.01f);
		sm.verts.Add(new Vector3(x, y, z));
		sm.verts.Add(new Vector3(x, y, z2));
		sm.verts.Add(new Vector3(x2, y, z2));
		sm.verts.Add(new Vector3(x2, y, z));
		sm.uvs.Add(new Vector3(0f, 0f, index));
		sm.uvs.Add(new Vector3(0f, 1f, index));
		sm.uvs.Add(new Vector3(1f, 1f, index));
		sm.uvs.Add(new Vector3(1f, 0f, index));
		sm.colors.Add(color);
		sm.colors.Add(color);
		sm.colors.Add(color);
		sm.colors.Add(color);
		sm.tris.Add(startVertIndex);
		sm.tris.Add(startVertIndex + 1);
		sm.tris.Add(startVertIndex + 2);
		sm.tris.Add(startVertIndex);
		sm.tris.Add(startVertIndex + 2);
		sm.tris.Add(startVertIndex + 3);
		Rand.PopState();
	}
}
