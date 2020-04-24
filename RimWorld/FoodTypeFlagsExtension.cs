using Verse;

namespace RimWorld
{
	public static class FoodTypeFlagsExtension
	{
		public static string ToHumanString(this FoodTypeFlags ft)
		{
			string text = "";
			if ((ft & FoodTypeFlags.VegetableOrFruit) != 0)
			{
				text += "FoodTypeFlags_VegetableOrFruit".Translate();
			}
			if ((ft & FoodTypeFlags.Meat) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Meat".Translate();
			}
			if ((ft & FoodTypeFlags.Corpse) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Corpse".Translate();
			}
			if ((ft & FoodTypeFlags.Seed) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Seed".Translate();
			}
			if ((ft & FoodTypeFlags.AnimalProduct) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_AnimalProduct".Translate();
			}
			if ((ft & FoodTypeFlags.Plant) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Plant".Translate();
			}
			if ((ft & FoodTypeFlags.Tree) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Tree".Translate();
			}
			if ((ft & FoodTypeFlags.Meal) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Meal".Translate();
			}
			if ((ft & FoodTypeFlags.Processed) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Processed".Translate();
			}
			if ((ft & FoodTypeFlags.Liquor) != 0)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += "FoodTypeFlags_Liquor".Translate();
			}
			if ((ft & FoodTypeFlags.Kibble) != 0)
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
}
