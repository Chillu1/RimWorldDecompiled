using System;
using System.Collections.Generic;

namespace Verse
{
	public class SpecialThingFilterDef : Def
	{
		public ThingCategoryDef parentCategory;

		public string saveKey;

		public bool allowedByDefault;

		public bool configurable = true;

		public Type workerClass;

		[Unsaved(false)]
		private SpecialThingFilterWorker workerInt;

		public SpecialThingFilterWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (SpecialThingFilterWorker)Activator.CreateInstance(workerClass);
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (workerClass == null)
			{
				yield return "SpecialThingFilterDef " + defName + " has no worker class.";
			}
		}

		public static SpecialThingFilterDef Named(string defName)
		{
			return DefDatabase<SpecialThingFilterDef>.GetNamed(defName);
		}
	}
}
