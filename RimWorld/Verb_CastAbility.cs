using UnityEngine;
using Verse;

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
			if (!CanHitTarget(target))
			{
				if (target.IsValid)
				{
					Messages.Message(ability.def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput);
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

		public override void OnGUI(LocalTargetInfo target)
		{
			if (CanHitTarget(target) && IsApplicableTo(target))
			{
				base.OnGUI(target);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}

		public void DrawRadius()
		{
			GenDraw.DrawRadiusRing(ability.pawn.Position, verbProps.range);
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			AbilityDef def = ability.def;
			DrawRadius();
			if (CanHitTarget(target) && IsApplicableTo(target))
			{
				if (def.HasAreaOfEffect)
				{
					if (target.IsValid)
					{
						GenDraw.DrawTargetHighlight(target);
						GenDraw.DrawRadiusRing(target.Cell, def.EffectRadius, RadiusHighlightColor);
					}
				}
				else
				{
					GenDraw.DrawTargetHighlight(target);
				}
			}
			if (target.IsValid)
			{
				ability.DrawEffectPreviews(target);
			}
		}
	}
}
