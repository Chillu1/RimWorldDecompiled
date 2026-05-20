using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Stance_Warmup : Stance_Busy
{
	private Sustainer sustainer;

	protected Effecter effecter;

	private Mote aimLineMote;

	private Mote aimChargeMote;

	private Mote aimTargetMote;

	private bool targetStartedDowned;

	private bool drawAimPie = true;

	private bool needsReInitAfterLoad;

	public Stance_Warmup()
	{
	}

	public Stance_Warmup(int ticks, LocalTargetInfo focusTarg, Verb verb)
		: base(ticks, focusTarg, verb)
	{
		if (focusTarg.HasThing && focusTarg.Thing is Pawn pawn)
		{
			targetStartedDowned = pawn.Downed;
			if (pawn.apparel != null && (!(verb is Verb_CastAbility verb_CastAbility) || verb_CastAbility.Ability.def.hostile))
			{
				for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
				{
					List<ThingComp> allComps = pawn.apparel.WornApparel[i].AllComps;
					for (int j = 0; j < allComps.Count; j++)
					{
						if (allComps[j] is CompShield compShield)
						{
							compShield.KeepDisplaying();
						}
					}
				}
			}
		}
		InitEffects();
		drawAimPie = verb?.verbProps.drawAimPie ?? false;
	}

	public void InitEffects(bool afterReload = false)
	{
		if (verb == null)
		{
			return;
		}
		VerbProperties verbProps = verb.verbProps;
		if (verbProps.soundAiming != null)
		{
			SoundInfo info = SoundInfo.InMap(verb.caster, MaintenanceType.PerTick);
			if (verb.CasterIsPawn)
			{
				info.pitchFactor = 1f / verb.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
			}
			sustainer = verbProps.soundAiming.TrySpawnSustainer(info);
		}
		if (verbProps.warmupEffecter != null && verb.Caster != null)
		{
			effecter = verbProps.warmupEffecter.Spawn(verb.Caster, verb.Caster.Map);
			effecter.Trigger(verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
		}
		if (verb.Caster == null)
		{
			return;
		}
		Map map = verb.Caster.Map;
		if (verbProps.aimingLineMote != null)
		{
			Vector3 vector = TargetPos();
			IntVec3 cell = vector.ToIntVec3();
			aimLineMote = MoteMaker.MakeInteractionOverlay(verbProps.aimingLineMote, verb.Caster, new TargetInfo(cell, map), Vector3.zero, vector - cell.ToVector3Shifted());
			if (afterReload)
			{
				aimLineMote?.ForceSpawnTick(startedTick);
			}
		}
		if (verbProps.aimingChargeMote != null)
		{
			aimChargeMote = MoteMaker.MakeStaticMote(verb.Caster.DrawPos, map, verbProps.aimingChargeMote, 1f, makeOffscreen: true);
			if (afterReload)
			{
				aimChargeMote?.ForceSpawnTick(startedTick);
			}
		}
		if (verbProps.aimingTargetMote != null)
		{
			aimTargetMote = MoteMaker.MakeStaticMote(focusTarg.CenterVector3, map, verbProps.aimingTargetMote, 1f, makeOffscreen: true);
			if (aimTargetMote != null)
			{
				aimTargetMote.exactRotation = AimDir().ToAngleFlat();
				if (afterReload)
				{
					aimTargetMote.ForceSpawnTick(startedTick);
				}
			}
		}
		if (verbProps.aimingTargetEffecter != null)
		{
			effecter = verbProps.aimingTargetEffecter.Spawn(new TargetInfo(verb.Caster), focusTarg.ToTargetInfo(map));
		}
	}

	private Vector3 TargetPos()
	{
		VerbProperties verbProps = verb.verbProps;
		Vector3 result = focusTarg.CenterVector3;
		if (verbProps.aimingLineMoteFixedLength.HasValue)
		{
			result = verb.Caster.DrawPos + AimDir() * verbProps.aimingLineMoteFixedLength.Value;
		}
		return result;
	}

	private Vector3 AimDir()
	{
		Vector3 result = focusTarg.CenterVector3 - verb.Caster.DrawPos;
		result.y = 0f;
		result.Normalize();
		return result;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref targetStartedDowned, "targetStartDowned", defaultValue: false);
		Scribe_Values.Look(ref drawAimPie, "drawAimPie", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			needsReInitAfterLoad = true;
		}
	}

	public override void StanceDraw()
	{
		if (drawAimPie && Find.Selector.IsSelected(stanceTracker.pawn))
		{
			GenDraw.DrawAimPie(stanceTracker.pawn, focusTarg, (int)((float)ticksLeft * pieSizeFactor), 0.2f);
		}
	}

	public override void StanceTick()
	{
		if (needsReInitAfterLoad)
		{
			InitEffects(afterReload: true);
			needsReInitAfterLoad = false;
		}
		if (sustainer != null && !sustainer.Ended)
		{
			sustainer.Maintain();
		}
		effecter?.EffectTick(verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
		Vector3 vector = AimDir();
		float exactRotation = vector.AngleFlat();
		bool stunned = stanceTracker.stunner.Stunned;
		if (aimLineMote != null)
		{
			aimLineMote.paused = stunned;
			aimLineMote.Maintain();
			Vector3 vector2 = TargetPos();
			IntVec3 cell = vector2.ToIntVec3();
			((MoteDualAttached)aimLineMote).UpdateTargets(verb.Caster, new TargetInfo(cell, verb.Caster.Map), Vector3.zero, vector2 - cell.ToVector3Shifted());
		}
		if (aimTargetMote != null)
		{
			aimTargetMote.paused = stunned;
			aimTargetMote.exactPosition = focusTarg.CenterVector3;
			aimTargetMote.exactRotation = exactRotation;
			aimTargetMote?.Maintain();
		}
		if (aimChargeMote != null)
		{
			aimChargeMote.paused = stunned;
			aimChargeMote.exactRotation = exactRotation;
			aimChargeMote.exactPosition = verb.Caster.Position.ToVector3Shifted() + vector * verb.verbProps.aimingChargeMoteOffset;
			aimChargeMote?.Maintain();
		}
		if (!stanceTracker.stunner.Stunned)
		{
			if (!targetStartedDowned && focusTarg.HasThing && focusTarg.Thing is Pawn && ((Pawn)focusTarg.Thing).Downed)
			{
				stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (focusTarg.HasThing && (!focusTarg.Thing.Spawned || verb == null || !verb.CanHitTargetFrom(base.Pawn.Position, focusTarg)))
			{
				stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (focusTarg == base.Pawn.mindState.enemyTarget)
			{
				base.Pawn.mindState.Notify_EngagedTarget();
			}
		}
		base.StanceTick();
	}

	public void Interrupt()
	{
		base.Expire();
		effecter?.Cleanup();
	}

	protected override void Expire()
	{
		verb?.WarmupComplete();
		effecter?.Cleanup();
		base.Expire();
	}
}
