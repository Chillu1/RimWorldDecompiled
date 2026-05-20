using RimWorld;

namespace Verse;

public class HediffComp_SeverityFromHemogen : HediffComp
{
	private Gene_Hemogen cachedHemogenGene;

	public HediffCompProperties_SeverityFromHemogen Props => (HediffCompProperties_SeverityFromHemogen)props;

	public override bool CompShouldRemove => base.Pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null;

	private Gene_Hemogen Hemogen => cachedHemogenGene ?? (cachedHemogenGene = base.Pawn.genes.GetFirstGeneOfType<Gene_Hemogen>());

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (Hemogen != null)
		{
			severityAdjustment += ((Hemogen.Value > 0f) ? Props.severityPerHourHemogen : Props.severityPerHourEmpty) / 2500f;
		}
	}
}
