using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class WindowStack
{
	public Window currentlyDrawnWindow;

	private List<Window> windows = new List<Window>();

	private List<int> immediateWindowsRequests = new List<int>();

	private bool updateInternalWindowsOrderLater;

	private Window focusedWindow;

	private static int uniqueWindowID;

	private bool gameStartDialogOpen;

	private float timeGameStartDialogClosed = -1f;

	private IntVec2 prevResolution = new IntVec2(UI.screenWidth, UI.screenHeight);

	private List<Window> windowStackOnGUITmpList = new List<Window>();

	private List<Window> updateImmediateWindowsListTmpList = new List<Window>();

	private List<Window> removeWindowsOfTypeTmpList = new List<Window>();

	private List<Window> closeWindowsTmpList = new List<Window>();

	public int Count => windows.Count;

	public Window this[int index] => windows[index];

	public IList<Window> Windows => windows.AsReadOnly();

	public FloatMenu FloatMenu => WindowOfType<FloatMenu>();

	public bool WindowsForcePause
	{
		get
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].forcePause)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool WindowsPreventCameraMotion
	{
		get
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].preventCameraMotion)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool WindowsPreventDrawTutor
	{
		get
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].preventDrawTutor)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float SecondsSinceClosedGameStartDialog
	{
		get
		{
			if (gameStartDialogOpen)
			{
				return 0f;
			}
			if (timeGameStartDialogClosed < 0f)
			{
				return 9999999f;
			}
			return Time.time - timeGameStartDialogClosed;
		}
	}

	public bool MouseObscuredNow => GetWindowAt(UI.MousePosUIInvertedUseEventIfCan) != currentlyDrawnWindow;

	public bool CurrentWindowGetsInput => GetsInput(currentlyDrawnWindow);

	public bool NonImmediateDialogWindowOpen
	{
		get
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (!(windows[i] is ImmediateWindow) && windows[i].layer == WindowLayer.Dialog)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool WindowsPreventSave
	{
		get
		{
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].preventSave)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnyWindowAbsorbingAllInput
	{
		get
		{
			foreach (Window window in windows)
			{
				if (window.absorbInputAroundWindow)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnySearchWidgetFocused
	{
		get
		{
			foreach (Window window in windows)
			{
				QuickSearchWidget commonSearchWidget = window.CommonSearchWidget;
				if (commonSearchWidget != null && commonSearchWidget.CurrentlyFocused())
				{
					return true;
				}
			}
			return false;
		}
	}

	public void WindowsUpdate()
	{
		AdjustWindowsIfResolutionChanged();
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].WindowUpdate();
		}
	}

	public void HandleEventsHighPriority()
	{
		if (Event.current.type == EventType.MouseDown && GetWindowAt(UI.GUIToScreenPoint(Event.current.mousePosition)) == null)
		{
			bool num = CloseWindowsBecauseClicked(null);
			NotifyOutsideClicks(null);
			if (num)
			{
				Event.current.Use();
			}
		}
		if (KeyBindingDefOf.Cancel.KeyDownEvent)
		{
			Notify_PressedCancel();
		}
		if (KeyBindingDefOf.Accept.KeyDownEvent)
		{
			Notify_PressedAccept();
		}
		if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown) && !GetsInput(null))
		{
			Event.current.Use();
		}
	}

	public void WindowStackOnGUI()
	{
		windowStackOnGUITmpList.Clear();
		windowStackOnGUITmpList.AddRange(windows);
		for (int num = windowStackOnGUITmpList.Count - 1; num >= 0; num--)
		{
			windowStackOnGUITmpList[num].ExtraOnGUI();
		}
		UpdateImmediateWindowsList();
		windowStackOnGUITmpList.Clear();
		windowStackOnGUITmpList.AddRange(windows);
		for (int i = 0; i < windowStackOnGUITmpList.Count; i++)
		{
			if (windowStackOnGUITmpList[i].drawShadow)
			{
				if (!windowStackOnGUITmpList[i].drawInScreenshotMode && Find.UIRoot.screenshotMode.Active)
				{
					continue;
				}
				GUI.color = new Color(1f, 1f, 1f, windowStackOnGUITmpList[i].shadowAlpha);
				Widgets.DrawShadowAround(windowStackOnGUITmpList[i].windowRect);
				GUI.color = Color.white;
			}
			windowStackOnGUITmpList[i].WindowOnGUI();
		}
		if (updateInternalWindowsOrderLater)
		{
			updateInternalWindowsOrderLater = false;
			UpdateInternalWindowsOrder();
		}
	}

	public void Notify_ClickedInsideWindow(Window window)
	{
		if (GetsInput(window))
		{
			windows.Remove(window);
			InsertAtCorrectPositionInList(window);
			focusedWindow = window;
		}
		else
		{
			Event.current.Use();
		}
		CloseWindowsBecauseClicked(window);
		NotifyOutsideClicks(window);
		updateInternalWindowsOrderLater = true;
	}

	public void Notify_ManuallySetFocus(Window window)
	{
		focusedWindow = window;
		updateInternalWindowsOrderLater = true;
	}

	public void Notify_PressedCancel()
	{
		for (int num = windows.Count - 1; num >= 0; num--)
		{
			if ((windows[num].closeOnCancel || windows[num].forceCatchAcceptAndCancelEventEvenIfUnfocused) && GetsInput(windows[num]))
			{
				windows[num].OnCancelKeyPressed();
				break;
			}
		}
	}

	public void Notify_PressedAccept()
	{
		for (int num = windows.Count - 1; num >= 0; num--)
		{
			if ((windows[num].closeOnAccept || windows[num].forceCatchAcceptAndCancelEventEvenIfUnfocused) && GetsInput(windows[num]))
			{
				windows[num].OnAcceptKeyPressed();
				break;
			}
		}
	}

	public void Notify_GameStartDialogOpened()
	{
		gameStartDialogOpen = true;
	}

	public void Notify_GameStartDialogClosed()
	{
		timeGameStartDialogClosed = Time.time;
		gameStartDialogOpen = false;
	}

	public bool IsOpen<WindowType>()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i] is WindowType)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOpen(Type type)
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOpen(Window window)
	{
		return windows.Contains(window);
	}

	public bool TryGetWindow<T>(out T window) where T : class
	{
		window = WindowOfType<T>();
		return window != null;
	}

	public WindowType WindowOfType<WindowType>() where WindowType : class
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i] is WindowType)
			{
				return windows[i] as WindowType;
			}
		}
		return null;
	}

	public bool GetsInput(Window window)
	{
		for (int num = windows.Count - 1; num >= 0; num--)
		{
			if (windows[num] == window)
			{
				return true;
			}
			if (windows[num].absorbInputAroundWindow)
			{
				return false;
			}
		}
		return true;
	}

	public void Add(Window window)
	{
		RemoveWindowsOfType(window.GetType());
		window.ID = uniqueWindowID++;
		window.PreOpen();
		InsertAtCorrectPositionInList(window);
		FocusAfterInsertIfShould(window);
		updateInternalWindowsOrderLater = true;
		window.PostOpen();
	}

	public void ImmediateWindow(int ID, Rect rect, WindowLayer layer, Action doWindowFunc, bool doBackground = true, bool absorbInputAroundWindow = false, float shadowAlpha = 1f, Action doClickOutsideFunc = null, bool ignoreScreenFader = false)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (ID == 0)
		{
			Log.Warning("Used 0 as immediate window ID.");
			return;
		}
		ID = -Math.Abs(ID);
		bool flag = false;
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].ID == ID)
			{
				ImmediateWindow obj = (ImmediateWindow)windows[i];
				obj.windowRect = rect;
				obj.doWindowFunc = doWindowFunc;
				obj.doClickOutsideFunc = doClickOutsideFunc;
				obj.layer = layer;
				obj.doWindowBackground = doBackground;
				obj.absorbInputAroundWindow = absorbInputAroundWindow;
				obj.shadowAlpha = shadowAlpha;
				obj.ignoreScreenFader = ignoreScreenFader;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AddNewImmediateWindow(ID, rect, layer, doWindowFunc, doBackground, absorbInputAroundWindow, shadowAlpha, doClickOutsideFunc, ignoreScreenFader);
		}
		immediateWindowsRequests.Add(ID);
	}

	public bool TryRemove(Type windowType, bool doCloseSound = true)
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].GetType() == windowType)
			{
				return TryRemove(windows[i], doCloseSound);
			}
		}
		return false;
	}

	public bool TryRemoveAssignableFromType(Type windowType, bool doCloseSound = true)
	{
		for (int i = 0; i < windows.Count; i++)
		{
			if (windowType.IsAssignableFrom(windows[i].GetType()))
			{
				return TryRemove(windows[i], doCloseSound);
			}
		}
		return false;
	}

	public bool TryRemove(Window window, bool doCloseSound = true)
	{
		bool flag = false;
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i] == window)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!window.OnCloseRequest())
		{
			return false;
		}
		if (doCloseSound && window.soundClose != null)
		{
			window.soundClose.PlayOneShotOnCamera();
		}
		window.PreClose();
		windows.Remove(window);
		window.PostClose();
		if (focusedWindow == window)
		{
			if (windows.Count > 0)
			{
				focusedWindow = windows[windows.Count - 1];
			}
			else
			{
				focusedWindow = null;
			}
			updateInternalWindowsOrderLater = true;
		}
		return true;
	}

	public Window GetWindowAt(Vector2 pos)
	{
		for (int num = windows.Count - 1; num >= 0; num--)
		{
			if (windows[num].windowRect.Contains(pos))
			{
				return windows[num];
			}
		}
		return null;
	}

	private void AddNewImmediateWindow(int ID, Rect rect, WindowLayer layer, Action doWindowFunc, bool doBackground, bool absorbInputAroundWindow, float shadowAlpha, Action doClickOutsideFunc, bool ignoreScreenFader)
	{
		if (ID >= 0)
		{
			Log.Error("Invalid immediate window ID.");
			return;
		}
		ImmediateWindow immediateWindow = new ImmediateWindow();
		immediateWindow.ID = ID;
		immediateWindow.layer = layer;
		immediateWindow.doWindowFunc = doWindowFunc;
		immediateWindow.doClickOutsideFunc = doClickOutsideFunc;
		immediateWindow.doWindowBackground = doBackground;
		immediateWindow.absorbInputAroundWindow = absorbInputAroundWindow;
		immediateWindow.shadowAlpha = shadowAlpha;
		immediateWindow.ignoreScreenFader = ignoreScreenFader;
		immediateWindow.PreOpen();
		immediateWindow.windowRect = rect;
		InsertAtCorrectPositionInList(immediateWindow);
		FocusAfterInsertIfShould(immediateWindow);
		updateInternalWindowsOrderLater = true;
		immediateWindow.PostOpen();
	}

	private void UpdateImmediateWindowsList()
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		updateImmediateWindowsListTmpList.Clear();
		updateImmediateWindowsListTmpList.AddRange(windows);
		for (int i = 0; i < updateImmediateWindowsListTmpList.Count; i++)
		{
			if (!IsImmediateWindow(updateImmediateWindowsListTmpList[i]))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < immediateWindowsRequests.Count; j++)
			{
				if (immediateWindowsRequests[j] == updateImmediateWindowsListTmpList[i].ID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				TryRemove(updateImmediateWindowsListTmpList[i]);
			}
		}
		immediateWindowsRequests.Clear();
	}

	private void InsertAtCorrectPositionInList(Window window)
	{
		int index = 0;
		for (int i = 0; i < windows.Count; i++)
		{
			if (window.layer >= windows[i].layer)
			{
				index = i + 1;
			}
		}
		windows.Insert(index, window);
		updateInternalWindowsOrderLater = true;
	}

	private void FocusAfterInsertIfShould(Window window)
	{
		if (!window.focusWhenOpened)
		{
			return;
		}
		int num = windows.Count - 1;
		while (num >= 0)
		{
			if (windows[num] == window)
			{
				focusedWindow = windows[num];
				updateInternalWindowsOrderLater = true;
				break;
			}
			if (windows[num] != focusedWindow)
			{
				num--;
				continue;
			}
			break;
		}
	}

	private void AdjustWindowsIfResolutionChanged()
	{
		IntVec2 a = new IntVec2(UI.screenWidth, UI.screenHeight);
		if (!UnityGUIBugsFixer.ResolutionsEqual(a, prevResolution))
		{
			prevResolution = a;
			for (int i = 0; i < windows.Count; i++)
			{
				windows[i].Notify_ResolutionChanged();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}
	}

	private void RemoveWindowsOfType(Type type)
	{
		removeWindowsOfTypeTmpList.Clear();
		removeWindowsOfTypeTmpList.AddRange(windows);
		for (int i = 0; i < removeWindowsOfTypeTmpList.Count; i++)
		{
			if (removeWindowsOfTypeTmpList[i].onlyOneOfTypeAllowed && removeWindowsOfTypeTmpList[i].GetType() == type)
			{
				TryRemove(removeWindowsOfTypeTmpList[i]);
			}
		}
	}

	private void NotifyOutsideClicks(Window clickedWindow)
	{
		foreach (Window window in windows)
		{
			if (window != clickedWindow)
			{
				window.Notify_ClickOutsideWindow();
			}
		}
	}

	private bool CloseWindowsBecauseClicked(Window clickedWindow)
	{
		closeWindowsTmpList.Clear();
		closeWindowsTmpList.AddRange(windows);
		bool result = false;
		int num = closeWindowsTmpList.Count - 1;
		while (num >= 0 && closeWindowsTmpList[num] != clickedWindow)
		{
			if (closeWindowsTmpList[num].closeOnClickedOutside)
			{
				result = true;
				TryRemove(closeWindowsTmpList[num]);
			}
			num--;
		}
		return result;
	}

	private bool IsImmediateWindow(Window window)
	{
		return window.ID < 0;
	}

	private void UpdateInternalWindowsOrder()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			GUI.BringWindowToFront(windows[i].ID);
		}
		if (focusedWindow != null)
		{
			GUI.FocusWindow(focusedWindow.ID);
		}
	}
}
