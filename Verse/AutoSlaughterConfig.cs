namespace Verse
{
	public class AutoSlaughterConfig : IExposable
	{
		public ThingDef animal;

		public int maxTotal = -1;

		public int maxMales = -1;

		public int maxMalesYoung = -1;

		public int maxFemales = -1;

		public int maxFemalesYoung = -1;

		public bool allowSlaughterPregnant;

		public bool allowSlaughterBonded;

		public string uiMaxTotalBuffer;

		public string uiMaxMalesBuffer;

		public string uiMaxMalesYoungBuffer;

		public string uiMaxFemalesBuffer;

		public string uiMaxFemalesYoungBuffer;

		public const int NoLimit = -1;

		public bool AnyLimit
		{
			get
			{
				if (maxTotal == -1 && maxMales == -1 && maxFemales == -1 && maxMalesYoung == -1 && maxFemalesYoung == -1 && allowSlaughterPregnant)
				{
					return !allowSlaughterBonded;
				}
				return true;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref animal, "animal");
			Scribe_Values.Look(ref maxTotal, "maxTotal", -1);
			Scribe_Values.Look(ref maxMales, "maxMales", -1);
			Scribe_Values.Look(ref maxMalesYoung, "maxMalesYoung", -1);
			Scribe_Values.Look(ref maxFemales, "maxFemales", -1);
			Scribe_Values.Look(ref maxFemalesYoung, "maxFemalesYoung", -1);
			Scribe_Values.Look(ref allowSlaughterPregnant, "allowSlaughterPregnant", defaultValue: false);
			Scribe_Values.Look(ref allowSlaughterBonded, "allowSlaughterBonded", defaultValue: false);
		}
	}
}
