using System;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_Shadow : Graphic
{
	private Mesh shadowMesh;

	private ShadowData shadowInfo;

	[TweakValue("Graphics_Shadow", -5f, 5f)]
	private static float GlobalShadowPosOffsetX;

	[TweakValue("Graphics_Shadow", -5f, 5f)]
	private static float GlobalShadowPosOffsetZ;

	public Graphic_Shadow(ShadowData shadowInfo)
	{
		this.shadowInfo = shadowInfo;
		if (shadowInfo == null)
		{
			throw new ArgumentNullException("shadowInfo");
		}
		shadowMesh = ShadowMeshPool.GetShadowMesh(shadowInfo);
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		if (shadowMesh != null && thingDef != null && shadowInfo != null && (Find.CurrentMap == null || !Find.CurrentMap.Biome.disableShadows) && (Find.CurrentMap == null || !loc.ToIntVec3().InBounds(Find.CurrentMap) || !Find.CurrentMap.roofGrid.Roofed(loc.ToIntVec3())) && DebugViewSettings.drawShadows)
		{
			Vector3 position = loc + shadowInfo.offset;
			position.y = AltitudeLayer.Shadows.AltitudeFor();
			Graphics.DrawMesh(shadowMesh, position, rot.AsQuat, MatBases.SunShadowFade, 0);
		}
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		Vector3 center = thing.TrueCenter() + (shadowInfo.offset + new Vector3(GlobalShadowPosOffsetX, 0f, GlobalShadowPosOffsetZ)).RotatedBy(thing.Rotation);
		center.y = AltitudeLayer.Shadows.AltitudeFor();
		Printer_Shadow.PrintShadow(layer, center, shadowInfo, thing.Rotation);
	}

	public override string ToString()
	{
		return "Graphic_Shadow(" + shadowInfo?.ToString() + ")";
	}
}
