using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoadDef : Def
{
	public class WorldRenderStep
	{
		public RoadWorldLayerDef layer;

		public float width;
	}

	public int priority;

	public bool ancientOnly;

	public float movementCostMultiplier = 1f;

	public int tilesPerSegment = 15;

	public RoadPathingDef pathingMode;

	public List<RoadDefGenStep> roadGenSteps;

	public List<WorldRenderStep> worldRenderSteps;

	[NoTranslate]
	public string worldTransitionGroup = "";

	public float distortionFrequency = 1f;

	public float distortionIntensity;

	[Unsaved(false)]
	private float[] cachedLayerWidth;

	public float GetLayerWidth(RoadWorldLayerDef def)
	{
		if (cachedLayerWidth == null)
		{
			cachedLayerWidth = new float[DefDatabase<RoadWorldLayerDef>.DefCount];
			for (int i = 0; i < DefDatabase<RoadWorldLayerDef>.DefCount; i++)
			{
				RoadWorldLayerDef roadWorldLayerDef = DefDatabase<RoadWorldLayerDef>.AllDefsListForReading[i];
				if (worldRenderSteps == null)
				{
					continue;
				}
				foreach (WorldRenderStep worldRenderStep in worldRenderSteps)
				{
					if (worldRenderStep.layer == roadWorldLayerDef)
					{
						cachedLayerWidth[roadWorldLayerDef.index] = worldRenderStep.width;
					}
				}
			}
		}
		return cachedLayerWidth[def.index];
	}

	public override void ClearCachedData()
	{
		base.ClearCachedData();
		cachedLayerWidth = null;
	}
}
