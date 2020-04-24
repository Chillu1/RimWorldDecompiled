using Verse;

namespace RimWorld
{
	public static class FoodPoisonCauseExtension
	{
		public static string ToStringHuman(this FoodPoisonCause cause)
		{
			switch (cause)
			{
			case FoodPoisonCause.Unknown:
				return "UnknownLower".Translate().CapitalizeFirst();
			case FoodPoisonCause.IncompetentCook:
				return "FoodPoisonCause_IncompetentCook".Translate();
			case FoodPoisonCause.FilthyKitchen:
				return "FoodPoisonCause_FilthyKitchen".Translate();
			case FoodPoisonCause.Rotten:
				return "FoodPoisonCause_Rotten".Translate();
			case FoodPoisonCause.DangerousFoodType:
				return "FoodPoisonCause_DangerousFoodType".Translate();
			default:
				return cause.ToString();
			}
		}
	}
}
