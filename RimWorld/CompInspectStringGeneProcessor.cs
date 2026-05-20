using System.Text;

namespace RimWorld;

public class CompInspectStringGeneProcessor : CompInspectString
{
	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.Props.inspectString);
		stringBuilder.Append(": +");
		stringBuilder.Append(parent.GetStatValue(StatDefOf.GeneticComplexityIncrease));
		return stringBuilder.ToString();
	}
}
