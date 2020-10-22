using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetSiteTile : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<bool> preferCloserTiles;

		public SlateRef<bool> allowCaravans;

		public SlateRef<bool?> clampRangeBySiteParts;

		public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

		protected override bool TestRunInt(Slate slate)
		{
			if (!TryFindTile(slate, out var tile))
			{
				return false;
			}
			slate.Set(storeAs.GetValue(slate), tile);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (TryFindTile(QuestGen.slate, out var tile))
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), tile);
			}
		}

		private bool TryFindTile(Slate slate, out int tile)
		{
			int nearThisTile = (slate.Get<Map>("map") ?? Find.RandomPlayerHomeMap)?.Tile ?? (-1);
			int num = int.MaxValue;
			bool? value = clampRangeBySiteParts.GetValue(slate);
			if (value.HasValue && value.Value)
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
			return TileFinder.TryFindNewSiteTile(out tile, var.min, var.max, allowCaravans.GetValue(slate), preferCloserTiles.GetValue(slate), nearThisTile);
		}
	}
}
