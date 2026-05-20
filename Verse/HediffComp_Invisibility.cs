using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffComp_Invisibility : HediffComp
{
	private int lastDisrupted = -99999;

	private bool wasForcedVisibleLastTick;

	private bool everVisible;

	private static readonly SimpleCurve RevealCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.1f, 0.5f),
		new CurvePoint(1f, 1f)
	};

	[Unsaved(false)]
	private CompHoldingPlatformTarget platformTarget;

	private CompHoldingPlatformTarget PlatformTarget => platformTarget ?? (platformTarget = base.Pawn.TryGetComp<CompHoldingPlatformTarget>());

	public HediffCompProperties_Invisibility Props => (HediffCompProperties_Invisibility)props;

	private float FadeIn => Mathf.Clamp01((float)(Find.TickManager.TicksGame - base.Pawn.mindState.lastBecameVisibleTick) / (float)Props.fadeDurationTicks);

	private float FadeOut => 1f - Mathf.Clamp01((float)(Find.TickManager.TicksGame - base.Pawn.mindState.lastBecameInvisibleTick) / (float)Props.fadeDurationTicks);

	private bool ShouldBeVisible => base.Pawn.mindState.lastBecameVisibleTick >= base.Pawn.mindState.lastBecameInvisibleTick;

	private float FadePct
	{
		get
		{
			if (!ShouldBeVisible)
			{
				return FadeOut;
			}
			return FadeIn;
		}
	}

	public bool PsychologicallyVisible
	{
		get
		{
			if (!ForcedVisible)
			{
				return FadePct > 0f;
			}
			return true;
		}
	}

	private bool ForcedVisible
	{
		get
		{
			if (base.Pawn.Downed)
			{
				return true;
			}
			if (Find.TickManager.TicksGame < lastDisrupted + Props.recoverFromDisruptedTicks)
			{
				return true;
			}
			if ((base.Pawn.Faction == null || !base.Pawn.Faction.IsPlayer) && base.Pawn.stances?.stunner?.Stunned == true)
			{
				return true;
			}
			if (base.Pawn.IsBurning())
			{
				return true;
			}
			if (Props.affectedByDisruptor && base.Pawn.health.hediffSet.HasHediff(HediffDefOf.DisruptorFlash))
			{
				return true;
			}
			if (base.Pawn.health.hediffSet.HasHediff(HediffDefOf.CoveredInFirefoam))
			{
				return true;
			}
			if (base.Pawn.ParentHolder is Pawn_CarryTracker)
			{
				return true;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = PlatformTarget;
			if (compHoldingPlatformTarget != null && compHoldingPlatformTarget.CurrentlyHeldOnPlatform)
			{
				return true;
			}
			return false;
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref wasForcedVisibleLastTick, "wasForcedVisibleLastTick", defaultValue: false);
		Scribe_Values.Look(ref lastDisrupted, "lastTookDamageTick", -99999);
		Scribe_Values.Look(ref everVisible, "everVisible", defaultValue: false);
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		BecomeInvisible(instant: true);
		UpdateTarget();
	}

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		UpdateTarget();
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (!ShouldBeVisible)
		{
			if (!wasForcedVisibleLastTick && ForcedVisible && FadePct == 0f)
			{
				base.Pawn.mindState.lastForcedVisibleTick = Find.TickManager.TicksGame;
				EffecterDefOf.ForcedVisible.Spawn(parent.pawn, parent.pawn.MapHeld).Cleanup();
				base.Pawn.Notify_ForcedVisible();
			}
			if (wasForcedVisibleLastTick && !ForcedVisible)
			{
				base.Pawn.mindState.lastBecameInvisibleTick = Find.TickManager.TicksGame;
				base.Pawn.Notify_BecameInvisible();
			}
		}
		wasForcedVisibleLastTick = ForcedVisible;
		if (!everVisible && PsychologicallyVisible)
		{
			everVisible = true;
			if (ModsConfig.AnomalyActive && !Props.visibleToPlayer && AnomalyUtility.ShouldNotifyCodex(base.Pawn, EntityDiscoveryType.BecameVisible, out var entries))
			{
				Find.EntityCodex.SetDiscovered(entries, base.Pawn.def, base.Pawn);
			}
			else
			{
				Find.HiddenItemsManager.SetDiscovered(base.Pawn.def);
			}
		}
	}

	private void UpdateTarget()
	{
		if (ModLister.CheckRoyaltyOrAnomaly("Invisibility hediff"))
		{
			Pawn pawn = parent.pawn;
			if (pawn.Spawned)
			{
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
			if (pawn.RaceProps.Humanlike)
			{
				PortraitsCache.SetDirty(pawn);
				GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
			}
		}
	}

	public void BecomeVisible(bool instant = false)
	{
		if (!ShouldBeVisible)
		{
			if (wasForcedVisibleLastTick)
			{
				instant = true;
			}
			if (instant)
			{
				EffecterDefOf.ForcedVisible.Spawn(parent.pawn, parent.pawn.Map).Cleanup();
				base.Pawn.mindState.lastBecameVisibleTick = Find.TickManager.TicksGame - Props.fadeDurationTicks;
			}
			else
			{
				base.Pawn.mindState.lastBecameVisibleTick = Find.TickManager.TicksGame;
			}
			if (!ForcedVisible && !wasForcedVisibleLastTick)
			{
				base.Pawn.Notify_BecameVisible();
			}
		}
	}

	public void BecomeInvisible(bool instant = false)
	{
		if (ShouldBeVisible)
		{
			if (instant)
			{
				base.Pawn.mindState.lastBecameInvisibleTick = Find.TickManager.TicksGame - Props.fadeDurationTicks;
			}
			else
			{
				base.Pawn.mindState.lastBecameInvisibleTick = Find.TickManager.TicksGame;
			}
			if (!ForcedVisible)
			{
				base.Pawn.Notify_BecameInvisible();
			}
			base.Pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public float GetAlpha()
	{
		if (Props.visibleToPlayer)
		{
			return 1f;
		}
		if (ForcedVisible)
		{
			return 1f;
		}
		return RevealCurve.Evaluate(FadePct);
	}

	public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		DisruptInvisibility();
	}

	public void DisruptInvisibility()
	{
		lastDisrupted = Find.TickManager.TicksGame;
	}
}
