namespace Verse;

public static class ThingIDMaker
{
	public static void GiveIDTo(Thing t)
	{
		if (t.def.HasThingIDNumber)
		{
			if (t.thingIDNumber != -1)
			{
				Log.Error("Giving ID to " + t?.ToString() + " which already has id " + t.thingIDNumber);
			}
			t.thingIDNumber = Find.UniqueIDsManager.GetNextThingID();
		}
	}
}
