using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class InstallationDesignatorDatabase
	{
		private static Dictionary<ThingDef, Designator_Install> designators = new Dictionary<ThingDef, Designator_Install>();

		public static Designator_Install DesignatorFor(ThingDef artDef)
		{
			if (designators.TryGetValue(artDef, out var value))
			{
				return value;
			}
			value = NewDesignatorFor(artDef);
			designators.Add(artDef, value);
			return value;
		}

		private static Designator_Install NewDesignatorFor(ThingDef artDef)
		{
			return new Designator_Install
			{
				hotKey = KeyBindingDefOf.Misc1
			};
		}
	}
}
