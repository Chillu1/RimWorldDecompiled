using System;
using Verse;

namespace RimWorld
{
	public class BabyPlayDef : Def
	{
		public Type workerClass;

		public JobDef jobDef;

		private BabyPlayGiver workerInt;

		public BabyPlayGiver Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (BabyPlayGiver)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
