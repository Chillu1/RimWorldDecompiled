using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

public abstract class Window
{
	public WindowLayer layer = WindowLayer.Dialog;

	public string optionalTitle;

	public bool doCloseX;

	public bool doCloseButton;

	public bool closeOnAccept = true;

	public bool closeOnCancel = true;

	public bool forceCatchAcceptAndCancelEventEvenIfUnfocused;

	public bool closeOnClickedOutside;

	public bool forcePause;

	public bool preventCameraMotion = true;

	public bool preventDrawTutor;

	public bool doWindowBackground = true;

	public bool onlyOneOfTypeAllowed = true;

	public bool absorbInputAroundWindow;

	public bool resizeable;

	public bool draggable;

	public bool drawShadow = true;

	public bool focusWhenOpened = true;

	public float shadowAlpha = 1f;

	public SoundDef soundAppear;

	public SoundDef soundClose;

	public SoundDef soundAmbient;

	public bool silenceAmbientSound;

	public bool grayOutIfOtherDialogOpen;

	public Vector2 commonSearchWidgetOffset = new Vector2(0f, CloseButSize.y - QuickSearchSize.y) / 2f;

	public bool openMenuOnCancel;

	public bool preventSave;

	public bool drawInScreenshotMode = true;

	public bool onlyDrawInDevMode;

	public bool ignoreScreenFader;

	public const float StandardMargin = 18f;

	public const float FooterRowHeight = 55f;

	public static readonly Vector2 CloseButSize = new Vector2(120f, 40f);

	public static readonly Vector2 QuickSearchSize = new Vector2(240f, 24f);

	public int ID;

	public Rect windowRect;

	private Sustainer sustainerAmbient;

	private WindowResizer resizer;

	private bool resizeLater;

	private Rect resizeLaterRect;

	private IWindowDrawing windowDrawing;

	private string onGUIProfilerLabelCached;

	public string extraOnGUIProfilerLabelCached;

	private GUI.WindowFunction innerWindowOnGUICached;

	private Action notify_CommonSearchChangedCached;

	public virtual Vector2 InitialSize => new Vector2(500f, 500f);

	protected virtual float Margin => 18f;

	public virtual bool IsDebug => false;

	public bool IsOpen => Find.WindowStack.IsOpen(this);

	public virtual QuickSearchWidget CommonSearchWidget => null;

	public virtual string CloseButtonText => "CloseButton".Translate();

	public Window(IWindowDrawing customWindowDrawing = null)
	{
		soundAppear = SoundDefOf.DialogBoxAppear;
		soundClose = SoundDefOf.Click;
		onGUIProfilerLabelCached = "WindowOnGUI: " + GetType().Name;
		extraOnGUIProfilerLabelCached = "ExtraOnGUI: " + GetType().Name;
		innerWindowOnGUICached = InnerWindowOnGUI;
		notify_CommonSearchChangedCached = Notify_CommonSearchChanged;
		windowDrawing = customWindowDrawing ?? new DefaultWindowDrawing();
	}

	public virtual void WindowUpdate()
	{
		if (sustainerAmbient != null)
		{
			sustainerAmbient.Maintain();
		}
	}

	public abstract void DoWindowContents(Rect inRect);

	public virtual void ExtraOnGUI()
	{
	}

