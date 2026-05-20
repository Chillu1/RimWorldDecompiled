using System;

namespace Verse;

public class ThingListChangedCallbacks
{
	public Action<Thing> onThingAdded = delegate
	{
	};

	public Action<Thing> onThingRemoved = delegate
	{
	};
}
