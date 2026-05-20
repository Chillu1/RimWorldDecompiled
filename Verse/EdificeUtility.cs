namespace Verse;

public static class EdificeUtility
{
	public static bool IsEdifice(this BuildableDef def)
	{
		if (def is ThingDef { category: ThingCategory.Building } thingDef)
		{
			return thingDef.building.isEdifice;
		}
		return false;
	}
}
