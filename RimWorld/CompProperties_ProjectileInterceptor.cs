using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_ProjectileInterceptor : CompProperties
{
	public float radius;

	public int cooldownTicks;

	public int disarmedByEmpForTicks;

	public bool interceptGroundProjectiles;

	public bool interceptAirProjectiles;

	public bool interceptNonHostileProjectiles;

	public bool interceptOutgoingProjectiles;

	public bool drawWithNoSelection;

	public int chargeIntervalTicks;

	public int chargeDurationTicks;

	public float minAlpha;

	public float idlePulseSpeed = 0.7f;

	public float minIdleAlpha = -1.7f;

	public int hitPoints = -1;

	public int rechargeHitPointsIntervalTicks = 240;

	[NoTranslate]
	public string gizmoTipKey;

	public bool hitPointsRestoreInstantlyAfterCharge;

	public bool startWithMaxHitPoints = true;

	public bool alwaysShowHitpointsGizmo;

	public bool activated;

	public int activeDuration;

	public Color color = Color.white;

	public EffecterDef reactivateEffect;

	public EffecterDef interceptEffect;

	public SoundDef activeSound;

	public CompProperties_ProjectileInterceptor()
	{
		compClass = typeof(CompProjectileInterceptor);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (hitPoints > 0 && chargeIntervalTicks > 0)
		{
			yield return "Cannot set both hitpoints and charge interval ticks.";
		}
	}
}
