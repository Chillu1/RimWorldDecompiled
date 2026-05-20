using System;
using Verse;

namespace RimWorld
{
	public class RaidAgeRestrictionDef : Def
	{
		public Type workerClass = typeof(RaidAgeRestrictionWorker);

		public DevelopmentalStage developmentStage;

		public FloatRange? ageRange;

		public float threatPointsFactor = 1f;

		public int earliestDay;

		public float chance;

		[MustTranslate]
		public string arrivalTextExtra;

		private RaidAgeRestrictionWorker workerInt;

		public RaidAgeRestrictionWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (RaidAgeRestrictionWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
