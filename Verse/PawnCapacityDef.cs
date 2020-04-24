using System;

namespace Verse
{
	public class PawnCapacityDef : Def
	{
		public int listOrder;

		public Type workerClass = typeof(PawnCapacityWorker);

		[MustTranslate]
		public string labelMechanoids = "";

		[MustTranslate]
		public string labelAnimals = "";

		public bool showOnHumanlikes = true;

		public bool showOnAnimals = true;

		public bool showOnMechanoids = true;

		public bool lethalFlesh;

		public bool lethalMechanoids;

		public float minForCapable;

		public float minValue;

		public bool zeroIfCannotBeAwake;

		public bool showOnCaravanHealthTab;

		[Unsaved(false)]
		private PawnCapacityWorker workerInt;

		public PawnCapacityWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnCapacityWorker)Activator.CreateInstance(workerClass);
				}
				return workerInt;
			}
		}

		public string GetLabelFor(Pawn pawn)
		{
			return GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike);
		}

		public string GetLabelFor(bool isFlesh, bool isHumanlike)
		{
			if (isHumanlike)
			{
				return label;
			}
			if (isFlesh)
			{
				if (!labelAnimals.NullOrEmpty())
				{
					return labelAnimals;
				}
				return label;
			}
			if (!labelMechanoids.NullOrEmpty())
			{
				return labelMechanoids;
			}
			return label;
		}
	}
}
