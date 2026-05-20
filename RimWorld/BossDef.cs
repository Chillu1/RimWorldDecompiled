using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class BossDef : Def
{
	public PawnKindDef kindDef;

	public int appearAfterTicks = int.MaxValue;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (DefDatabase<BossDef>.AllDefs.Any((BossDef b) => b != this && b.kindDef == kindDef))
		{
			yield return kindDef.label + " appears in two BossDefs, only one PawnKind per boss is currently supported.";
		}
	}
}
