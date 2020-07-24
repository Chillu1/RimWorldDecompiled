using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class CompAbilityEffect : AbilityComp
	{
		public CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

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
			if (pawn2 != null)
			{
				Faction factionOrExtraHomeFaction = pawn2.FactionOrExtraHomeFaction;
				if (Props.goodwillImpact != 0 && pawn.Faction != null && factionOrExtraHomeFaction != null && !factionOrExtraHomeFaction.HostileTo(pawn.Faction) && (Props.applyGoodwillImpactToLodgers || !pawn2.IsQuestLodger()) && !pawn2.IsQuestHelper())
				{
					factionOrExtraHomeFaction.TryAffectGoodwillWith(pawn.Faction, Props.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, "GoodwillChangedReason_UsedAbility".Translate(parent.def.LabelCap, pawn2.LabelShort), pawn2);
				}
			}
			ThingDef moteDef = (!Props.psychic) ? ThingDefOf.Mote_PsycastSkipEffect : ThingDefOf.Mote_PsycastPsychicEffect;
			if (target.HasThing)
			{
				MoteMaker.MakeAttachedOverlay(target.Thing, moteDef, Vector3.zero);
			}
			else
			{
				MoteMaker.MakeStaticMote(target.Cell, parent.pawn.Map, moteDef);
			}
			if (Props.clamorType != null)
			{
				GenClamor.DoClamor(parent.pawn, target.Cell, Props.clamorRadius, Props.clamorType);
			}
			if (Props.sound != null)
			{
				Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
			}
			if (!Props.message.NullOrEmpty())
			{
				Messages.Message(Props.message, parent.pawn, Props.messageType ?? MessageTypeDefOf.SilentInput);
			}
		}

		public virtual bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (!Props.availableWhenTargetIsWounded && (target.Pawn.health.hediffSet.BleedRateTotal > 0f || target.Pawn.health.HasHediffsNeedingTend()))
			{
				return false;
			}
			return true;
		}

		public virtual void DrawEffectPreview(LocalTargetInfo target)
		{
		}

		public virtual bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			return true;
		}
	}
}
