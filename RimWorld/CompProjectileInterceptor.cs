using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompProjectileInterceptor : ThingComp
{
	private int lastInterceptTicks = -999999;

	private int startedChargingTick = -1;

	private bool shutDown;

	private StunHandler stunner;

	private Sustainer sustainer;

	public int currentHitPoints = -1;

	public int? maxHitPointsOverride;

	private int activatedTick = -999999;

	private float lastInterceptAngle;

	private bool drawInterceptCone;

	private bool debugInterceptNonHostileProjectiles;

	private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

	private static readonly Material ForceFieldConeMat = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);

	private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

	private const float TextureActualRingSizeFactor = 1.1601562f;

	private static readonly Color InactiveColor = new Color(0.2f, 0.2f, 0.2f);

	private static Material ShieldDotMat => MaterialPool.MatFrom("Things/Mote/ShieldDownDot", ShaderDatabase.MoteGlow);

	protected virtual int NumInactiveDots => 7;

	public CompProperties_ProjectileInterceptor Props => (CompProperties_ProjectileInterceptor)props;

	public bool Active
	{
		get
		{
			if (OnCooldown || Charging || stunner.Stunned || shutDown || currentHitPoints == 0 || !parent.Spawned)
			{
				return false;
			}
			if (Props.activated && Find.TickManager.TicksGame > activatedTick + Props.activeDuration)
			{
				return false;
			}
			if (parent is Pawn p && (p.IsCharging() || p.IsSelfShutdown()))
			{
				return false;
			}
			CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
			if (comp != null && !comp.Awake)
			{
				return false;
			}
			return true;
		}
	}

	public bool OnCooldown => Find.TickManager.TicksGame < lastInterceptTicks + Props.cooldownTicks;

	public bool Charging
	{
		get
		{
			if (startedChargingTick >= 0)
			{
				return Find.TickManager.TicksGame > startedChargingTick;
			}
			return false;
		}
	}

	public int ChargeCycleStartTick
	{
		get
		{
			if (startedChargingTick < 0)
			{
				return 0;
			}
			return startedChargingTick;
		}
	}

	public int ChargingTicksLeft
	{
		get
		{
			if (startedChargingTick < 0)
			{
				return 0;
			}
			return Mathf.Max(startedChargingTick + Props.chargeDurationTicks - Find.TickManager.TicksGame, 0);
		}
	}

	public int CooldownTicksLeft
	{
		get
		{
			if (!OnCooldown)
			{
				return 0;
			}
			return Props.cooldownTicks - (Find.TickManager.TicksGame - lastInterceptTicks);
		}
	}

	public bool ReactivatedThisTick => Find.TickManager.TicksGame - lastInterceptTicks == Props.cooldownTicks;

	public bool ShouldDisplayHitpointsGizmo
	{
		get
		{
			if (Props.alwaysShowHitpointsGizmo)
			{
				return true;
			}
			if (parent is Pawn pawn && (pawn.IsColonistPlayerControlled || pawn.RaceProps.IsMechanoid))
			{
				return HitPointsMax > 0;
			}
			return false;
		}
	}

	public int HitPointsMax => maxHitPointsOverride ?? Props.hitPoints;

	private int RemainingTicks => activatedTick + Props.activeDuration - Find.TickManager.TicksGame;

	protected virtual int HitPointsPerInterval => 1;

	public override void PostPostMake()
	{
		base.PostPostMake();
		if (Props.chargeIntervalTicks > 0)
		{
			startedChargingTick = Find.TickManager.TicksGame + Rand.Range(0, Props.chargeIntervalTicks);
		}
		if (HitPointsMax > 0)
		{
			if (Props.startWithMaxHitPoints)
			{
				currentHitPoints = HitPointsMax;
			}
			else
			{
				currentHitPoints = 0;
			}
		}
		stunner = new StunHandler(parent);
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		sustainer?.End();
	}

	public bool CheckIntercept(Projectile projectile, Vector3 lastExactPos, Vector3 newExactPos)
	{
		Vector3 vector = parent.Position.ToVector3Shifted();
		float num = Props.radius + projectile.def.projectile.SpeedTilesPerTick + 0.1f;
		if ((newExactPos.x - vector.x) * (newExactPos.x - vector.x) + (newExactPos.z - vector.z) * (newExactPos.z - vector.z) > num * num)
		{
			return false;
		}
		if (!Active)
		{
			return false;
		}
		if (!InterceptsProjectile(Props, projectile))
		{
			return false;
		}
		if (projectile.Launcher == null && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
		{
			return false;
		}
		if (parent.Faction != null)
		{
			if (projectile.Launcher != null && projectile.Launcher.Spawned && !projectile.Launcher.HostileTo(parent.Faction))
			{
				return false;
			}
			if (projectile.Launcher != null && !projectile.Launcher.Spawned && !projectile.Launcher.Faction.HostileTo(parent.Faction))
			{
				return false;
			}
		}
		if (!Props.interceptOutgoingProjectiles && (new Vector2(vector.x, vector.z) - new Vector2(lastExactPos.x, lastExactPos.z)).sqrMagnitude <= Props.radius * Props.radius)
		{
			return false;
		}
		if (!GenGeo.IntersectLineCircleOutline(new Vector2(vector.x, vector.z), Props.radius, new Vector2(lastExactPos.x, lastExactPos.z), new Vector2(newExactPos.x, newExactPos.z)))
		{
			return false;
		}
		lastInterceptAngle = lastExactPos.AngleToFlat(parent.TrueCenter());
		lastInterceptTicks = Find.TickManager.TicksGame;
		drawInterceptCone = true;
		TriggerEffecter(newExactPos.ToIntVec3());
		if (projectile.DamageDef == DamageDefOf.EMP && Props.disarmedByEmpForTicks > 0)
		{
			BreakShieldEmp(new DamageInfo(projectile.DamageDef, projectile.DamageDef.defaultDamage));
		}
		if (currentHitPoints > 0)
		{
			currentHitPoints -= projectile.DamageAmount;
			if (currentHitPoints < 0)
			{
				currentHitPoints = 0;
			}
			if (currentHitPoints == 0)
			{
				startedChargingTick = Find.TickManager.TicksGame;
				BreakShieldHitpoints(new DamageInfo(projectile.DamageDef, projectile.DamageDef.defaultDamage));
				return true;
			}
		}
		return true;
	}

	public bool CheckBombardmentIntercept(Bombardment bombardment, Bombardment.BombardmentProjectile projectile)
	{
		if (!Active || !Props.interceptAirProjectiles)
		{
			return false;
		}
		if (!projectile.targetCell.InHorDistOf(parent.Position, Props.radius))
		{
			return false;
		}
		if ((bombardment.instigator == null || !bombardment.instigator.HostileTo(parent)) && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
		{
			return false;
		}
		lastInterceptTicks = Find.TickManager.TicksGame;
		drawInterceptCone = false;
		TriggerEffecter(projectile.targetCell);
		return true;
	}

	public bool BombardmentCanStartFireAt(Bombardment bombardment, IntVec3 cell)
	{
		if (!Active || !Props.interceptAirProjectiles)
		{
			return true;
		}
		if ((bombardment.instigator == null || !bombardment.instigator.HostileTo(parent)) && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
		{
			return true;
		}
		return !cell.InHorDistOf(parent.Position, Props.radius);
	}

	private void TriggerEffecter(IntVec3 pos)
	{
		Effecter effecter = new Effecter(Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
		effecter.Trigger(new TargetInfo(pos, parent.Map), TargetInfo.Invalid);
		effecter.Cleanup();
	}

	public static bool InterceptsProjectile(CompProperties_ProjectileInterceptor props, Projectile projectile)
	{
		if (props.interceptGroundProjectiles && props.interceptAirProjectiles)
		{
			return true;
		}
		if (props.interceptGroundProjectiles)
		{
			return !projectile.def.projectile.flyOverhead;
		}
		if (props.interceptAirProjectiles)
		{
			return projectile.def.projectile.flyOverhead;
		}
		return false;
	}

	public override void CompTick()
	{
		if (HitPointsMax > 0 && ChargingTicksLeft == 0)
		{
			if (currentHitPoints == 0 && Props.hitPointsRestoreInstantlyAfterCharge)
			{
				currentHitPoints = HitPointsMax;
			}
			if (parent.IsHashIntervalTick(Props.rechargeHitPointsIntervalTicks))
			{
				currentHitPoints = Mathf.Clamp(currentHitPoints + HitPointsPerInterval, 0, HitPointsMax);
			}
			if (startedChargingTick > 0)
			{
				startedChargingTick = -1;
			}
		}
		if (Props.activated && !Charging && Find.TickManager.TicksGame == activatedTick + Props.activeDuration)
		{
			startedChargingTick = Find.TickManager.TicksGame;
		}
		if (ReactivatedThisTick && Props.reactivateEffect != null)
		{
			Props.reactivateEffect.Spawn(parent, parent.MapHeld).Cleanup();
		}
		if (Props.chargeIntervalTicks > 0 && Find.TickManager.TicksGame >= startedChargingTick + Props.chargeDurationTicks)
		{
			startedChargingTick += Props.chargeIntervalTicks;
		}
		stunner.StunHandlerTick();
		if (Props.activeSound.NullOrUndefined())
		{
			return;
		}
		if (Active)
		{
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(parent));
			}
			sustainer.Maintain();
		}
		else if (sustainer != null && !sustainer.Ended)
		{
			sustainer.End();
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		base.PostDrawExtraSelectionOverlays();
		if (!Active && !WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			for (int i = 0; i < NumInactiveDots; i++)
			{
				Vector3 vector = new Vector3(0f, 0f, 1f).RotatedBy((float)i / (float)NumInactiveDots * 360f) * (Props.radius * 0.966f);
				Vector3 vector2 = parent.DrawPos + vector;
				Graphics.DrawMesh(MeshPool.plane10, new Vector3(vector2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), vector2.z), Quaternion.identity, ShieldDotMat, 0);
			}
		}
	}

	public override void Notify_LordDestroyed()
	{
		base.Notify_LordDestroyed();
		shutDown = true;
	}

	public override void PostDraw()
	{
		base.PostDraw();
		Vector3 drawPos = parent.DrawPos;
		drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
		float currentAlpha = GetCurrentAlpha();
		if (currentAlpha > 0f)
		{
			Color value = ((!Active && Find.Selector.IsSelected(parent)) ? InactiveColor : Props.color);
			value.a *= currentAlpha;
			MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawPos, Quaternion.identity, new Vector3(Props.radius * 2f * 1.1601562f, 1f, Props.radius * 2f * 1.1601562f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, ForceFieldMat, 0, null, 0, MatPropertyBlock);
		}
		float currentConeAlpha_RecentlyIntercepted = GetCurrentConeAlpha_RecentlyIntercepted();
		if (currentConeAlpha_RecentlyIntercepted > 0f)
		{
			Color color = Props.color;
			color.a *= currentConeAlpha_RecentlyIntercepted;
			MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
			Matrix4x4 matrix2 = default(Matrix4x4);
			matrix2.SetTRS(drawPos, Quaternion.Euler(0f, lastInterceptAngle - 90f, 0f), new Vector3(Props.radius * 2f * 1.1601562f, 1f, Props.radius * 2f * 1.1601562f));
			Graphics.DrawMesh(MeshPool.plane10, matrix2, ForceFieldConeMat, 0, null, 0, MatPropertyBlock);
		}
	}

	private float GetCurrentAlpha()
	{
		return Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(GetCurrentAlpha_Idle(), GetCurrentAlpha_Selected()), GetCurrentAlpha_RecentlyIntercepted()), GetCurrentAlpha_RecentlyActivated()), Props.minAlpha);
	}

	private float GetCurrentAlpha_Idle()
	{
		float idlePulseSpeed = Props.idlePulseSpeed;
		float minIdleAlpha = Props.minIdleAlpha;
		if (!Active)
		{
			return 0f;
		}
		if (parent.Faction == Faction.OfPlayer && !debugInterceptNonHostileProjectiles)
		{
			return 0f;
		}
		if (Find.Selector.IsSelected(parent))
		{
			return 0f;
		}
		return Mathf.Lerp(minIdleAlpha, 0.11f, (Mathf.Sin((float)(Gen.HashCombineInt(parent.thingIDNumber, 96804938) % 100) + Time.realtimeSinceStartup * idlePulseSpeed) + 1f) / 2f);
	}

	private float GetCurrentAlpha_Selected()
	{
		float num = Mathf.Max(2f, Props.idlePulseSpeed);
		if ((!Find.Selector.IsSelected(parent) && !Props.drawWithNoSelection) || !Active)
		{
			return 0f;
		}
		return Mathf.Lerp(0.2f, 0.62f, (Mathf.Sin((float)(Gen.HashCombineInt(parent.thingIDNumber, 35990913) % 100) + Time.realtimeSinceStartup * num) + 1f) / 2f);
	}

	private float GetCurrentAlpha_RecentlyIntercepted()
	{
		int num = Find.TickManager.TicksGame - lastInterceptTicks;
		return Mathf.Clamp01(1f - (float)num / 40f) * 0.09f;
	}

	private float GetCurrentAlpha_RecentlyActivated()
	{
		if (!Active)
		{
			return 0f;
		}
		int num = Find.TickManager.TicksGame - (lastInterceptTicks + Props.cooldownTicks);
		return Mathf.Clamp01(1f - (float)num / 50f) * 0.09f;
	}

	private float GetCurrentConeAlpha_RecentlyIntercepted()
	{
		if (!drawInterceptCone)
		{
			return 0f;
		}
		int num = Find.TickManager.TicksGame - lastInterceptTicks;
		return Mathf.Clamp01(1f - (float)num / 40f) * 0.82f;
	}

	public void Activate()
	{
		activatedTick = Find.TickManager.TicksGame;
	}

	public void Deactivate()
	{
		activatedTick = Find.TickManager.TicksGame - Props.activeDuration;
		startedChargingTick = Find.TickManager.TicksGame;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (ShouldDisplayHitpointsGizmo)
		{
			Gizmo_ProjectileInterceptorHitPoints gizmo_ProjectileInterceptorHitPoints = new Gizmo_ProjectileInterceptorHitPoints();
			gizmo_ProjectileInterceptorHitPoints.interceptor = this;
			yield return gizmo_ProjectileInterceptorHitPoints;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		if (OnCooldown)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset cooldown",
				action = delegate
				{
					lastInterceptTicks = Find.TickManager.TicksGame - Props.cooldownTicks;
				}
			};
		}
		yield return new Command_Toggle
		{
			defaultLabel = "DEV: Intercept non-hostile",
			isActive = () => debugInterceptNonHostileProjectiles,
			toggleAction = delegate
			{
				debugInterceptNonHostileProjectiles = !debugInterceptNonHostileProjectiles;
			}
		};
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Props.interceptGroundProjectiles || Props.interceptAirProjectiles)
		{
			string text = ((Props.interceptGroundProjectiles && Props.interceptAirProjectiles) ? ((string)"InterceptsProjectiles_BothProjectiles".Translate()) : ((!Props.interceptGroundProjectiles) ? ((string)"InterceptsProjectiles_AerialProjectiles".Translate()) : ((string)"InterceptsProjectiles_GroundProjectiles".Translate())));
			if (Props.cooldownTicks > 0)
			{
				stringBuilder.AppendTagged("InterceptsProjectilesEvery".Translate(text, Props.cooldownTicks.ToStringTicksToPeriod()));
			}
			else
			{
				stringBuilder.Append("InterceptsProjectiles".Translate(text));
			}
		}
		if (Props.activated && Active)
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.AppendTagged("ShieldDuration".Translate() + ": " + RemainingTicks.ToStringSecondsFromTicks("F0"));
		}
		if (OnCooldown)
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.AppendTagged("CooldownTime".Translate() + ": " + CooldownTicksLeft.ToStringTicksToPeriod());
		}
		if (stunner.Stunned)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendTagged("DisarmedTime".Translate() + ": " + stunner.StunTicksLeft.ToStringTicksToPeriod());
		}
		if (shutDown)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("ShutDown".Translate());
		}
		else if (Props.chargeIntervalTicks > 0)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			if (Charging)
			{
				stringBuilder.AppendTagged("ChargingTime".Translate() + ": " + ChargingTicksLeft.ToStringTicksToPeriod());
			}
			else
			{
				stringBuilder.AppendTagged("ChargingNext".Translate((ChargeCycleStartTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(), Props.chargeDurationTicks.ToStringTicksToPeriod(), Props.chargeIntervalTicks.ToStringTicksToPeriod()));
			}
		}
		return stringBuilder.ToString();
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		base.PostPreApplyDamage(ref dinfo, out absorbed);
		if (dinfo.Def == DamageDefOf.EMP && Props.disarmedByEmpForTicks > 0)
		{
			BreakShieldEmp(dinfo);
		}
	}

	private void BreakShieldEmp(DamageInfo dinfo)
	{
		float fTheta;
		Vector3 center;
		if (Active)
		{
			EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, Props.radius);
			int num = Mathf.CeilToInt(Props.radius * 2f);
			fTheta = MathF.PI * 2f / (float)num;
			center = parent.TrueCenter();
			for (int i = 0; i < num; i++)
			{
				FleckMaker.ConnectingLine(PosAtIndex(i), PosAtIndex((i + 1) % num), FleckDefOf.LineEMP, parent.Map, 1.5f);
			}
		}
		dinfo.SetAmount((float)Props.disarmedByEmpForTicks / 30f);
		stunner.Notify_DamageApplied(dinfo);
		Vector3 PosAtIndex(int index)
		{
			return new Vector3(Props.radius * Mathf.Cos(fTheta * (float)index) + center.x, 0f, Props.radius * Mathf.Sin(fTheta * (float)index) + center.z);
		}
	}

	private void BreakShieldHitpoints(DamageInfo dinfo)
	{
		EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, Props.radius);
		stunner.Notify_DamageApplied(dinfo);
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastInterceptTicks, "lastInterceptTicks", -999999);
		Scribe_Values.Look(ref shutDown, "shutDown", defaultValue: false);
		Scribe_Values.Look(ref startedChargingTick, "nextChargeTick", -1);
		Scribe_Deep.Look(ref stunner, "stunner", parent);
		Scribe_Values.Look(ref currentHitPoints, "currentHitPoints", -1);
		Scribe_Values.Look(ref maxHitPointsOverride, "maxHitPointsOverride");
		Scribe_Values.Look(ref activatedTick, "activatedTick", -999999);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (Props.chargeIntervalTicks > 0 && startedChargingTick <= 0)
			{
				startedChargingTick = Find.TickManager.TicksGame + Rand.Range(0, Props.chargeIntervalTicks);
			}
			if (stunner == null)
			{
				stunner = new StunHandler(parent);
			}
		}
	}
}
