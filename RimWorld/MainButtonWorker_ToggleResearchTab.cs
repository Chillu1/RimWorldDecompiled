using Verse;

namespace RimWorld
{
	public class MainButtonWorker_ToggleResearchTab : MainButtonWorker_ToggleTab
	{
		public override float ButtonBarPercent => Find.ResearchManager.currentProj?.ProgressPercent ?? 0f;
	}
}
