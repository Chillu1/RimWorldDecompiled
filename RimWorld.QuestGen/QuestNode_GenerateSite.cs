using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_GenerateSite : QuestNode
{
	public SlateRef<IEnumerable<SitePartDefWithParams>> sitePartsParams;

	public SlateRef<Faction> faction;

	public SlateRef<PlanetTile> tile;

	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<RulePack> singleSitePartRules;

	public SlateRef<bool> hiddenSitePartsPossible;

	public SlateRef<WorldObjectDef> worldObjectDef;

	private const string RootSymbol = "root";

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests && sitePartsParams.GetValue(slate) != null)
		{
			foreach (SitePartDefWithParams item in sitePartsParams.GetValue(slate))
			{
				if (item.def.wantsThreatPoints)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Site var = QuestGen_Sites.GenerateSite(sitePartsParams.GetValue(slate), tile.GetValue(slate), faction.GetValue(slate), hiddenSitePartsPossible.GetValue(slate), singleSitePartRules.GetValue(slate), worldObjectDef.GetValue(slate));
		if (storeAs.GetValue(slate) != null)
		{
			QuestGen.slate.Set(storeAs.GetValue(slate), var);
		}
	}
}
