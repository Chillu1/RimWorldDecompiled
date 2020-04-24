using System;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeDef : Def
	{
		public Type workerClass = typeof(PawnsArrivalModeWorker);

		public SimpleCurve selectionWeightCurve;

		public SimpleCurve pointsFactorCurve;

		public TechLevel minTechLevel;

		public bool forQuickMilitaryAid;

		public bool walkIn;

		[MustTranslate]
		public string textEnemy;

		[MustTranslate]
		public string textFriendly;

		[MustTranslate]
		public string textWillArrive;

		[Unsaved(false)]
		private PawnsArrivalModeWorker workerInt;

		public PawnsArrivalModeWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnsArrivalModeWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
