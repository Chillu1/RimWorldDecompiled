using System;
using RimWorld;

namespace Verse;

public static class ThingMaker
{
	public static Thing MakeThing(ThingDef def, ThingDef stuff = null)
	{
		if (stuff != null && !stuff.IsStuff)
		{
			Log.Error("MakeThing error: Tried to make " + def?.ToString() + " from " + stuff?.ToString() + " which is not a stuff. Assigning default.");
			stuff = GenStuff.DefaultStuffFor(def);
		}
		if (def.MadeFromStuff && stuff == null)
		{
			Log.Error("MakeThing error: " + def?.ToString() + " is madeFromStuff but stuff=null. Assigning default.");
			stuff = GenStuff.DefaultStuffFor(def);
		}
		if (!def.MadeFromStuff && stuff != null)
		{
			Log.Error("MakeThing error: " + def?.ToString() + " is not madeFromStuff but stuff=" + stuff?.ToString() + ". Setting to null.");
			stuff = null;
		}
		Thing obj = (Thing)Activator.CreateInstance(def.thingClass);
		obj.def = def;
		obj.SetStuffDirect(stuff);
		obj.PostMake();
		obj.PostPostMake();
		return obj;
	}
}
