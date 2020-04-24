using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage_ConfigureStartingPawns : ScenPart_ConfigPage
	{
		public int pawnCount = 3;

		public int pawnChoiceCount = 10;

		private string pawnCountBuffer;

		private string pawnCountChoiceBuffer;

		private const int MaxPawnCount = 10;

		private const int MaxPawnChoiceCount = 10;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			base.DoEditInterface(listing);
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			scenPartRect.height = ScenPart.RowHeight;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = new Rect(scenPartRect.x - 200f, scenPartRect.y + ScenPart.RowHeight, 200f, ScenPart.RowHeight);
			rect.xMax -= 4f;
			Widgets.Label(rect, "ScenPart_StartWithPawns_OutOf".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldNumeric(scenPartRect, ref pawnCount, ref pawnCountBuffer, 1f, 10f);
			scenPartRect.y += ScenPart.RowHeight;
			Widgets.TextFieldNumeric(scenPartRect, ref pawnChoiceCount, ref pawnCountChoiceBuffer, pawnCount, 10f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref pawnCount, "pawnCount", 0);
			Scribe_Values.Look(ref pawnChoiceCount, "pawnChoiceCount", 0);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StartWithPawns".Translate(pawnCount, pawnChoiceCount);
		}

		public override void Randomize()
		{
			pawnCount = Rand.RangeInclusive(1, 6);
			pawnChoiceCount = 10;
		}

		public override void PostWorldGenerate()
		{
			Find.GameInitData.startingPawnCount = pawnCount;
			int num = 0;
			do
			{
				StartingPawnUtility.ClearAllStartingPawns();
				for (int i = 0; i < pawnCount; i++)
				{
					Find.GameInitData.startingAndOptionalPawns.Add(StartingPawnUtility.NewGeneratedStartingPawn());
				}
				num++;
			}
			while (num <= 20 && !StartingPawnUtility.WorkTypeRequirementsSatisfied());
			while (Find.GameInitData.startingAndOptionalPawns.Count < pawnChoiceCount)
			{
				Find.GameInitData.startingAndOptionalPawns.Add(StartingPawnUtility.NewGeneratedStartingPawn());
			}
		}
	}
}
