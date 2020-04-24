using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompOrbitalBeam : ThingComp
	{
		private int startTick;

		private int totalDuration;

		private int fadeOutDuration;

		private float angle;

		private Sustainer sustainer;

		private const float AlphaAnimationSpeed = 0.3f;

		private const float AlphaAnimationStrength = 0.025f;

		private const float BeamEndHeightRatio = 0.5f;

		private static readonly Material BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

		private static readonly Material BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);

		private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

		public CompProperties_OrbitalBeam Props => (CompProperties_OrbitalBeam)props;

		private int TicksPassed => Find.TickManager.TicksGame - startTick;

		private int TicksLeft => totalDuration - TicksPassed;

		private float BeamEndHeight => Props.width * 0.5f;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Values.Look(ref totalDuration, "totalDuration", 0);
			Scribe_Values.Look(ref fadeOutDuration, "fadeOutDuration", 0);
			Scribe_Values.Look(ref angle, "angle", 0f);
		}

		public void StartAnimation(int totalDuration, int fadeOutDuration, float angle)
		{
			startTick = Find.TickManager.TicksGame;
			this.totalDuration = totalDuration;
			this.fadeOutDuration = fadeOutDuration;
			this.angle = angle;
			CheckSpawnSustainer();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			CheckSpawnSustainer();
		}

		public override void CompTick()
		{
			base.CompTick();
			if (sustainer != null)
			{
				sustainer.Maintain();
				if (TicksLeft < fadeOutDuration)
				{
					sustainer.End();
					sustainer = null;
				}
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (TicksLeft > 0)
			{
				Vector3 drawPos = parent.DrawPos;
				float num = ((float)parent.Map.Size.z - drawPos.z) * 1.41421354f;
				Vector3 a = Vector3Utility.FromAngleFlat(angle - 90f);
				Vector3 a2 = drawPos + a * num * 0.5f;
				a2.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				float num2 = Mathf.Min((float)TicksPassed / 10f, 1f);
				Vector3 b = a * ((1f - num2) * num);
				float num3 = 0.975f + Mathf.Sin((float)TicksPassed * 0.3f) * 0.025f;
				if (TicksLeft < fadeOutDuration)
				{
					num3 *= (float)TicksLeft / (float)fadeOutDuration;
				}
				Color color = Props.color;
				color.a *= num3;
				MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(a2 + a * BeamEndHeight * 0.5f + b, Quaternion.Euler(0f, angle, 0f), new Vector3(Props.width, 1f, num));
				Graphics.DrawMesh(MeshPool.plane10, matrix, BeamMat, 0, null, 0, MatPropertyBlock);
				Vector3 pos = drawPos + b;
				pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				Matrix4x4 matrix2 = default(Matrix4x4);
				matrix2.SetTRS(pos, Quaternion.Euler(0f, angle, 0f), new Vector3(Props.width, 1f, BeamEndHeight));
				Graphics.DrawMesh(MeshPool.plane10, matrix2, BeamEndMat, 0, null, 0, MatPropertyBlock);
			}
		}

		private void CheckSpawnSustainer()
		{
			if (TicksLeft >= fadeOutDuration && Props.sound != null)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					sustainer = Props.sound.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
				});
			}
		}
	}
}
