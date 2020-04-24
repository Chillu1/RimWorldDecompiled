namespace Verse
{
	public static class EdificeUtility
	{
		public static bool IsEdifice(this BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null && thingDef.category == ThingCategory.Building)
			{
				return thingDef.building.isEdifice;
			}
			return false;
		}
	}
}
