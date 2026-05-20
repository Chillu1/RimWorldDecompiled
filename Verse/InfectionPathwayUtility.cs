using System.Collections.Generic;
using RimWorld;

namespace Verse;

public static class InfectionPathwayUtility
{
	public static void AddInfectionPathway(InfectionPathwayDef def, Pawn to, Pawn source = null)
	{
		if (to.infectionVectors != null)
		{
			to.infectionVectors.AddInfectionVector(def, source);
		}
	}

	public static void AddInfectionPathways(List<InfectionPathwayDef> defs, Pawn to, Pawn source = null)
	{
		if (to.infectionVectors == null)
		{
			return;
		}
		foreach (InfectionPathwayDef def in defs)
		{
			to.infectionVectors.AddInfectionVector(def, source);
		}
	}
}
