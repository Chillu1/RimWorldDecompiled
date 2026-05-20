using Verse;

namespace RimWorld;

public class Thought_Situational_GeneticChemicalDependency : Thought_Situational
{
	private static readonly SimpleCurve MoodOffsetCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, -4f),
		new CurvePoint(6f, -20f)
	};

	public override float MoodOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		foreach (Gene item in pawn.genes.GenesListForReading)
		{
			if (item is Gene_ChemicalDependency gene_ChemicalDependency && gene_ChemicalDependency.def.chemical == def.chemicalDef && gene_ChemicalDependency.LinkedHediff != null)
			{
				return MoodOffsetCurve.Evaluate(gene_ChemicalDependency.LinkedHediff.Severity);
			}
		}
		return 0f;
	}
}
