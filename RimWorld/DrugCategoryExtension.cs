namespace RimWorld
{
	public static class DrugCategoryExtension
	{
		public static bool IncludedIn(this DrugCategory lhs, DrugCategory rhs)
		{
			return lhs <= rhs;
		}
	}
}
