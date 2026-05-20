using Verse;

namespace RimWorld;

public class CompAbilityEffect_HemogenCost : CompAbilityEffect
{
	public new CompProperties_AbilityHemogenCost Props => (CompProperties_AbilityHemogenCost)props;

	private bool HasEnoughHemogen
	{
		get
		{
			Gene_Hemogen gene_Hemogen = parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
			if (gene_Hemogen == null || gene_Hemogen.Value < Props.hemogenCost)
			{
				return false;
			}
			return true;
		}
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		GeneUtility.OffsetHemogen(parent.pawn, 0f - Props.hemogenCost);
	}

	public override bool GizmoDisabled(out string reason)
	{
		Gene_Hemogen gene_Hemogen = parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
		if (gene_Hemogen == null)
		{
			reason = "AbilityDisabledNoHemogenGene".Translate(parent.pawn);
			return true;
		}
		if (gene_Hemogen.Value < Props.hemogenCost)
		{
			reason = "AbilityDisabledNoHemogen".Translate(parent.pawn);
			return true;
		}
		float num = TotalHemogenCostOfQueuedAbilities();
		float num2 = Props.hemogenCost + num;
		if (Props.hemogenCost > float.Epsilon && num2 > gene_Hemogen.Value)
		{
			reason = "AbilityDisabledNoHemogen".Translate(parent.pawn);
			return true;
		}
		reason = null;
		return false;
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return HasEnoughHemogen;
	}

	private float TotalHemogenCostOfQueuedAbilities()
	{
		float num = ((!(parent.pawn.jobs?.curJob?.verbToUse is Verb_CastAbility verb_CastAbility)) ? 0f : (verb_CastAbility.ability?.HemogenCost() ?? 0f));
		if (parent.pawn.jobs != null)
		{
			for (int i = 0; i < parent.pawn.jobs.jobQueue.Count; i++)
			{
				if (parent.pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastAbility verb_CastAbility2)
				{
					num += verb_CastAbility2.ability?.HemogenCost() ?? 0f;
				}
			}
		}
		return num;
	}
}
