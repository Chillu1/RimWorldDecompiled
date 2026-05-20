using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Counsel : CompAbilityEffect
{
	public static readonly SimpleCurve SuccessChanceBySocialSkill = new SimpleCurve
	{
		new CurvePoint(0f, 0.02f),
		new CurvePoint(5f, 0.4f),
		new CurvePoint(10f, 0.7f),
		new CurvePoint(20f, 0.9f)
	};

	public static readonly SimpleCurve SuccessChanceFactorByOpinion = new SimpleCurve
	{
		new CurvePoint(-100f, 0.7f),
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 1.3f)
	};

	public new CompProperties_AbilityCounsel Props => (CompProperties_AbilityCounsel)props;

	public override bool HideTargetPawnTooltip => true;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!ModLister.CheckIdeology("Ideoligion councel"))
		{
			return;
		}
		Pawn pawn = target.Pawn;
		if (Rand.Chance(ChanceForPawn(pawn)))
		{
			List<Thought> list = new List<Thought>();
			pawn.needs.mood.thoughts.GetAllMoodThoughts(list);
			Thought_Memory thought_Memory = (Thought_Memory)list.Where((Thought t) => t is Thought_Memory && t.MoodOffset() <= Props.minMoodOffset).MaxByWithFallback((Thought t) => 0f - t.MoodOffset());
			if (thought_Memory != null)
			{
				Thought_Counselled thought_Counselled = (Thought_Counselled)ThoughtMaker.MakeThought(ThoughtDefOf.Counselled);
				thought_Counselled.durationTicksOverride = thought_Memory.DurationTicks - thought_Memory.age;
				thought_Counselled.moodOffsetOverride = 0f - thought_Memory.MoodOffset();
				pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Counselled, parent.pawn);
				Messages.Message(Props.successMessage.Formatted(parent.pawn.Named("INITIATOR"), pawn.Named("RECIPIENT"), thought_Memory.MoodOffset()), new LookTargets(new Pawn[2] { parent.pawn, pawn }), MessageTypeDefOf.PositiveEvent, historical: false);
			}
			else
			{
				Thought_Memory thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.Counselled_MoodBoost);
				pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory2, parent.pawn);
				Messages.Message(Props.successMessageNoNegativeThought.Formatted(parent.pawn.Named("INITIATOR"), pawn.Named("RECIPIENT"), thought_Memory2.def.stages[0].baseMoodEffect.Named("MOODBONUS")), new LookTargets(new Pawn[2] { parent.pawn, pawn }), MessageTypeDefOf.PositiveEvent, historical: false);
			}
			PlayLogEntry_Interaction entry = new PlayLogEntry_Interaction(InteractionDefOf.Counsel_Success, parent.pawn, pawn, null);
			Find.PlayLog.Add(entry);
		}
		else
		{
			pawn.needs.mood.thoughts.memories.TryGainMemory(Props.failedThoughtRecipient, parent.pawn);
			PlayLogEntry_Interaction entry2 = new PlayLogEntry_Interaction(InteractionDefOf.Counsel_Failure, parent.pawn, pawn, null);
			Find.PlayLog.Add(entry2);
			Messages.Message(Props.failMessage.Formatted(parent.pawn.Named("INITIATOR"), pawn.Named("RECIPIENT")), new LookTargets(new Pawn[2] { parent.pawn, pawn }), MessageTypeDefOf.NegativeEvent, historical: false);
		}
		if (Props.sound != null)
		{
			Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
		}
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		if (!AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, parent))
		{
			return false;
		}
		if (parent.pawn.Ideo != pawn.Ideo)
		{
			if (throwMessages)
			{
				Precept_Role role = parent.pawn.Ideo.GetRole(parent.pawn);
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityMustBeSameIdeoCounsel".Translate(role.LabelForPawn(parent.pawn), parent.pawn.Ideo.memberName), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
		{
			return false;
		}
		List<Thought> list = new List<Thought>();
		pawn.needs.mood.thoughts.GetAllMoodThoughts(list);
		if (list.Any((Thought t) => t.def == ThoughtDefOf.Counselled))
		{
			if (throwMessages)
			{
				Messages.Message("AbilityCantApplyAlreadyCounselled".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	public float ChanceForPawn(Pawn pawn)
	{
		return SuccessChanceBySocialSkill.Evaluate(parent.pawn.skills.GetSkill(SkillDefOf.Social).Level) * SuccessChanceFactorByOpinion.Evaluate(pawn.relations.OpinionOf(parent.pawn));
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return null;
		}
		if (!Valid(target))
		{
			return null;
		}
		Pawn pawn2 = parent.pawn;
		Pawn pawn3 = pawn;
		string text = ChanceForPawn(pawn).ToStringPercent();
		string text2 = SuccessChanceBySocialSkill.Evaluate(pawn2.skills.GetSkill(SkillDefOf.Social).Level).ToStringPercent();
		string text3 = SuccessChanceFactorByOpinion.Evaluate(pawn3.relations.OpinionOf(pawn2)).ToStringPercent();
		return "chance".Translate().CapitalizeFirst() + ": " + text + "\n\n" + "Factors".Translate() + ":\n" + " -  " + "AbilityIdeoConvertBreakdownSocialSkill".Translate(pawn2.Named("PAWN")) + " " + text2 + "\n" + " -  " + "AbilityIdeoConvertBreakdownOpinion".Translate(pawn2.Named("INITIATOR"), pawn3.Named("RECIPIENT")) + " " + text3;
	}
}
