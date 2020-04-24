using Verse;

namespace RimWorld
{
	public class MainButtonWorker_ToggleTab : MainButtonWorker
	{
		public override void Activate()
		{
			Find.MainTabsRoot.ToggleTab(def);
		}
	}
}
