using Verse;

namespace RimWorld;

public class ThoughtWorker_GeneticChemicalDependency : ThoughtWorker
{
	public override string PostProcessLabel(Pawn p, string label)
	{
		return label.Formatted(def.chemicalDef.Named("CHEMICAL"));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return base.PostProcessDescription(p, description.Formatted(def.chemicalDef.Named("CHEMICAL")));
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive || p.genes == null)
		{
			return ThoughtState.Inactive;
		}
		foreach (Gene item in p.genes.GenesListForReading)
		{
			if (item is Gene_ChemicalDependency gene_ChemicalDependency && gene_ChemicalDependency.def.chemical == def.chemicalDef && gene_ChemicalDependency.LinkedHediff != null)
			{
				return gene_ChemicalDependency.LinkedHediff.Severity >= 1f;
			}
		}
		return ThoughtState.Inactive;
	}
}
