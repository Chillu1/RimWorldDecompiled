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

		public int chargeIntervalTicks;

		public int chargeDurationTicks;

		public Color color = Color.white;

		public EffecterDef reactivateEffect;

		public CompProperties_ProjectileInterceptor()
		{
			compClass = typeof(CompProjectileInterceptor);
		}
	}
}
