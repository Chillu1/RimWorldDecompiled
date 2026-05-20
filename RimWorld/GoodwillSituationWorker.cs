using Verse;

namespace RimWorld
{
	public abstract class GoodwillSituationWorker
	{
		public GoodwillSituationDef def;

		public virtual string GetPostProcessedLabel(Faction other)
		{
			return def.label;
		}

		public string GetPostProcessedLabelCap(Faction other)
		{
			return GetPostProcessedLabel(other).CapitalizeFirst(def);
		}

		public virtual int GetMaxGoodwill(Faction other)
		{
			return 100;
		}

		public virtual int GetNaturalGoodwillOffset(Faction other)
		{
			return 0;
		}
	}
}
