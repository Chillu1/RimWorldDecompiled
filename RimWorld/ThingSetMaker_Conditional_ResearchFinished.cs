using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Conditional_ResearchFinished : ThingSetMaker_Conditional
	{
		public ResearchProjectDef researchProject;

		protected override bool Condition(ThingSetMakerParams parms)
		{
			return researchProject.IsFinished;
		}
	}
}
