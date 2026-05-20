using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Verb_CastAbility : Verb, IAbilityVerb
{
	public Ability ability;

	public Ability Ability
	{
		get
		{
			return ability;
		}
		set
		{
			ability = value;
		}
	}

	public static Color RadiusHighlightColor => new Color(0.3f, 0.8f, 1f);

	public override string ReportLabel => ability.def.label;

	public override bool MultiSelect => true;

	public override bool HidePawnTooltips
	{
		get
		{
			foreach (CompAbilityEffect effectComp in ability.EffectComps)
			{
				if (effectComp.HideTargetPawnTooltip)
				{
					return true;
				}
			}
			return false;
		}
	}

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

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (EffectiveRange > 0f)
		{
			if (!CanHitTarget(target))
			{
				if (target.IsValid && showMessages)
				{
					string text = "CannotUseAbility".Translate(ability.def.label) + ": ";
					if (ability.verb.OutOfRange(ability.pawn.Position, target, target.HasThing ? target.Thing.OccupiedRect() : CellRect.SingleCell(target.Cell)))
					{
						Messages.Message(text + "AbilityOutOfRange".Translate(), new LookTargets(ability.pawn, target.ToTargetInfo(ability.pawn.Map)), MessageTypeDefOf.RejectInput, historical: false);
					}
					else if (ability.pawn.Spawned)
					{
						Messages.Message(text + "AbilityCannotHitTarget".Translate(), new LookTargets(ability.pawn, target.ToTargetInfo(ability.pawn.Map)), MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				return false;
			}
		}
		else if (!ability.pawn.CanReach(target, PathEndMode.Touch, ability.pawn.NormalMaxDanger()))
		{
			if (target.IsValid && showMessages)
			{
				Messages.Message("CannotUseAbility".Translate(ability.def.label) + ": " + "AbilityCannotReachTarget".Translate(), new LookTargets(ability.pawn, target.ToTargetInfo(ability.pawn.Map)), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!IsApplicableTo(target, showMessages))
		{
			return false;
		}
		for (int i = 0; i < ability.EffectComps.Count; i++)
		{
			if (!ability.EffectComps[i].Valid(target, showMessages))
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanHitTarget(LocalTargetInfo targ)
	{
		if (EffectiveRange <= 0f)
		{
			return true;
		}
		return base.CanHitTarget(targ);
	}

	public override void OnGUI(LocalTargetInfo target)
	{
		if (CanHitTarget(target) && IsApplicableTo(target) && ValidateTarget(target, showMessages: false))
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
			string text = effectComp.ExtraLabelMouseAttachment(target);
			if (!text.NullOrEmpty())
			{
				Widgets.MouseAttachedLabel(text);
				break;
			}
		}
	}

	public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
	{
		bool num = base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
		if (num && ability.def.stunTargetWhileCasting && ability.def.verbProperties.warmupTime > 0f && castTarg.Thing is Pawn pawn && pawn != ability.pawn)
		{
			pawn.stances.stunner.StunFor(ability.def.verbProperties.warmupTime.SecondsToTicks(), ability.pawn, addBattleLog: false, showMote: false);
			if (!pawn.Awake())
			{
				RestUtility.WakeUp(pawn);
			}
		}
		return num;
	}

	public override void DrawHighlight(LocalTargetInfo target)
	{
		AbilityDef def = ability.def;
		if (EffectiveRange > 0f)
		{
			verbProps.DrawRadiusRing(caster.Position, this);
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
