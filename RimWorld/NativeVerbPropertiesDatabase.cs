using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class NativeVerbPropertiesDatabase
	{
		public static List<VerbProperties> allVerbDefs;

		static NativeVerbPropertiesDatabase()
		{
			allVerbDefs = VerbDefsHardcodedNative.AllVerbDefs().ToList();
		}

		public static VerbProperties VerbWithCategory(VerbCategory id)
		{
			VerbProperties verbProperties = allVerbDefs.Where((VerbProperties v) => v.category == id).FirstOrDefault();
			if (verbProperties == null)
			{
				Log.Error("Failed to find Verb with id " + id);
			}
			return verbProperties;
		}
	}
}
