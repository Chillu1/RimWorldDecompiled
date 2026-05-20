using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LudeonTK;

[StaticConstructorOnStartup]
public class EditWindow_Log : EditWindow
{
	private static LogMessage selectedMessage = null;

	private static Vector2 messagesScrollPosition;

	private static Vector2 detailsScrollPosition;

	private static float detailsPaneHeight = 100f;

	private static bool canAutoOpen = true;

	private static bool showErrors = true;

	private static bool showWarnings = true;

	private static bool showMessages = true;

	public static bool wantsToOpen = false;

	private float listingViewHeight;

	private bool borderDragging;

	private const float CountWidth = 28f;

	private const float Yinc = 25f;

	private const float DetailsPaneBorderHeight = 7f;

	private const float DetailsPaneMinHeight = 10f;

	private const float ListingMinHeight = 80f;

	private const float TopAreaHeight = 26f;

	private const float MessageMaxHeight = 30f;

	private static readonly Texture2D AltMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.17f, 0.17f, 0.17f, 0.85f));

	private static readonly Texture2D SelectedMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.25f, 0.25f, 0.17f, 0.85f));

	private static readonly Texture2D StackTraceAreaTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.5f));

	private static readonly Texture2D StackTraceBorderTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.3f, 0.3f, 1f));

	private static readonly Texture2D WarningIcon = Resources.Load<Texture2D>("Textures/UI/Widgets/YellowWarning");

	private static readonly Texture2D ErrorIcon = Resources.Load<Texture2D>("Textures/UI/Widgets/Error");

	private static readonly Texture2D InfoIcon = Resources.Load<Texture2D>("Textures/UI/Buttons/InfoButton");

	private static readonly string MessageDetailsControlName = "MessageDetailsTextArea";

	public override Vector2 InitialSize => new Vector2((float)UI.screenWidth / 2f, (float)UI.screenHeight / 2f);

	public override bool IsDebug => true;

	private static LogMessage SelectedMessage
	{
		get
		{
			return selectedMessage;
		}
		set
		{
			if (selectedMessage != value)
			{
				selectedMessage = value;
				if (UnityData.IsInMainThread && GUI.GetNameOfFocusedControl() == MessageDetailsControlName)
				{
					UI.UnfocusCurrentControl();
				}
			}
		}
	}

	public EditWindow_Log()
	{
		optionalTitle = "Debug log";
		closeOnAccept = false;
		closeOnCancel = Prefs.CloseLogWindowOnEscape;
		drawInScreenshotMode = true;
	}

	public static void TryAutoOpen()
	{
		if (canAutoOpen)
		{
			wantsToOpen = true;
		}
	}

	public static void ClearSelectedMessage()
	{
		SelectedMessage = null;
		detailsScrollPosition = Vector2.zero;
	}

	public static void SelectLastMessage(bool expandDetailsPane = false)
	{
		ClearSelectedMessage();
		SelectedMessage = Log.Messages.LastOrDefault();
		messagesScrollPosition.y = (float)Log.Messages.Count() * 30f;
		if (expandDetailsPane)
		{
			detailsPaneHeight = 9999f;
		}
	}

	public static void ClearAll()
	{
		ClearSelectedMessage();
		messagesScrollPosition = Vector2.zero;
	}

	public override void PostClose()
	{
		base.PostClose();
		wantsToOpen = false;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Tiny;
		float x = inRect.x;
		float y = inRect.y;
		DoRowButton(ref x, y, "Clear", "Clear all log messages.", delegate
		{
			Log.Clear();
			ClearAll();
		});
		DoRowButton(ref x, y, "Trace big", "Set the stack trace to be large on screen.", delegate
		{
			detailsPaneHeight = 700f;
		});
		DoRowButton(ref x, y, "Trace medium", "Set the stack trace to be medium-sized on screen.", delegate
		{
			detailsPaneHeight = 300f;
		});
		DoRowButton(ref x, y, "Trace small", "Set the stack trace to be small on screen.", delegate
		{
			detailsPaneHeight = 100f;
		});
		if (canAutoOpen)
		{
			DoRowButton(ref x, y, "Auto-open is ON", null, delegate
			{
				canAutoOpen = false;
			});
		}
		else
		{
			DoRowButton(ref x, y, "Auto-open is OFF", null, delegate
			{
				canAutoOpen = true;
			});
		}
		DoRowButton(ref x, y, "Copy to clipboard", "Copy all messages to the clipboard.", delegate
		{
			CopyAllMessagesToClipboard();
		});
		if (DebugSettings.pauseOnError)
		{
			DoRowButton(ref x, y, "Pause on error is ON", null, delegate
			{
				DebugSettings.pauseOnError = false;
			});
		}
		else
		{
			DoRowButton(ref x, y, "Pause on error is OFF", null, delegate
			{
				DebugSettings.pauseOnError = true;
			});
		}
		DoImageToggle(ref x, y, InfoIcon, "Show info messages", ref showMessages);
		DoImageToggle(ref x, y, WarningIcon, "Show warning messages", ref showWarnings);
		DoImageToggle(ref x, y, ErrorIcon, "Show error messages", ref showErrors);
		Text.Font = GameFont.Small;
		Rect rect = new Rect(inRect);
		rect.yMin += 26f;
		rect.yMax = inRect.height;
		if (selectedMessage != null)
		{
			rect.yMax -= detailsPaneHeight;
		}
		Rect detailsRect = new Rect(inRect);
		detailsRect.yMin = rect.yMax;
		DoMessagesListing(rect);
		DoMessageDetails(detailsRect, inRect);
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect))
		{
			ClearSelectedMessage();
		}
		detailsPaneHeight = Mathf.Max(detailsPaneHeight, 10f);
		detailsPaneHeight = Mathf.Min(detailsPaneHeight, inRect.height - 80f);
	}

	public static void Notify_MessageDequeued(LogMessage oldMessage)
	{
		if (SelectedMessage == oldMessage)
		{
			SelectedMessage = null;
		}
	}

	private void DoMessagesListing(Rect listingRect)
	{
		Rect viewRect = new Rect(0f, 0f, listingRect.width - 16f, listingViewHeight + 100f);
		DevGUI.BeginScrollView(listingRect, ref messagesScrollPosition, viewRect);
		float width = viewRect.width - 28f;
		Text.Font = GameFont.Tiny;
		float num = 0f;
		bool flag = false;
		foreach (LogMessage message in Log.Messages)
		{
			if ((!showMessages && message.type == LogMessageType.Message) || (!showWarnings && message.type == LogMessageType.Warning) || (!showErrors && message.type == LogMessageType.Error))
			{
				if (selectedMessage == message)
				{
					ClearSelectedMessage();
				}
				continue;
			}
			string text = message.ToString();
			if (text.Length > 1000)
			{
				text = text.Substring(0, 1000);
			}
			float num2 = Math.Min(Text.TinyFontSupported ? 30f : Text.LineHeight, Text.CalcHeight(text, width));
			GUI.color = new Color(1f, 1f, 1f, 0.7f);
			DevGUI.Label(new Rect(4f, num, 28f, num2), message.repeats.ToStringCached());
			Rect rect = new Rect(28f, num, width, num2);
			if (selectedMessage == message)
			{
				GUI.DrawTexture(rect, SelectedMessageTex);
			}
			else if (flag)
			{
				GUI.DrawTexture(rect, AltMessageTex);
			}
			if (DevGUI.ButtonInvisible(rect))
			{
				ClearSelectedMessage();
				SelectedMessage = message;
			}
			GUI.color = message.Color;
			DevGUI.Label(rect, text);
			num += num2;
			flag = !flag;
		}
		if (Event.current.type == EventType.Layout)
		{
			listingViewHeight = num;
		}
		DevGUI.EndScrollView();
		GUI.color = Color.white;
	}

	private void DoMessageDetails(Rect detailsRect, Rect outRect)
	{
		if (selectedMessage != null)
		{
			Rect rect = detailsRect;
			rect.height = 7f;
			Rect rect2 = detailsRect;
			rect2.yMin = rect.yMax;
			GUI.DrawTexture(rect, StackTraceBorderTex);
			if (Mouse.IsOver(rect))
			{
				DevGUI.DrawHighlight(rect);
			}
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
			{
				borderDragging = true;
				Event.current.Use();
			}
			if (borderDragging)
			{
				detailsPaneHeight = outRect.height + Mathf.Round(3.5f) - Event.current.mousePosition.y;
			}
			if (Event.current.rawType == EventType.MouseUp)
			{
				borderDragging = false;
			}
			GUI.DrawTexture(rect2, StackTraceAreaTex);
			string text = selectedMessage.text + "\n" + selectedMessage.StackTrace;
			GUI.SetNextControlName(MessageDetailsControlName);
			DevGUI.TextAreaScrollable(rect2, text, ref detailsScrollPosition, readOnly: true);
		}
	}

	private void CopyAllMessagesToClipboard()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (LogMessage message in Log.Messages)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(message.text);
			stringBuilder.Append(message.StackTrace);
			if (stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.AppendLine();
			}
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
	}
}
