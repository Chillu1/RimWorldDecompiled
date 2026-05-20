using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class GlobalDrawLayer_Sun : WorldDrawLayerBase
{
	private const float SunDrawSize = 15f;

	protected override int RenderLayer => WorldCameraManager.WorldSkyboxLayer;

	protected override Quaternion Rotation => Quaternion.LookRotation(GenCelestial.CurSunPositionInWorldSpace());

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		Rand.PushState();
		Rand.Seed = Find.World.info.Seed;
		LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Sun);
		WorldRendererUtility.PrintQuadTangentialToPlanet(Vector3.forward * 20f, 15f, 0f, subMesh, counterClockwise: true);
		Rand.PopState();
		FinalizeMesh(MeshParts.All);
	}
}
