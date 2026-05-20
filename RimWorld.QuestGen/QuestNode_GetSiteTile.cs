using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetSiteTile : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<bool> preferCloserTiles;

	public SlateRef<bool> allowCaravans;

	public SlateRef<bool> canSelectSpace;

	public SlateRef<bool?> clampRangeBySiteParts;

	public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

	public SlateRef<List<LandmarkDef>> allowedLandmarks;

	public SlateRef<float?> selectLandmarkChance;

	public SlateRef<bool> canSelectComboLandmarks;

	protected override bool TestRunInt(Slate slate)
	{
		if (!TryFindTile(slate, out var tile))
		{
			return false;
		}
		if (clampRangeBySiteParts.GetValue(slate) == true && sitePartDefs.GetValue(slate) == null)
		{
			return false;
		}
		slate.Set(storeAs.GetValue(slate), tile);
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!slate.TryGet<int>(storeAs.GetValue(slate), out var _) && TryFindTile(QuestGen.slate, out var tile))
		{
			QuestGen.slate.Set(storeAs.GetValue(slate), tile);
		}
	}

	private bool TryFindTile(Slate slate, out PlanetTile tile)
	{
		bool value = canSelectSpace.GetValue(slate);
		PlanetTile nearTile = (slate.Get<Map>("map") ?? (value ? Find.RandomPlayerHomeMap : Find.RandomSurfacePlayerHomeMap))?.Tile ?? PlanetTile.Invalid;
		if (nearTile.Valid && nearTile.LayerDef.isSpace && !value)
		{
			nearTile = PlanetTile.Invalid;
		}
		int num = int.MaxValue;
		bool? value2 = clampRangeBySiteParts.GetValue(slate);
		if (value2.HasValue && value2.Value)
		{
			foreach (SitePartDef item in sitePartDefs.GetValue(slate))
			{
				if (item.conditionCauserDef != null)
				{
					num = Mathf.Min(num, item.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange);
				}
			}
		}
		if (!slate.TryGet<IntRange>("siteDistRange", out var var))
		{
			var = new IntRange(7, Mathf.Min(27, num));
		}
		else if (num != int.MaxValue)
		{
			var = new IntRange(Mathf.Min(var.min, num), Mathf.Min(var.max, num));
		}
		TileFinderMode tileFinderMode = (preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random);
		float num2 = ((!ModsConfig.OdysseyActive) ? 0f : (selectLandmarkChance.GetValue(slate) ?? 0.5f));
		return TileFinder.TryFindNewSiteTile(out tile, nearTile, var.min, var.max, allowCaravans.GetValue(slate), allowedLandmarks.GetValue(slate), num2, canSelectComboLandmarks.GetValue(slate), tileFinderMode, exitOnFirstTileFound: false, value);
	}
}
