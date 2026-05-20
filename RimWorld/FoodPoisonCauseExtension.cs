using Verse;

namespace RimWorld;

public static class FoodPoisonCauseExtension
{
	public static string ToStringHuman(this FoodPoisonCause cause)
	{
		return cause switch
		{
			FoodPoisonCause.Unknown => "UnknownLower".Translate().CapitalizeFirst(), 
			FoodPoisonCause.IncompetentCook => "FoodPoisonCause_IncompetentCook".Translate(), 
			FoodPoisonCause.FilthyKitchen => "FoodPoisonCause_FilthyKitchen".Translate(), 
			FoodPoisonCause.Rotten => "FoodPoisonCause_Rotten".Translate(), 
			FoodPoisonCause.DangerousFoodType => "FoodPoisonCause_DangerousFoodType".Translate(), 
			_ => cause.ToString(), 
		};
	}
}
