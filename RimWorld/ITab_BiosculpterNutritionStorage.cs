namespace RimWorld
{
	public class ITab_BiosculpterNutritionStorage : ITab_Storage
	{
		protected override bool IsPrioritySettingVisible => false;

		public ITab_BiosculpterNutritionStorage()
		{
			labelKey = "Nutrition";
		}
	}
}
