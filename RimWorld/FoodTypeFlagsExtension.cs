using Verse;

namespace RimWorld;

public static class FoodTypeFlagsExtension
{
	public static string ToHumanString(this FoodTypeFlags ft)
	{
		string text = "";
		if ((ft & FoodTypeFlags.VegetableOrFruit) != FoodTypeFlags.None)
		{
			text += "FoodTypeFlags_VegetableOrFruit".Translate();
		}
		if ((ft & FoodTypeFlags.Meat) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Meat".Translate();
		}
		if ((ft & FoodTypeFlags.Corpse) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Corpse".Translate();
		}
		if ((ft & FoodTypeFlags.Seed) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Seed".Translate();
		}
		if ((ft & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_AnimalProduct".Translate();
		}
		if ((ft & FoodTypeFlags.Plant) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Plant".Translate();
		}
		if ((ft & FoodTypeFlags.Tree) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Tree".Translate();
		}
		if ((ft & FoodTypeFlags.Meal) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Meal".Translate();
		}
		if ((ft & FoodTypeFlags.Processed) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Processed".Translate();
		}
		if ((ft & FoodTypeFlags.Liquor) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Liquor".Translate();
		}
		if ((ft & FoodTypeFlags.Kibble) != FoodTypeFlags.None)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "FoodTypeFlags_Kibble".Translate();
		}
		return text;
	}
}
