using RimWorld;
using System;

namespace Verse
{
	public static class ThingMaker
	{
		public static Thing MakeThing(ThingDef def, ThingDef stuff = null)
		{
			if (stuff != null && !stuff.IsStuff)
			{
				Log.Error(string.Concat("MakeThing error: Tried to make ", def, " from ", stuff, " which is not a stuff. Assigning default."));
				stuff = GenStuff.DefaultStuffFor(def);
			}
			if (def.MadeFromStuff && stuff == null)
			{
				Log.Error(string.Concat("MakeThing error: ", def, " is madeFromStuff but stuff=null. Assigning default."));
				stuff = GenStuff.DefaultStuffFor(def);
			}
			if (!def.MadeFromStuff && stuff != null)
			{
				Log.Error(string.Concat("MakeThing error: ", def, " is not madeFromStuff but stuff=", stuff, ". Setting to null."));
				stuff = null;
			}
			Thing obj = (Thing)Activator.CreateInstance(def.thingClass);
			obj.def = def;
			obj.SetStuffDirect(stuff);
			obj.PostMake();
			return obj;
		}
	}
}
