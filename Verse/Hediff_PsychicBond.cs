using RimWorld;

namespace Verse;

public class Hediff_PsychicBond : HediffWithTarget
{
	private const int HediffCheckInterval = 65;

	public override string LabelBase => base.LabelBase + " (" + target?.LabelShortCap + ")";

	public override bool ShouldRemove
	{
		get
		{
			if (!base.ShouldRemove)
			{
				return pawn.Dead;
			}
			return true;
		}
	}

	public override void PostRemoved()
	{
		Gene_PsychicBonding gene_PsychicBonding = base.pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		if (gene_PsychicBonding != null)
		{
			gene_PsychicBonding.RemoveBond();
		}
		else if (target != null && target is Pawn pawn)
		{
			pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>()?.RemoveBond();
		}
	}

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		if (pawn.IsHashIntervalTick(65, delta))
		{
			Severity = (ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(pawn, this) ? 0.5f : 1.5f);
		}
	}
}
