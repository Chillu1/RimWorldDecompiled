using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class CompBiosculpterPod_Cycle : ThingComp
	{
		private List<string> tmpMissingResearchLabels = new List<string>();

		public CompProperties_BiosculpterPod_BaseCycle Props => (CompProperties_BiosculpterPod_BaseCycle)props;

		public abstract void CycleCompleted(Pawn occupant);

		public virtual string Description(Pawn tunedFor)
		{
			return Props.description;
		}

		public List<string> MissingResearchLabels()
		{
			tmpMissingResearchLabels.Clear();
			if (Props.requiredResearch.NullOrEmpty())
			{
				return tmpMissingResearchLabels;
			}
			foreach (ResearchProjectDef item in Props.requiredResearch)
			{
				if (!item.IsFinished)
				{
					tmpMissingResearchLabels.Add(item.LabelCap);
				}
			}
			return tmpMissingResearchLabels;
		}
	}
}
