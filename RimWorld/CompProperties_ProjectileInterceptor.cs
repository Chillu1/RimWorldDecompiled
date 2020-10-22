using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_ProjectileInterceptor : CompProperties
	{
		public float radius;

		public int cooldownTicks;

		public int disarmedByEmpForTicks;

		public bool interceptGroundProjectiles;

		public bool interceptAirProjectiles;

		public bool interceptNonHostileProjectiles;

		public bool interceptOutgoingProjectiles;

		public int chargeIntervalTicks;

		public int chargeDurationTicks;

		public float minAlpha;

		public float idlePulseSpeed = 0.7f;

		public float minIdleAlpha = -1.7f;

		public Color color = Color.white;

		public EffecterDef reactivateEffect;

		public EffecterDef interceptEffect;

		public SoundDef activeSound;

		public CompProperties_ProjectileInterceptor()
		{
			compClass = typeof(CompProjectileInterceptor);
		}
	}
}
