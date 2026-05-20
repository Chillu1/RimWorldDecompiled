using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PreceptComp_MentalBreak : PreceptComp
{
	public MentalBreakDef mentalBreakDef;

	public override IEnumerable<string> GetDescriptions()
	{
		yield return mentalBreakDef.LabelCap;
	}
}
