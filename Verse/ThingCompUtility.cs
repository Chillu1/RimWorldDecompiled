namespace Verse
{
	public static class ThingCompUtility
	{
		public static T TryGetComp<T>(this Thing thing) where T : ThingComp
		{
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingWithComps == null)
			{
				return null;
			}
			return thingWithComps.GetComp<T>();
		}
	}
}
