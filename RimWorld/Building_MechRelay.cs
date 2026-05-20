using System;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_MechRelay : Building
{
	[Unsaved(false)]
	private Material cachedShadowMaterial;

	[Unsaved(false)]
	private CompMechRelay compMechRelay;

	private const float ZOffset = 0.55f;

	private const float BobHeight = 0.35f;

	private const float CrashTicks = 120f;

	private const float CrashRotation = 45f;

	private const float CrashProgressExponent = 2f;

	private Material ShadowMaterial
	{
		get
		{
			if (cachedShadowMaterial == null)
			{
				cachedShadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowDropPod", ShaderDatabase.Transparent);
			}
			return cachedShadowMaterial;
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (base.Spawned)
		{
			if (compMechRelay == null)
			{
				compMechRelay = GetComp<CompMechRelay>();
			}
			Skyfaller.DrawDropSpotShadow(drawLoc, Rot4.North, ShadowMaterial, def.size.ToVector2(), 65);
			float a = 0.55f + 0.5f * (1f + Mathf.Sin(MathF.PI * 2f * (float)GenTicks.TicksGame / 300f)) * 0.35f;
			float t = ((compMechRelay.DestabilizationTick == -1) ? 0f : Mathf.Pow(1f - Mathf.Clamp01((float)(compMechRelay.DestabilizationTick - Find.TickManager.TicksGame) / 120f), 2f));
			float num = Mathf.Lerp(a, 0f, t);
			float extraRotation = Mathf.Lerp(0f, 45f, t);
			drawLoc.z += num;
			Graphic.Draw(drawLoc, flip ? base.Rotation.Opposite : base.Rotation, this, extraRotation);
			SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);
		}
		else
		{
			base.DrawAt(drawLoc, flip);
		}
	}
}
