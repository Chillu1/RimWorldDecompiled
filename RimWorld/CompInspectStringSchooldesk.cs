using Verse;

namespace RimWorld
{
	public class CompInspectStringSchooldesk : CompInspectString
	{
		public override string CompInspectStringExtra()
		{
			float num = LearningUtility.SchoolDeskLearningRate(parent);
			if (num > 1f)
			{
				return base.Props.inspectString + ": " + (num - 1f).ToStringPercent();
			}
			return null;
		}
	}
}
