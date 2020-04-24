using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompRadialPulse : ThingComp
	{
		private static readonly Material RingMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

		private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

		private const float TextureActualRingSizeFactor = 1.16015625f;

		private CompProperties_RadialPulse Props => (CompProperties_RadialPulse)props;

		private float RingLerpFactor => (float)(Find.TickManager.TicksGame % Props.ticksBetweenPulses) / (float)Props.ticksPerPulse;

		private float RingScale => Props.radius * Mathf.Lerp(0f, 2f, RingLerpFactor) * 1.16015625f;

		private bool ParentIsActive => parent.GetComp<CompSendSignalOnPawnProximity>()?.Sent ?? false;

		public override void PostDraw()
		{
			if (!ParentIsActive)
			{
				Vector3 pos = parent.Position.ToVector3Shifted();
				pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				Color color = Props.color;
				color.a = Mathf.Lerp(Props.color.a, 0f, RingLerpFactor);
				MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(pos, Quaternion.identity, new Vector3(RingScale, 1f, RingScale));
				Graphics.DrawMesh(MeshPool.plane10, matrix, RingMat, 0, null, 0, MatPropertyBlock);
			}
		}
	}
}
