using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnFootprintMaker
{
	private Pawn pawn;

	private Vector3 lastFootprintPlacePos;

	private bool lastFootprintRight;

	private const float FootprintIntervalDist = 0.632f;

	private static readonly Vector3 FootprintOffset = new Vector3(0f, 0f, -0.3f);

	private const float LeftRightOffsetDist = 0.17f;

	private const float FootprintSplashSize = 2f;

	public PawnFootprintMaker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (!pawn.RaceProps.makesFootprints || (pawn.IsMutant && !pawn.mutant.Def.makesFootprints))
		{
			TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
			if (terrain == null || !terrain.takeSplashes)
			{
				return;
			}
		}
		if ((pawn.Drawer.DrawPos - lastFootprintPlacePos).MagnitudeHorizontalSquared() > 0.39942405f)
		{
			TryPlaceFootprint();
		}
	}

	private void TryPlaceFootprint()
	{
		Vector3 drawPos = pawn.Drawer.DrawPos;
		Vector3 normalized = (drawPos - lastFootprintPlacePos).normalized;
		float rot = normalized.AngleFlat();
		float angle = (lastFootprintRight ? 90 : (-90));
		Vector3 vector = normalized.RotatedBy(angle) * 0.17f * Mathf.Sqrt(pawn.BodySize);
		Vector3 vector2 = drawPos + FootprintOffset + vector;
		IntVec3 c = vector2.ToIntVec3();
		if (c.InBounds(pawn.Map))
		{
			TerrainDef terrain = c.GetTerrain(pawn.Map);
			if (terrain != null)
			{
				if (terrain.takeSplashes)
				{
					FleckMaker.WaterSplash(vector2, pawn.Map, Mathf.Sqrt(pawn.BodySize) * 2f, 1.5f);
				}
				if (pawn.RaceProps.makesFootprints && terrain.takeFootprints && pawn.Map.snowGrid.GetDepth(pawn.Position) >= 0.4f)
				{
					FleckMaker.PlaceFootprint(vector2, pawn.Map, rot);
				}
			}
		}
		lastFootprintPlacePos = drawPos;
		lastFootprintRight = !lastFootprintRight;
	}
}
