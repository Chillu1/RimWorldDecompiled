using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRitualEffect_Lightball : CompRitualEffect_IntervalSpawn
	{
		public new CompProperties_RitualEffectLightball Props => (CompProperties_RitualEffectLightball)props;

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return Vector3.zero;
		}

		public override void SpawnFleck(LordJob_Ritual ritual, Vector3? forcedPos = null, float? exactRotation = null)
		{
			CompPowerTrader compPowerTrader = ritual.selectedTarget.Thing.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && compPowerTrader.PowerOn)
			{
				float num = Rand.Range(0f, 360f);
				float num2 = num + 180f;
				float num3 = (num + num2) / 2f + (float)Rand.Range(-55, 55);
				Vector3 vector = parent.ritual.selectedTarget.Cell.ToVector3Shifted();
				Vector3 vector2 = Quaternion.AngleAxis(num, Vector3.up) * Vector3.forward * Props.radius;
				Vector3 vector3 = Quaternion.AngleAxis(num2, Vector3.up) * Vector3.forward * Props.radius;
				Vector3 vector4 = Quaternion.AngleAxis(num3, Vector3.up) * Vector3.forward * Props.radius;
				base.SpawnFleck(parent.ritual, vector + vector2, num - 45f);
				base.SpawnFleck(parent.ritual, vector + vector3, num2 - 45f);
				base.SpawnFleck(parent.ritual, vector + vector4, num3 - 45f);
				lastSpawnTick = GenTicks.TicksGame;
			}
		}
	}
}
