using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Verb_CastAbility : Verb
	{
		public Ability ability;

		public static Color RadiusHighlightColor => new Color(0.3f, 0.8f, 1f);

		public override string ReportLabel => ability.def.label;

		public override bool MultiSelect => true;

		public override ITargetingSource DestinationSelector
		{
			get
			{
				CompAbilityEffect_WithDest compAbilityEffect_WithDest = ability.CompOfType<CompAbilityEffect_WithDest>();
				if (compAbilityEffect_WithDest != null && compAbilityEffect_WithDest.Props.destination == AbilityEffectDestination.Selected)
				{
					return compAbilityEffect_WithDest;
				}
				return null;
			}
		}

		public override Texture2D UIIcon => ability.def.uiIcon;

		protected override bool TryCastShot()
		{
			return ability.Activate(currentTarget, currentDestination);
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			CompAbilityEffect_WithDest compAbilityEffect_WithDest = ability.CompOfType<CompAbilityEffect_WithDest>();
			if (compAbilityEffect_WithDest != null && compAbilityEffect_WithDest.Props.destination == AbilityEffectDestination.Selected)
			{
				compAbilityEffect_WithDest.SetTarget(target);
			}
			else
			{
				ability.QueueCastingJob(target, null);
			}
		}

		public virtual bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false)
		{
			return true;
		}

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			if (verbProps.range > 0f)
			{
				if (!CanHitTarget(target))
				{
					if (target.IsValid)
					{
						Messages.Message(ability.def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), new LookTargets(ability.pawn, target.ToTargetInfo(ability.pawn.Map)), MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
			}
			else if (!ability.pawn.CanReach(target, PathEndMode.Touch, ability.pawn.NormalMaxDanger()))
			{
				if (target.IsValid)
				{
					Messages.Message(ability.def.LabelCap + ": " + "AbilityCannotReachTarget".Translate(), new LookTargets(ability.pawn, target.ToTargetInfo(ability.pawn.Map)), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (!IsApplicableTo(target, showMessages: true))
			{
				return false;
			}
			for (int i = 0; i < ability.EffectComps.Count; i++)
			{
				if (!ability.EffectComps[i].Valid(target, throwMessages: true))
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanHitTarget(LocalTargetInfo targ)
		{
			if (verbProps.range <= 0f)
			{
				return true;
			}
			return base.CanHitTarget(targ);
		}

		public override void OnGUI(LocalTargetInfo target)
		{
			if (CanHitTarget(target) && IsApplicableTo(target) && ValidateTarget(target))
			{
				base.OnGUI(target);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
			DrawAttachmentExtraLabel(target);
		}

		protected void DrawAttachmentExtraLabel(LocalTargetInfo target)
		{
			foreach (CompAbilityEffect effectComp in ability.EffectComps)
			{
				string text = effectComp.ExtraLabel(target);
				if (!text.NullOrEmpty())
				{
					Widgets.MouseAttachedLabel(text);
					break;
				}
			}
		}

		public void DrawRadius()
		{
			if (ability.pawn.Spawned)
			{
				GenDraw.DrawRadiusRing(ability.pawn.Position, verbProps.range);
			}
		}

		public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
		{
			bool num = base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns);
			Pawn pawn;
			if (num && ability.def.stunTargetWhileCasting && ability.def.verbProperties.warmupTime > 0f && (pawn = castTarg.Thing as Pawn) != null && pawn != ability.pawn)
			{
				pawn.stances.stunner.StunFor_NewTmp(ability.def.verbProperties.warmupTime.SecondsToTicks(), ability.pawn, addBattleLog: false, showMote: false);
			}
			return num;
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			AbilityDef def = ability.def;
			if (verbProps.range > 0f)
			{
				DrawRadius();
			}
			if (CanHitTarget(target) && IsApplicableTo(target))
			{
				if (def.HasAreaOfEffect)
				{
					if (target.IsValid)
					{
						GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
						GenDraw.DrawRadiusRing(target.Cell, def.EffectRadius, RadiusHighlightColor);
					}
				}
				else
				{
					GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
				}
			}
			if (target.IsValid)
			{
				ability.DrawEffectPreviews(target);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref ability, "ability");
		}
	}
}
