using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetSiteTile : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<bool> preferCloserTiles;

		public SlateRef<bool> allowCaravans;

		protected override bool TestRunInt(Slate slate)
		{
			if (!TryFindTile(slate, out int tile))
			{
				return false;
			}
			slate.Set(storeAs.GetValue(slate), tile);
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (TryFindTile(QuestGen.slate, out int tile))
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), tile);
			}
		}

		private bool TryFindTile(Slate slate, out int tile)
		{
			int nearThisTile = (slate.Get<Map>("map") ?? Find.RandomPlayerHomeMap)?.Tile ?? (-1);
			if (!slate.TryGet("siteDistRange", out IntRange var))
			{
				return TileFinder.TryFindNewSiteTile(out tile, 7, 27, preferCloserTiles: preferCloserTiles.GetValue(slate), allowCaravans: allowCaravans.GetValue(slate), nearThisTile: nearThisTile);
			}
			return TileFinder.TryFindNewSiteTile(out tile, var.min, var.max, allowCaravans.GetValue(slate), preferCloserTiles.GetValue(slate), nearThisTile);
		}
	}
}
