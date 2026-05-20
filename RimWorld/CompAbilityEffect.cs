using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class CompAbilityEffect : AbilityComp
{
	public CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

	public virtual bool HideTargetPawnTooltip => false;

	public virtual bool ShouldHideGizmo => false;

	protected bool SendLetter
	{
		get
		{
			if (!Props.sendLetter)
			{
				return false;
			}
			if (!Props.customLetterText.NullOrEmpty())
			{
				return !Props.customLetterLabel.NullOrEmpty();
			}
			return false;
		}
	}

	public virtual void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (Props.screenShakeIntensity > float.Epsilon)
		{
			Find.CameraDriver.shaker.DoShake(Props.screenShakeIntensity);
		}
		Pawn pawn = parent.pawn;
		Pawn pawn2 = target.Pawn;
		if (pawn2 != null && !pawn2.IsSlaveOfColony)
		{
			Faction homeFaction = pawn2.HomeFaction;
			if (Props.goodwillImpact != 0 && pawn.Faction == Faction.OfPlayer && homeFaction != null && !homeFaction.HostileTo(pawn.Faction) && (Props.applyGoodwillImpactToLodgers || !pawn2.IsQuestLodger()) && !pawn2.IsQuestHelper())
			{
				Faction.OfPlayer.TryAffectGoodwillWith(homeFaction, Props.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
			}
		}
		if (Props.clamorType != null)
		{
			GenClamor.DoClamor(parent.pawn, target.Cell, Props.clamorRadius, Props.clamorType);
		}
		((SoundDef)(pawn.gender switch
		{
			Gender.Male => Props.soundMale ?? Props.sound, 
			Gender.Female => Props.soundFemale ?? Props.sound, 
			_ => Props.sound, 
		}))?.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
		if (!Props.message.NullOrEmpty())
		{
			Messages.Message(Props.message, parent.pawn, Props.messageType ?? MessageTypeDefOf.SilentInput);
		}
	}

	public virtual void Apply(GlobalTargetInfo target)
	{
	}

	public virtual bool AICanTargetNow(LocalTargetInfo target)
	{
		return true;
	}

	public virtual bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!Props.availableWhenTargetIsWounded && (target.Pawn.health.hediffSet.BleedRateTotal > 0f || target.Pawn.health.HasHediffsNeedingTend()))
		{
			return false;
		}
		return true;
	}

	public virtual bool CanApplyOn(GlobalTargetInfo target)
	{
		return true;
	}

	public virtual void PostApplied(List<LocalTargetInfo> targets, Map map)
	{
	}

	public virtual IEnumerable<PreCastAction> GetPreCastActions()
	{
		return Enumerable.Empty<PreCastAction>();
	}

	public virtual IEnumerable<Mote> CustomWarmupMotes(LocalTargetInfo target)
	{
		return Enumerable.Empty<Mote>();
	}

	public virtual void DrawEffectPreview(LocalTargetInfo target)
	{
	}

	public virtual bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!Props.canTargetBaby && !AbilityUtility.ValidateMustNotBeBaby(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!Props.canTargetBosses && pawn.kindDef.isBoss)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool Valid(GlobalTargetInfo target, bool throwMessages = false)
	{
		return true;
	}

	public virtual string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		return null;
	}

	public virtual string WorldMapExtraLabel(GlobalTargetInfo target)
	{
		return null;
	}

	public virtual Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
	{
		return null;
	}

	public virtual Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
	{
		return null;
	}

	public virtual string ExtraTooltipPart()
	{
		return null;
	}

	public virtual void OnGizmoUpdate()
	{
	}
}
