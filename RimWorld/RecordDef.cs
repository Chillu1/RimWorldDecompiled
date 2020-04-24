using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RecordDef : Def
	{
		public RecordType type;

		public Type workerClass = typeof(RecordWorker);

		public List<JobDef> measuredTimeJobs;

		[Unsaved(false)]
		private RecordWorker workerInt;

		public RecordWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (RecordWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
