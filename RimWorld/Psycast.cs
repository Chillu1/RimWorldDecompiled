using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Psycast : Ability
{
	private Mote moteCast;

	private static float MoteCastFadeTime = 0.4f;

	private static float MoteCastScale = 1f;

	private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.48f);

	public override AcceptanceReport CanCast
	{
		get
		{
			AcceptanceReport canCast = base.CanCast;
			if (!canCast)
			{
				return canCast;
			}
			if (def.EntropyGain > float.Epsilon)
			{
				if (pawn.GetPsylinkLevel() < def.level && def.level > 0)
				{
					return false;
				}
				return !pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain);
			}
			if (def.PsyfocusCost > pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
			{
				return false;
			}
			return true;
		}
	}

	public Psycast(Pawn pawn)
		: base(pawn)
	{
	}

	public Psycast(Pawn pawn, AbilityDef def)
		: base(pawn, def)
	{
	}

	public override IEnumerable<Command> GetGizmos()
	{
		if (ModLister.RoyaltyInstalled)
		{
			if (gizmo == null)
			{
				gizmo = new Command_Psycast(this, pawn);
			}
			yield return gizmo;
		}
	}

	public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!ModLister.CheckRoyalty("Psycast"))
		{
			return false;
		}
		if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain))
		{
			return false;
		}
		float num = FinalPsyfocusCost(target);
		if (num > float.Epsilon)
		{
			pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - num);
		}
		if (def.showPsycastEffects)
		{
			if (base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic))
			{
				if (def.HasAreaOfEffect)
				{
					FleckMaker.Static(target.Cell, pawn.Map, FleckDefOf.PsycastAreaEffect, def.EffectRadius);
					SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
				}
				else
				{
					SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
				}
			}
			else if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
			{
				SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
			}
		}
		return base.Activate(target, dest);
	}

	public override bool Activate(GlobalTargetInfo target)
	{
		if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain))
		{
			return false;
		}
		float psyfocusCost = def.PsyfocusCost;
		if (psyfocusCost > float.Epsilon)
		{
			pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
		}
		return base.Activate(target);
	}

	protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (CanApplyPsycastTo(target))
		{
			foreach (CompAbilityEffect effect in effects)
			{
				effect.Apply(target, dest);
			}
			return;
		}
		MoteMaker.ThrowText(target.CenterVector3, pawn.Map, "TextMote_Immune".Translate());
	}

	public bool CanApplyPsycastTo(LocalTargetInfo target)
	{
		if (!base.EffectComps.Any((CompAbilityEffect e) => e.Props.psychic))
		{
			return true;
		}
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon)
			{
				return false;
			}
			if (pawn.Faction != null && pawn.Faction == Faction.OfMechanoids && base.EffectComps.Any((CompAbilityEffect e) => !e.Props.applicableToMechs))
			{
				return false;
			}
		}
		return true;
	}

	public override bool GizmoDisabled(out string reason)
	{
		if (pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
		{
			reason = "CommandPsycastZeroPsychicSensitivity".Translate();
			return true;
		}
		float num = PsycastUtility.TotalPsyfocusCostOfQueuedPsycasts(pawn);
		if (def.level > 0 && pawn.GetPsylinkLevel() < def.level)
		{
			reason = "CommandPsycastHigherLevelPsylinkRequired".Translate(def.level);
			return true;
		}
		if (def.PsyfocusCost + num > pawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
		{
			reason = "CommandPsycastNotEnoughPsyfocus".Translate(def.PsyfocusCostPercent, (pawn.psychicEntropy.CurrentPsyfocus - num).ToStringPercent("0.#"), def.label.Named("PSYCASTNAME"), pawn.Named("CASTERNAME"));
			return true;
		}
		if (def.level > pawn.psychicEntropy.MaxAbilityLevel)
		{
			reason = "CommandPsycastLowPsyfocus".Translate(Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[def.RequiredPsyfocusBand].ToStringPercent());
			return true;
		}
		if (def.EntropyGain > float.Epsilon && pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(pawn)))
		{
			reason = "CommandPsycastWouldExceedEntropy".Translate(def.label);
			return true;
		}
		return base.GizmoDisabled(out reason);
	}

	public override void AbilityTick()
	{
		base.AbilityTick();
		if (pawn.Spawned && base.Casting)
		{
			if (moteCast == null || moteCast.Destroyed)
			{
				moteCast = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_CastPsycast, MoteCastOffset, MoteCastScale, base.verb.verbProps.warmupTime - MoteCastFadeTime);
			}
			else
			{
				moteCast.Maintain();
			}
		}
	}
}
