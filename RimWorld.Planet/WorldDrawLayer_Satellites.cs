using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_Satellites : WorldDrawLayer
{
	private struct Orbit
	{
		public float inclination;

		public float angleOfAscendingNode;

		public float altitude;

		public float rotationAngle;

		public float angleDeltaSecond;
	}

	private const float DistanceToOrbit = 25f;

	private Material material;

	private int lastUpdate;

	private List<Orbit> orbits;

	protected override Quaternion Rotation => Quaternion.identity;

	public override bool ShouldRegenerate => lastUpdate != GenTicks.TicksGame;

	public override IEnumerable Regenerate()
	{
		if (material == null)
		{
			material = MaterialPool.MatFrom("PlaceholderImage", ShaderDatabase.WorldOverlayTransparentLit, 3600);
		}
		if (orbits == null)
		{
			orbits = new List<Orbit>
			{
				new Orbit
				{
					inclination = 15f,
					angleOfAscendingNode = 0f,
					altitude = 25f,
					rotationAngle = 0f,
					angleDeltaSecond = -4f
				}
			};
		}
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		Rand.PushState();
		Rand.Seed = Find.World.info.Seed;
		float num = (GenTicks.TicksGame - lastUpdate).TicksToSeconds();
		for (int i = 0; i < orbits.Count; i++)
		{
			Orbit value = orbits[i];
			WorldRendererUtility.PrintQuadTangentialToPlanet(Quaternion.AngleAxis(value.angleOfAscendingNode, Vector3.up) * Quaternion.Euler(0f, 0f, value.inclination) * Quaternion.Euler(0f, value.rotationAngle, 0f) * (-Vector3.forward * (100f + value.altitude)), subMesh: GetSubMesh(material), size: planetLayer.AverageTileSize * 6f, altOffset: 0f);
			value.rotationAngle = (value.rotationAngle + value.angleDeltaSecond * num) % 360f;
			orbits[i] = value;
		}
		Rand.PopState();
		FinalizeMesh(MeshParts.All);
		lastUpdate = GenTicks.TicksGame;
	}
}
