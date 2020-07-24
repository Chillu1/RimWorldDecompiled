using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Psycast : Ability
	{
		private Mote moteCast;

		private Sustainer soundCast;

		private static float MoteCastFadeTime = 0.4f;

		private static float MoteCastScale = 1f;

		private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.48f);

		public override bool CanCast
		{
			get
			{
				if (!base.CanCast)
				{
					return false;
				}
				if (def.EntropyGain > float.Epsilon)
				{
					Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == HediffDefOf.PsychicAmplifier);
					if ((hediff == null || hediff.Severity < (float)def.level) && def.level > 0)
					{
						return false;
					}
					return !pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain);
				}
				if (def.PsyfocusCost > pawn.psychicEntropy.CurrentPsyfocus)
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
			if (gizmo == null)
			{
				gizmo = new Command_Psycast(this);
			}
			yield return gizmo;
		}

		public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (def.EntropyGain > float.Epsilon && !pawn.psychicEntropy.TryAddEntropy(def.EntropyGain))
			{
				return false;
			}
			if (def.PsyfocusCost > float.Epsilon)
			{
				pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - def.PsyfocusCost);
			}
			bool flag = base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic);
			if (flag)
			{
				if (def.HasAreaOfEffect)
				{
					MoteMaker.MakeStaticMote(target.Cell, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, def.EffectRadius);
					SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
				}
				else
				{
					SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
				}
			}
			else if (def.HasAreaOfEffect)
			{
				SoundDefOf.PsycastSkipPulse.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
			}
			else
			{
				SoundDefOf.PsycastSkipEffect.PlayOneShot(new TargetInfo(target.Cell, pawn.Map));
			}
			if (target.Thing != pawn)
			{
				MoteMaker.MakeConnectingLine(pawn.DrawPos, target.CenterVector3, flag ? ThingDefOf.Mote_PsycastPsychicLine : ThingDefOf.Mote_PsycastSkipLine, pawn.Map);
			}
			return base.Activate(target, dest);
		}

		protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (CanApplyPsycastTo(target))
			{
				foreach (CompAbilityEffect effect in effects)
				{
					effect.Apply(target, dest);
				}
			}
			else
			{
				MoteMaker.ThrowText(target.CenterVector3, pawn.Map, "TextMote_Immune".Translate());
			}
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
				if (pawn.Faction == Faction.OfMechanoids && base.EffectComps.Any((CompAbilityEffect e) => !e.Props.applicableToMechs))
				{
					return false;
				}
			}
			return true;
		}

		public override bool GizmoDisabled(out string reason)
		{
			if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon)
			{
				reason = "CommandPsycastZeroPsychicSensitivity".Translate();
				return true;
			}
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier);
			if ((firstHediffOfDef == null || firstHediffOfDef.Severity < (float)def.level) && def.level > 0)
			{
				reason = "CommandPsycastHigherLevelPsylinkRequired".Translate(def.level);
				return true;
			}
			if (def.level > pawn.psychicEntropy.MaxAbilityLevel)
			{
				reason = "CommandPsycastLowPsyfocus".Translate(Pawn_PsychicEntropyTracker.PsyfocusBandPercentages[def.RequiredPsyfocusBand].ToStringPercent());
				return true;
			}
			if (def.PsyfocusCost > pawn.psychicEntropy.CurrentPsyfocus)
			{
				reason = "CommandPsycastNotEnoughPsyfocus".Translate(def.PsyfocusCost.ToStringPercent(), pawn.psychicEntropy.CurrentPsyfocus.ToStringPercent(), def.label.Named("PSYCASTNAME"), pawn.Named("CASTERNAME"));
				return true;
			}
			if (def.EntropyGain > float.Epsilon && pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(pawn)))
			{
				reason = "CommandPsycastWouldExceedEntropy".Translate(def.label);
				return true;
			}
			return base.GizmoDisabled(out reason);
		}

		public override void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
		{
			base.QueueCastingJob(target, destination);
			if (moteCast == null || moteCast.Destroyed)
			{
				moteCast = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_CastPsycast, MoteCastOffset, MoteCastScale, base.verb.verbProps.warmupTime - MoteCastFadeTime);
			}
		}

		public override void AbilityTick()
		{
			base.AbilityTick();
			if (moteCast != null && !moteCast.Destroyed && base.verb.WarmingUp)
			{
				moteCast.Maintain();
			}
			if (base.verb.WarmingUp)
			{
				if (soundCast == null || soundCast.Ended)
				{
					soundCast = SoundDefOf.PsycastCastLoop.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
				}
				else
				{
					soundCast.Maintain();
				}
			}
		}
	}
}
