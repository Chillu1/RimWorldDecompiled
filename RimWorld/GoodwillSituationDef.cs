using System;
using Verse;

namespace RimWorld
{
	public class GoodwillSituationDef : Def
	{
		public Type workerClass = typeof(GoodwillSituationWorker);

		public int baseMaxGoodwill = 100;

		public MemeDef meme;

		public MemeDef otherMeme;

		public int naturalGoodwillOffset;

		public bool versusAll;

		[Unsaved(false)]
		private GoodwillSituationWorker workerInt;

		public GoodwillSituationWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (GoodwillSituationWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
