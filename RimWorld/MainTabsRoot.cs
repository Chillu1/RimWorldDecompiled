using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabsRoot
	{
		public MainButtonDef OpenTab => Find.WindowStack.WindowOfType<MainTabWindow>()?.def;

		public void HandleLowPriorityShortcuts()
		{
			if (OpenTab == MainButtonDefOf.Inspect && (Find.Selector.NumSelected == 0 || WorldRendererUtility.WorldRenderedNow))
			{
				EscapeCurrentTab();
			}
			if (Find.Selector.NumSelected == 0 && Event.current.type == EventType.MouseDown && Event.current.button == 1 && !WorldRendererUtility.WorldRenderedNow)
			{
				Event.current.Use();
				MainButtonDefOf.Architect.Worker.InterfaceTryActivate();
			}
			if (OpenTab != null && OpenTab != MainButtonDefOf.Inspect && Event.current.type == EventType.MouseDown && Event.current.button != 2)
			{
				EscapeCurrentTab();
				if (Event.current.button == 0)
				{
					Find.Selector.ClearSelection();
					Find.WorldSelector.ClearSelection();
				}
			}
		}

		public void EscapeCurrentTab(bool playSound = true)
		{
			SetCurrentTab(null, playSound);
		}

		public void SetCurrentTab(MainButtonDef tab, bool playSound = true)
		{
			if (tab != OpenTab)
			{
				ToggleTab(tab, playSound);
			}
		}

		public void ToggleTab(MainButtonDef newTab, bool playSound = true)
		{
			if (OpenTab == null && newTab == null)
			{
				return;
			}
			if (OpenTab == newTab)
			{
				Find.WindowStack.TryRemove(OpenTab.TabWindow);
				if (playSound)
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
				return;
			}
			if (OpenTab != null)
			{
				Find.WindowStack.TryRemove(OpenTab.TabWindow, doCloseSound: false);
			}
			if (newTab != null)
			{
				Find.WindowStack.Add(newTab.TabWindow);
			}
			if (playSound)
			{
				if (newTab == null)
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.TabOpen.PlayOneShotOnCamera();
				}
			}
			if (TutorSystem.TutorialMode && newTab != null)
			{
				TutorSystem.Notify_Event("Open-MainTab-" + newTab.defName);
			}
		}
	}
}