	public virtual void PreOpen()
	{
		SetInitialSizeAndPosition();
		CommonSearchWidget?.Reset();
		if (layer == WindowLayer.Dialog)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.DesignatorManager.Dragger.EndDrag();
				Find.DesignatorManager.Deselect();
				Find.Selector.Notify_DialogOpened();
			}
			if (Find.World != null)
			{
				Find.WorldSelector.Notify_DialogOpened();
			}
		}
	}

	public virtual void PostOpen()
	{
		if (soundAppear != null)
		{
			soundAppear.PlayOneShotOnCamera();
		}
		if (soundAmbient != null)
		{
			sustainerAmbient = soundAmbient.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame));
		}
	}

	public virtual bool OnCloseRequest()
	{
		return true;
	}

	public virtual void PreClose()
	{
	}

	public virtual void PostClose()
	{
	}

	public virtual void WindowOnGUI()
	{
		if ((!drawInScreenshotMode && Find.UIRoot.screenshotMode.Active) || (onlyDrawInDevMode && !Prefs.DevMode))
		{
			return;
		}
		if (resizeable)
		{
			if (resizer == null)
			{
				resizer = new WindowResizer();
			}
			if (resizeLater)
			{
				resizeLater = false;
				windowRect = resizeLaterRect;
			}
		}
		windowRect = windowRect.Rounded();
		windowRect = GUI.Window(ID, windowRect, innerWindowOnGUICached, "", windowDrawing.EmptyStyle);
	}

	private void InnerWindowOnGUI(int x)
	{
		UnityGUIBugsFixer.OnGUI();
		SteamDeck.WindowOnGUI();
		OriginalEventUtility.RecordOriginalEvent(Event.current);
		Rect rect = windowRect.AtZero();
		Find.WindowStack.currentlyDrawnWindow = this;
		if (doWindowBackground)
		{
			windowDrawing.DoWindowBackground(rect);
		}
		if (KeyBindingDefOf.Cancel.KeyDownEvent)
		{
			Find.WindowStack.Notify_PressedCancel();
		}
		if (KeyBindingDefOf.Accept.KeyDownEvent)
		{
			Find.WindowStack.Notify_PressedAccept();
		}
		if (Event.current.type == EventType.MouseDown)
		{
			Find.WindowStack.Notify_ClickedInsideWindow(this);
		}
		if (Event.current.type == EventType.KeyDown && !Find.WindowStack.GetsInput(this))
		{
			Event.current.Use();
		}
		if (!optionalTitle.NullOrEmpty())
		{
			GUI.Label(new Rect(Margin, Margin, windowRect.width, 25f), optionalTitle);
		}
		if (doCloseX && windowDrawing.DoClostButtonSmall(rect))
		{
			Close();
		}
		if (resizeable && Event.current.type != EventType.Repaint)
		{
			Rect rect2 = resizer.DoResizeControl(windowRect);
			if (rect2 != windowRect)
			{
				resizeLater = true;
				resizeLaterRect = rect2;
			}
		}
		Rect rect3 = rect.ContractedBy(Margin);
		if (!optionalTitle.NullOrEmpty())
		{
			rect3.yMin += Margin + 25f;
		}
		CommonSearchWidget?.OnGUI(QuickSearchWidgetRect(rect, rect3), notify_CommonSearchChangedCached);
		windowDrawing.BeginGroup(rect3);
		try
		{
			DoWindowContents(rect3.AtZero());
		}
		catch (Exception ex)
		{
			Log.Error("Exception filling window for " + GetType()?.ToString() + ": " + ex);
		}
		windowDrawing.EndGroup();
		LateWindowOnGUI(rect3);
		if (grayOutIfOtherDialogOpen)
		{
			IList<Window> windows = Find.WindowStack.Windows;
			for (int i = 0; i < windows.Count; i++)
			{
				if (windows[i].layer == WindowLayer.Dialog && !(windows[i] is Page) && windows[i] != this)
				{
					windowDrawing.DoGrayOut(rect);
					break;
				}
			}
		}
		if (resizeable && Event.current.type == EventType.Repaint)
		{
			resizer.DoResizeControl(windowRect);
		}
		if (doCloseButton)
		{
			Text.Font = GameFont.Small;
			Rect rect4 = new Rect(rect.width / 2f - CloseButSize.x / 2f, rect.height - 55f, CloseButSize.x, CloseButSize.y);
			if (windowDrawing.DoCloseButton(rect4, CloseButtonText))
			{
				Close();
			}
		}
		if (KeyBindingDefOf.Cancel.KeyDownEvent && IsOpen)
		{
			OnCancelKeyPressed();
		}
		if (draggable)
		{
			GUI.DragWindow();
		}
		else if (Event.current.type == EventType.MouseDown)
		{
			Event.current.Use();
		}
		if (!ignoreScreenFader)
		{
			ScreenFader.OverlayOnGUI(rect.size);
		}
		Find.WindowStack.currentlyDrawnWindow = null;
		OriginalEventUtility.Reset();
	}

	protected virtual Rect QuickSearchWidgetRect(Rect winRect, Rect inRect)
	{
		Vector2 vector = commonSearchWidgetOffset;
		return new Rect(winRect.x + vector.x, winRect.height - 55f + vector.y, QuickSearchSize.x, QuickSearchSize.y);
	}

	protected virtual void LateWindowOnGUI(Rect inRect)
	{
	}

	public virtual void Close(bool doCloseSound = true)
	{
		Find.WindowStack.TryRemove(this, doCloseSound);
	}

	public virtual bool CausesMessageBackground()
	{
		return false;
	}

	protected virtual void SetInitialSizeAndPosition()
	{
		Vector2 initialSize = InitialSize;
		windowRect = new Rect(((float)UI.screenWidth - initialSize.x) / 2f, ((float)UI.screenHeight - initialSize.y) / 2f, initialSize.x, initialSize.y);
		windowRect = windowRect.Rounded();
	}

	public virtual void OnCancelKeyPressed()
	{
		if (closeOnCancel)
		{
			Close();
			Event.current.Use();
		}
		if (openMenuOnCancel)
		{
			Find.MainTabsRoot.ToggleTab(MainButtonDefOf.Menu);
		}
	}

	public virtual void OnAcceptKeyPressed()
	{
		if (closeOnAccept)
		{
			Close();
			Event.current.Use();
		}
	}

	public virtual void Notify_ResolutionChanged()
	{
		SetInitialSizeAndPosition();
	}

	public virtual void Notify_CommonSearchChanged()
	{
	}

	public virtual void Notify_ClickOutsideWindow()
	{
		CommonSearchWidget?.Unfocus();
	}
}
