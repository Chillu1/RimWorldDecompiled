using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class EditWindow_Log : EditWindow
	{
		private static LogMessage selectedMessage = null;

		private static Vector2 messagesScrollPosition;

		private static Vector2 detailsScrollPosition;

		private static float detailsPaneHeight = 100f;

		private static bool canAutoOpen = true;

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
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			if (widgetRow.ButtonText("Clear", "Clear all log messages."))
			{
				Log.Clear();
				ClearAll();
			}
			if (widgetRow.ButtonText("Trace big", "Set the stack trace to be large on screen."))
			{
				detailsPaneHeight = 700f;
			}
			if (widgetRow.ButtonText("Trace medium", "Set the stack trace to be medium-sized on screen."))
			{
				detailsPaneHeight = 300f;
			}
			if (widgetRow.ButtonText("Trace small", "Set the stack trace to be small on screen."))
			{
				detailsPaneHeight = 100f;
			}
			if (canAutoOpen)
			{
				if (widgetRow.ButtonText("Auto-open is ON", ""))
				{
					canAutoOpen = false;
				}
			}
			else if (widgetRow.ButtonText("Auto-open is OFF", ""))
			{
				canAutoOpen = true;
			}
			if (widgetRow.ButtonText("Copy to clipboard", "Copy all messages to the clipboard."))
			{
				CopyAllMessagesToClipboard();
			}
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
			Widgets.BeginScrollView(listingRect, ref messagesScrollPosition, viewRect);
			float width = viewRect.width - 28f;
			Text.Font = GameFont.Tiny;
			float num = 0f;
			bool flag = false;
			foreach (LogMessage message in Log.Messages)
			{
				string text = message.text;
				if (text.Length > 1000)
				{
					text = text.Substring(0, 1000);
				}
				float num2 = Math.Min(30f, Text.CalcHeight(text, width));
				GUI.color = new Color(1f, 1f, 1f, 0.7f);
				Widgets.Label(new Rect(4f, num, 28f, num2), message.repeats.ToStringCached());
				Rect rect = new Rect(28f, num, width, num2);
				if (selectedMessage == message)
				{
					GUI.DrawTexture(rect, SelectedMessageTex);
				}
				else if (flag)
				{
					GUI.DrawTexture(rect, AltMessageTex);
				}
				if (Widgets.ButtonInvisible(rect))
				{
					ClearSelectedMessage();
					SelectedMessage = message;
				}
				GUI.color = message.Color;
				Widgets.Label(rect, text);
				num += num2;
				flag = !flag;
			}
			if (Event.current.type == EventType.Layout)
			{
				listingViewHeight = num;
			}
			Widgets.EndScrollView();
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
					Widgets.DrawHighlight(rect);
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
				if (text.Length > 15000)
				{
					Widgets.LabelScrollable(rect2, text, ref detailsScrollPosition, dontConsumeScrollEventsIfNoScrollbar: false, takeScrollbarSpaceEvenIfNoScrollbar: true, longLabel: true);
				}
				else
				{
					Widgets.TextAreaScrollable(rect2, text, ref detailsScrollPosition, readOnly: true);
				}
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
}
