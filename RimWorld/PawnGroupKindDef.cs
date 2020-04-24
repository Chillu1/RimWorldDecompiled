using System;
using Verse;

namespace RimWorld
{
	public class PawnGroupKindDef : Def
	{
		public Type workerClass = typeof(PawnGroupKindWorker);

		[Unsaved(false)]
		private PawnGroupKindWorker workerInt;

		public PawnGroupKindWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnGroupKindWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
