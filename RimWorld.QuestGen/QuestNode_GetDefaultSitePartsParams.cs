using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetDefaultSitePartsParams : QuestNode
{
	public SlateRef<PlanetTile> tile;

	public SlateRef<Faction> faction;

	public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

	[NoTranslate]
	public SlateRef<string> storeSitePartsParamsAs;

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		return true;
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		SiteMakerHelper.GenerateDefaultParams(slate.Get("points", 0f), tile.GetValue(slate), faction.GetValue(slate), sitePartDefs.GetValue(slate), out var sitePartDefsWithParams);
		if (sitePartDefsWithParams != null)
		{
			for (int i = 0; i < sitePartDefsWithParams.Count; i++)
			{
				if (sitePartDefsWithParams[i].def == SitePartDefOf.PreciousLump)
				{
					sitePartDefsWithParams[i].parms.preciousLumpResources = slate.Get<ThingDef>("targetMineable");
				}
			}
		}
		slate.Set(storeSitePartsParamsAs.GetValue(slate), sitePartDefsWithParams);
	}
}
