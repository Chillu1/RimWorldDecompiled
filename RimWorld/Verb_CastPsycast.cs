using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Verb_CastPsycast : Verb_CastAbility
	{
		private const float StatLabelOffsetY = 1f;

		public Psycast Psycast => ability as Psycast;

		public override bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Psycasts are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 324345647);
				return false;
			}
			if (!base.IsApplicableTo(target, showMessages))
			{
				return false;
			}
			if (!Psycast.def.HasAreaOfEffect && !Psycast.CanApplyPsycastTo(target))
			{
				if (showMessages)
				{
					Messages.Message(ability.def.LabelCap + ": " + "AbilityTargetPsychicallyDeaf".Translate(), target.ToTargetInfo(ability.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			if (IsApplicableTo(target))
			{
				base.OrderForceTarget(target);
			}
		}

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			if (!base.ValidateTarget(target))
			{
				return false;
			}
			if (CasterPawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
			{
				Messages.Message("CommandPsycastZeroPsychicSensitivity".Translate(), caster, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (Psycast.def.EntropyGain > float.Epsilon && CasterPawn.psychicEntropy.WouldOverflowEntropy(Psycast.def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(CasterPawn)))
			{
				Messages.Message("CommandPsycastWouldExceedEntropy".Translate(), caster, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			float num = Psycast.FinalPsyfocusCost(target);
			float num2 = PsycastUtility.TotalPsyfocusCostOfQueuedPsycasts(CasterPawn);
			float num3 = num + num2;
			if (num > float.Epsilon && num3 > CasterPawn.psychicEntropy.CurrentPsyfocus + 0.0005f)
			{
				Messages.Message("CommandPsycastNotEnoughPsyfocus".Translate(num3.ToStringPercent("0.#"), (CasterPawn.psychicEntropy.CurrentPsyfocus - num2).ToStringPercent("0.#"), Psycast.def.label.Named("PSYCASTNAME"), caster.Named("CASTERNAME")), caster, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			return true;
		}

		public override void OnGUI(LocalTargetInfo target)
		{
			bool flag = ability.EffectComps.Any((CompAbilityEffect e) => e.Props.psychic);
			Texture2D texture2D = UIIcon;
			if (!Psycast.CanApplyPsycastTo(target))
			{
				texture2D = TexCommand.CannotShoot;
				DrawIneffectiveWarning(target);
			}
			if (target.IsValid && CanHitTarget(target) && IsApplicableTo(target))
			{
				if (flag)
				{
					foreach (LocalTargetInfo affectedTarget in ability.GetAffectedTargets(target))
					{
						if (Psycast.CanApplyPsycastTo(affectedTarget))
						{
							DrawSensitivityStat(affectedTarget);
						}
						else
						{
							DrawIneffectiveWarning(affectedTarget);
						}
					}
				}
				if (ability.EffectComps.Any((CompAbilityEffect e) => !e.Valid(target)))
				{
					texture2D = TexCommand.CannotShoot;
				}
			}
			else
			{
				texture2D = TexCommand.CannotShoot;
			}
			if (ThingRequiringRoyalPermissionUtility.IsViolatingRulesOfAnyFaction_NewTemp(HediffDefOf.PsychicAmplifier, CasterPawn, Psycast.def.level, ignoreSilencer: true) && Psycast.def.DetectionChance > 0f)
			{
				TaggedString taggedString = ((string)"Illegal".Translate()).ToUpper() + "\n" + Psycast.def.DetectionChance.ToStringPercent() + " " + "DetectionChance".Translate();
				Text.Font = GameFont.Small;
				GenUI.DrawMouseAttachment(texture2D, taggedString, 0f, default(Vector2), null, drawTextBackground: true, new Color(0.25f, 0f, 0f));
			}
			else
			{
				GenUI.DrawMouseAttachment(texture2D);
			}
			DrawAttachmentExtraLabel(target);
		}

		private void DrawIneffectiveWarning(LocalTargetInfo target)
		{
			if (target.Pawn != null)
			{
				Vector3 drawPos = target.Pawn.DrawPos;
				drawPos.z += 1f;
				GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), "Ineffective".Translate(), Color.red);
			}
		}

		private void DrawSensitivityStat(LocalTargetInfo target)
		{
			if (target.Pawn != null)
			{
				Pawn pawn = target.Pawn;
				float statValue = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
				Vector3 drawPos = pawn.DrawPos;
				drawPos.z += 1f;
				GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), (string)(StatDefOf.PsychicSensitivity.LabelCap + ": ") + statValue, (statValue > float.Epsilon) ? Color.white : Color.red);
			}
		}
	}
}
