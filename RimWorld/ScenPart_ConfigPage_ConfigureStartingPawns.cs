using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage_ConfigureStartingPawns : ScenPart_ConfigPage_ConfigureStartingPawnsBase
	{
		public int pawnCount = 3;

		public DevelopmentalStage allowedDevelopmentalStages = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;

		public List<SkillDef> requiredSkills;

		private string pawnCountBuffer;

		private string pawnCountChoiceBuffer;

		private const int MaxPawnChoiceCount = 10;

		protected override int TotalPawnCount => pawnCount;

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
		}

		public override string Summary(Scenario scen)
		{
			if (pawnCount == 1)
			{
				return "ScenPart_StartWithPawn".Translate();
			}
			return "ScenPart_StartWithPawns".Translate(pawnCount);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ pawnCount;
		}

		public override void Randomize()
		{
			pawnCount = Rand.RangeInclusive(1, 6);
			pawnChoiceCount = 10;
		}

		protected override void GenerateStartingPawns()
		{
			int num = 0;
			do
			{
				StartingPawnUtility.ClearAllStartingPawns();
				for (int i = 0; i < pawnCount; i++)
				{
					StartingPawnUtility.AddNewPawn();
				}
				num++;
			}
			while (num <= 20 && !StartingPawnUtility.WorkTypeRequirementsSatisfied());
		}

		public override void PostIdeoChosen()
		{
			Find.GameInitData.allowedDevelopmentalStages = allowedDevelopmentalStages;
			Find.GameInitData.startingSkillsRequired = requiredSkills;
			base.PostIdeoChosen();
		}
	}
}
