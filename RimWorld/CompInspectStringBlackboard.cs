using System.Text;
using Verse;

namespace RimWorld;

public class CompInspectStringBlackboard : CompInspectString
{
	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.Props.inspectString);
		stringBuilder.Append(": +");
		stringBuilder.Append(0.2f.ToStringPercent());
		return stringBuilder.ToString();
	}
}
