using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_TradeRequest_RandomOfferDuration : QuestNode
	{
		private const float MinNonTravelTimeFractionOfTravelTime = 0.35f;

		private const float MinNonTravelTimeDays = 6f;

		public SlateRef<Settlement> settlement;

		[NoTranslate]
		public SlateRef<string> storeAs;

		[NoTranslate]
		public SlateRef<string> storeEstimatedTravelTimeAs;

		public static int RandomOfferDurationTicks(int tileIdFrom, int tileIdTo, out int travelTicks)
		{
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			travelTicks = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
			float num = (float)travelTicks / 60000f;
			int num2 = Mathf.CeilToInt(Mathf.Max(num + 6f, num * 1.35f));
			if (num2 > SiteTuning.QuestSiteTimeoutDaysRange.max)
			{
				return -1;
			}
			int num3 = Mathf.Max(randomInRange, num2);
			return 60000 * num3;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Map map = slate.Get<Map>("map");
			slate.Set(storeAs.GetValue(slate), RandomOfferDurationTicks(map.Tile, settlement.GetValue(slate).Tile, out int travelTicks));
			slate.Set(storeEstimatedTravelTimeAs.GetValue(slate), travelTicks);
		}

		protected override bool TestRunInt(Slate slate)
		{
			Map map = slate.Get<Map>("map");
			slate.Set(storeAs.GetValue(slate), RandomOfferDurationTicks(map.Tile, settlement.GetValue(slate).Tile, out int travelTicks));
			slate.Set(storeEstimatedTravelTimeAs.GetValue(slate), travelTicks);
			return true;
		}
	}
}
