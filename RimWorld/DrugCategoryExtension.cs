using Verse;

namespace RimWorld
{
	public static class DrugCategoryExtension
	{
		public static bool IncludedIn(this DrugCategory lhs, DrugCategory rhs)
		{
			return lhs <= rhs;
		}

		public static string GetLabel(this DrugCategory category)
		{
			return category switch
			{
				DrugCategory.None => "DrugCategory_None".Translate(), 
				DrugCategory.Medical => "DrugCategory_Medical".Translate(), 
				DrugCategory.Social => "DrugCategory_Social".Translate(), 
				DrugCategory.Hard => "DrugCategory_Hard".Translate(), 
				_ => "DrugCategory_Any".Translate(), 
			};
		}
	}
}
