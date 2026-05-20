namespace Verse;

public static class DietCategoryUtility
{
	public static string ToStringHuman(this DietCategory diet)
	{
		return diet switch
		{
			DietCategory.NeverEats => "DietCategory_NeverEats".Translate(), 
			DietCategory.Herbivorous => "DietCategory_Herbivorous".Translate(), 
			DietCategory.Dendrovorous => "DietCategory_Dendrovorous".Translate(), 
			DietCategory.Ovivorous => "DietCategory_Ovivorous".Translate(), 
			DietCategory.Omnivorous => "DietCategory_Omnivorous".Translate(), 
			DietCategory.Carnivorous => "DietCategory_Carnivorous".Translate(), 
			_ => "error", 
		};
	}

	public static string ToStringHumanShort(this DietCategory diet)
	{
		return diet switch
		{
			DietCategory.NeverEats => "DietCategory_NeverEats_Short".Translate(), 
			DietCategory.Herbivorous => "DietCategory_Herbivorous_Short".Translate(), 
			DietCategory.Dendrovorous => "DietCategory_Dendrovorous_Short".Translate(), 
			DietCategory.Ovivorous => "DietCategory_Ovivorous_Short".Translate(), 
			DietCategory.Omnivorous => "DietCategory_Omnivorous_Short".Translate(), 
			DietCategory.Carnivorous => "DietCategory_Carnivorous_Short".Translate(), 
			_ => "error", 
		};
	}
}
