using UnityEngine;
using Verse;

namespace RimWorld;

public class PlanetLayerSettings : IExposable
{
	public Vector3 origin;

	public float radius = 100f;

	public float viewAngle = 180f;

	public float extraCameraAltitude;

	public int subdivisions = 10;

	public bool useSurfaceViewAngle;

	public float backgroundWorldCameraOffset;

	public float backgroundWorldCameraParallaxDistancePer100Cells;

	public void ExposeData()
	{
		Scribe_Values.Look(ref origin, "origin");
		Scribe_Values.Look(ref radius, "radius", 100f);
		Scribe_Values.Look(ref viewAngle, "viewAngle", 180f);
		Scribe_Values.Look(ref extraCameraAltitude, "extraCameraAltitude", 0f);
		Scribe_Values.Look(ref subdivisions, "subdivisions", 10);
		Scribe_Values.Look(ref useSurfaceViewAngle, "useSurfaceViewAngle", defaultValue: false);
		Scribe_Values.Look(ref backgroundWorldCameraOffset, "backgroundWorldCameraOffset", 0f);
		Scribe_Values.Look(ref backgroundWorldCameraParallaxDistancePer100Cells, "backgroundWorldCameraParallaxDistancePer100Cells", 0f);
	}
}
