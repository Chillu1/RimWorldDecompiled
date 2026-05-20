using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAncientHeatVent : CompAncientVent
{
	private float targetGlow = 1f;

	private float intensity;

	private MaterialPropertyBlock glowProps;

	private MaterialPropertyBlock heatShimmerProps;

	private static int shaderPropertyIDIntensity;

	protected override bool AppliesEffectsToPawns => true;

	public new CompProperties_AncientHeatVent Props => (CompProperties_AncientHeatVent)props;

	protected override void ToggleIndividualVent(bool on)
	{
	}

	private void DrawFleckQuad(FleckDef def, MaterialPropertyBlock props)
	{
		GraphicData graphicData = def.graphicData;
		Vector3 pos = parent.Position.ToVector3Shifted() + graphicData.drawOffset;
		pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
		Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, (parent.DrawSize * graphicData.drawSize).ToVector3());
		Graphics.DrawMesh(MeshPool.plane10, matrix, graphicData.Graphic.MatSingle, 0, null, 0, props);
	}

	private void DrawGlow()
	{
		GraphicData graphicData = FleckDefOf.AncientVentHeatGlow.graphicData;
		glowProps = new MaterialPropertyBlock();
		glowProps.SetColor(ShaderPropertyIDs.Color, graphicData.color.WithAlpha(intensity * targetGlow * graphicData.color.a));
		DrawFleckQuad(FleckDefOf.AncientVentHeatGlow, glowProps);
	}

	private void DrawHeatShimmer()
	{
		if (heatShimmerProps == null)
		{
			heatShimmerProps = new MaterialPropertyBlock();
		}
		heatShimmerProps.SetFloat(shaderPropertyIDIntensity, intensity);
		DrawFleckQuad(FleckDefOf.AncientVentHeatShimmer, glowProps);
	}

	public override void PostDraw()
	{
		if (base.VentOn)
		{
			DrawGlow();
			DrawHeatShimmer();
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		intensity = (base.VentOn ? 1f : 0f);
		shaderPropertyIDIntensity = Shader.PropertyToID("_Intensity");
	}

	public override void CompTickInterval(int delta)
	{
		if (base.VentOn && intensity < 1f)
		{
			intensity += (float)delta * (1f / 60f) / Props.rampUpTime;
			if (intensity > 1f)
			{
				intensity = 1f;
			}
		}
		else
		{
			if (!(intensity > 0f))
			{
				return;
			}
			targetGlow = Mathf.Lerp(Props.minGlowBrightness, Props.maxGlowBrightness, 0.5f + 0.5f * Mathf.Sin((float)GenTicks.TicksGame * (1f / 60f) * Props.pulseSpeed));
			if (!base.VentOn)
			{
				intensity -= (float)delta * (1f / 60f) / Props.rampDownTime;
				if (intensity < 0f)
				{
					intensity = 0f;
				}
			}
		}
	}
}
