using Verse;

namespace RimWorld;

public class Alert_Analyzable_NeuralLump : Alert_Analyzable
{
	protected override ThingDef Def => ThingDefOf.FleshmassNeuralLump;

	public Alert_Analyzable_NeuralLump()
	{
		requireAnomaly = true;
		defaultLabel = "AlertNeuralLump".Translate();
		defaultExplanation = "AlertNeuralLumpDesc".Translate();
	}
}
