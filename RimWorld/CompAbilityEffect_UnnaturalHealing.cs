using System.Text;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_UnnaturalHealing : CompAbilityEffect
{
	private const float TentacleChance = 0.25f;

	public new CompProperties_UnnaturalHealing Props => (CompProperties_UnnaturalHealing)props;

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		Hediff hediff;
		BodyPartRecord part;
		bool flag = ((!MetalhorrorUtility.IsInfected(parent.pawn)) ? HealthUtility.TryGetWorstHealthCondition(pawn, out hediff, out part) : HealthUtility.TryGetWorstHealthCondition(pawn, out hediff, out part, HediffDefOf.MetalhorrorImplant));
		if (!flag && throwMessages)
		{
			Messages.Message(string.Format("{0}: {1}", "CannotUseAbility".Translate(parent.def.label), "AbilityCannotCastNoHealableInjury".Translate(pawn.Named("PAWN")).Resolve().StripTags()), pawn, MessageTypeDefOf.RejectInput, historical: false);
		}
		return flag;
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return;
		}
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.Bleeding)
			{
				hediff.Tended(1f, 1f, 1);
			}
		}
		bool flag = false;
		string text;
		if (MetalhorrorUtility.IsInfected(parent.pawn))
		{
			text = HealthUtility.FixWorstHealthCondition(pawn, HediffDefOf.MetalhorrorImplant);
			MetalhorrorUtility.Infect(pawn, parent.pawn, "UnnaturalHealingImplant");
		}
		else
		{
			text = HealthUtility.FixWorstHealthCondition(pawn);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("LetterUnnaturalHealing".Translate(parent.pawn.Named("CASTER"), pawn.Named("PAWN")));
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.AppendLine("\n" + text);
		}
		if (pawn.ageTracker.Adult && Rand.Chance(0.25f) && FleshbeastUtility.TryGiveMutation(pawn, HediffDefOf.Tentacle))
		{
			stringBuilder.Append("\n" + "LetterUnnaturalHealingTentacle".Translate(pawn.Named("PAWN")));
			TaleRecorder.RecordTale(TaleDefOf.MutatedMyArm, parent.pawn, pawn);
			flag = true;
		}
		else
		{
			TaleRecorder.RecordTale(TaleDefOf.HealedMe, parent.pawn, pawn);
		}
		TaggedString label = "LetterUnnaturalHealingLabel".Translate();
		LetterDef textLetterDef = (flag ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
		Find.LetterStack.ReceiveLetter(label, stringBuilder.ToString().TrimEndNewlines(), textLetterDef, pawn);
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return false;
	}
}
