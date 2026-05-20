using UnityEngine;
using Verse;

namespace RimWorld;

public class Capture
{
	public Building_GravEngine engine;

	public SavedTexture2D capture;

	public Vector3 drawSize;

	public Vector3 captureCenter;

	public float minCameraSize;

	public float maxCameraSize;

	public CellRect captureBounds;

	public bool IsTerrainCapture => engine == null;

	public bool IsGravshipCapture => engine != null;

	public Capture(Building_GravEngine engine)
	{
		this.engine = engine;
	}
}
