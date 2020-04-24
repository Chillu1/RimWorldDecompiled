using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class SettlementNameGenerator
	{
		private static List<string> usedNames = new List<string>();

		public static string GenerateSettlementName(Settlement factionBase, RulePackDef rulePack = null)
		{
			if (rulePack == null)
			{
				if (factionBase.Faction == null || factionBase.Faction.def.settlementNameMaker == null)
				{
					return factionBase.def.label;
				}
				rulePack = factionBase.Faction.def.settlementNameMaker;
			}
			usedNames.Clear();
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Name != null)
				{
					usedNames.Add(settlement.Name);
				}
			}
			return NameGenerator.GenerateName(rulePack, usedNames, appendNumberIfNameUsed: true);
		}
	}
}
