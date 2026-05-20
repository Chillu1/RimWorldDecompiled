using UnityEngine;
using Verse;

namespace RimWorld;

public class Skyfaller_FlyingPawn : Skyfaller
{
	public Pawn Pawn => (Pawn)innerContainer[0];

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		AnimationDef bestFlyAnimation = Pawn_FlightTracker.GetBestFlyAnimation(Pawn, Rot4.FromAngleFlat(angle));
		if (bestFlyAnimation != null)
		{
			Pawn.Drawer.renderer.SetAnimation(bestFlyAnimation);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		GetDrawPositionAndRotation(ref drawLoc, out var _);
		Pawn?.DrawNowAt(drawLoc, flip);
		DrawDropSpotShadow();
	}

	protected override void SpawnThings()
	{
		Pawn pawn = Pawn;
		if (pawn != null)
		{
			GenSpawn.Spawn(pawn, base.Position, base.Map);
			pawn.Rotation = Rot4.East;
			pawn.Drawer.renderer.SetAnimation(null);
		}
	}

	protected override void DrawDropSpotShadow()
	{
		Material shadowMaterial = base.ShadowMaterial;
		if (shadowMaterial != null)
		{
			Vector3 drawPos = DrawPos;
			drawPos.y = AltitudeLayer.Shadows.AltitudeFor();
			drawPos.z = base.Position.ToVector3Shifted().z;
			Skyfaller.DrawDropSpotShadow(drawPos, base.Rotation, shadowMaterial, def.skyfaller.shadowSize, ticksToImpact);
		}
	}
}
