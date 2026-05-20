using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class ITab : InspectTabBase
{
	protected object SelObject => Find.Selector.SingleSelectedObject;

	protected List<object> AllSelObjects => Find.Selector.SelectedObjects;

	protected Thing SelThing => Find.Selector.SingleSelectedThing;

	protected virtual Pawn SelPawn => SelThing as Pawn;

	private MainTabWindow_Inspect InspectPane => (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;

	protected override bool StillValid
	{
		get
		{
			if (Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
			{
				return false;
			}
			MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow;
			if (mainTabWindow_Inspect.CurTabs != null)
			{
				return mainTabWindow_Inspect.CurTabs.Contains(this);
			}
			return false;
		}
	}

	protected override float PaneTopY => InspectPane.PaneTopY;

	protected override void CloseTab()
	{
		InspectPane.CloseOpenTab();
		SoundDefOf.TabClose.PlayOneShotOnCamera();
	}
}
