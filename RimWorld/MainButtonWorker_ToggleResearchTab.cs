using Verse;

namespace RimWorld;

public class MainButtonWorker_ToggleResearchTab : MainButtonWorker_ToggleTab
{
	public override float ButtonBarPercent => Find.ResearchManager.GetProject()?.ProgressPercent ?? 0f;
}
