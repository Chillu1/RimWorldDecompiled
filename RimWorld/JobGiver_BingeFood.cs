using Verse;

namespace RimWorld
{
	public class JobGiver_BingeFood : JobGiver_Binge
	{
		private const int BaseIngestInterval = 1100;

		protected override int IngestInterval(Pawn pawn)
		{
			return 1100;
		}

		protected override Thing BestIngestTarget(Pawn pawn)
		{
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate: true, out var foodSource, out var _, canRefillDispenser: false, canUseInventory: true, allowForbidden: true, allowCorpse: true, allowSociallyImproper: true))
			{
				return foodSource;
			}
			return null;
		}
	}
}
