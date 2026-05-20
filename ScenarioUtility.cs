using System.Collections.Generic;
using RimWorld;
using Verse;

public static class ScenarioUtility
{
	public static bool AllowsChildSelection(Scenario scenario)
	{
		List<ScenPart> parts = scenario.parts;
		for (int i = 0; i < parts.Count; i++)
		{
			if (!(parts[i] is ScenPart_ConfigPage_ConfigureStartingPawns_KindDefs scenPart_ConfigPage_ConfigureStartingPawns_KindDefs))
			{
				continue;
			}
			foreach (PawnKindCount kindCount in scenPart_ConfigPage_ConfigureStartingPawns_KindDefs.kindCounts)
			{
				if (!CanBeChild(kindCount.kindDef))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool CanBeChild(PawnKindDef kindDef)
	{
		if (!kindDef.apparelRequired.NullOrEmpty())
		{
			for (int i = 0; i < kindDef.apparelRequired.Count; i++)
			{
				if (kindDef.apparelRequired[i].IsApparel && !kindDef.apparelRequired[i].apparel.developmentalStageFilter.Juvenile())
				{
					return false;
				}
			}
		}
		return true;
	}
}
