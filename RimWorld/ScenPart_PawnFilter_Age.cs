using System;
using Verse;

namespace RimWorld
{
	public class ScenPart_PawnFilter_Age : ScenPart
	{
		public IntRange allowedAgeRange = new IntRange(0, 999999);

		private const int RangeMin = 15;

		private const int RangeMax = 120;

		private const int RangeMinMax = 19;

		private const int RangeMinWidth = 4;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Widgets.IntRange(listing.GetScenPartRect(this, 31f), (int)listing.CurHeight, ref allowedAgeRange, 15, 120, null, 4);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref allowedAgeRange, "allowedAgeRange");
		}

		public override string Summary(Scenario scen)
		{
			if (allowedAgeRange.min > 15)
			{
				if (allowedAgeRange.max < 10000)
				{
					return "ScenPart_StartingPawnAgeRange".Translate(allowedAgeRange.min, allowedAgeRange.max);
				}
				return "ScenPart_StartingPawnAgeMin".Translate(allowedAgeRange.min);
			}
			if (allowedAgeRange.max < 10000)
			{
				return "ScenPart_StartingPawnAgeMax".Translate(allowedAgeRange.max);
			}
			throw new Exception();
		}

		public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			return allowedAgeRange.Includes(pawn.ageTracker.AgeBiologicalYears);
		}

		public override void Randomize()
		{
			allowedAgeRange = new IntRange(15, 120);
			switch (Rand.RangeInclusive(0, 2))
			{
			case 0:
				allowedAgeRange.min = Rand.Range(20, 60);
				break;
			case 1:
				allowedAgeRange.max = Rand.Range(20, 60);
				break;
			case 2:
				allowedAgeRange.min = Rand.Range(20, 60);
				allowedAgeRange.max = Rand.Range(20, 60);
				break;
			}
			MakeAllowedAgeRangeValid();
		}

		private void MakeAllowedAgeRangeValid()
		{
			if (allowedAgeRange.max < 19)
			{
				allowedAgeRange.max = 19;
			}
			if (allowedAgeRange.max - allowedAgeRange.min < 4)
			{
				allowedAgeRange.min = allowedAgeRange.max - 4;
			}
		}
	}
}
