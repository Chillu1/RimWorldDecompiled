using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verse
{
	public static class LongEventHandler
	{
		private class QueuedLongEvent
		{
			public Action eventAction;

			public IEnumerator eventActionEnumerator;

			public string levelToLoad;

			public string eventTextKey = "";

			public string eventText = "";

			public bool doAsynchronously;

			public Action<Exception> exceptionHandler;

			public bool alreadyDisplayed;

			public bool canEverUseStandardWindow = true;

			public bool showExtraUIInfo = true;

			public bool UseAnimatedDots
			{
				get
				{
					if (!doAsynchronously)
					{
						return eventActionEnumerator != null;
					}
					return true;
				}
			}

			public bool ShouldWaitUntilDisplayed
			{
				get
				{
					if (!alreadyDisplayed && UseStandardWindow)
					{
						return !eventText.NullOrEmpty();
					}
					return false;
				}
			}

			public bool UseStandardWindow
			{
				get
				{
					if (canEverUseStandardWindow && !doAsynchronously)
					{
						return eventActionEnumerator == null;
					}
					return false;
				}
			}
		}

		private static Queue<QueuedLongEvent> eventQueue = new Queue<QueuedLongEvent>();

		private static QueuedLongEvent currentEvent = null;

		private static Thread eventThread = null;

		private static AsyncOperation levelLoadOp = null;

		private static List<Action> toExecuteWhenFinished = new List<Action>();

		private static bool executingToExecuteWhenFinished = false;

		private static readonly object CurrentEventTextLock = new object();

		private static readonly Vector2 StatusRectSize = new Vector2(240f, 75f);

		public static bool ShouldWaitForEvent
		{
			get
			{
				if (!AnyEventNowOrWaiting)
				{
					return false;
				}
				if (currentEvent != null && !currentEvent.UseStandardWindow)
				{
					return true;
				}
				if (Find.UIRoot == null || Find.WindowStack == null)
				{
					return true;
				}
				return false;
			}
		}

		public static bool AnyEventNowOrWaiting
		{
			get
			{
				if (currentEvent == null)
				{
					return eventQueue.Count > 0;
				}
				return true;
			}
		}

		public static bool AnyEventWhichDoesntUseStandardWindowNowOrWaiting
		{
			get
			{
				QueuedLongEvent queuedLongEvent = currentEvent;
				if (queuedLongEvent != null && !queuedLongEvent.UseStandardWindow)
				{
					return true;
				}
				return eventQueue.Any((QueuedLongEvent x) => !x.UseStandardWindow);
			}
		}

		public static bool ForcePause => AnyEventNowOrWaiting;

		public static void QueueLongEvent(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler, bool showExtraUIInfo = true)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = action;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			queuedLongEvent.showExtraUIInfo = showExtraUIInfo;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(IEnumerable action, string textKey, Action<Exception> exceptionHandler = null, bool showExtraUIInfo = true)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventActionEnumerator = action.GetEnumerator();
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = false;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			queuedLongEvent.showExtraUIInfo = showExtraUIInfo;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(Action preLoadLevelAction, string levelToLoad, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler, bool showExtraUIInfo = true)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = preLoadLevelAction;
			queuedLongEvent.levelToLoad = levelToLoad;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			queuedLongEvent.canEverUseStandardWindow = !AnyEventWhichDoesntUseStandardWindowNowOrWaiting;
			queuedLongEvent.showExtraUIInfo = showExtraUIInfo;
			eventQueue.Enqueue(queuedLongEvent);
		}

		public static void ClearQueuedEvents()
		{
			eventQueue.Clear();
		}

		public static void LongEventsOnGUI()
		{
			if (currentEvent == null)
			{
				GameplayTipWindow.ResetTipTimer();
				return;
			}
			float num = StatusRectSize.x;
			lock (CurrentEventTextLock)
			{
				Text.Font = GameFont.Small;
				num = Mathf.Max(num, Text.CalcSize(currentEvent.eventText + "...").x + 40f);
			}
			bool flag = Find.UIRoot != null && !currentEvent.UseStandardWindow && currentEvent.showExtraUIInfo;
			bool flag2 = Find.UIRoot != null && Current.Game != null && !currentEvent.UseStandardWindow && currentEvent.showExtraUIInfo;
			Vector2 vector = flag2 ? ModSummaryWindow.GetEffectiveSize() : Vector2.zero;
			float num2 = StatusRectSize.y;
			if (flag2)
			{
				num2 += 17f + vector.y;
			}
			if (flag)
			{
				num2 += 17f + GameplayTipWindow.WindowSize.y;
			}
			float num3 = ((float)UI.screenHeight - num2) / 2f;
			Vector2 offset = new Vector2(((float)UI.screenWidth - GameplayTipWindow.WindowSize.x) / 2f, num3 + StatusRectSize.y + 17f);
			Vector2 offset2 = new Vector2(((float)UI.screenWidth - vector.x) / 2f, offset.y + GameplayTipWindow.WindowSize.y + 17f);
			Rect r = new Rect(((float)UI.screenWidth - num) / 2f, num3, num, StatusRectSize.y);
			r = r.Rounded();
			if (!currentEvent.UseStandardWindow || Find.UIRoot == null || Find.WindowStack == null)
			{
				if (UIMenuBackgroundManager.background == null)
				{
					UIMenuBackgroundManager.background = new UI_BackgroundMain();
				}
				UIMenuBackgroundManager.background.BackgroundOnGUI();
				Widgets.DrawShadowAround(r);
				Widgets.DrawWindowBackground(r);
				DrawLongEventWindowContents(r);
				if (flag)
				{
					GameplayTipWindow.DrawWindow(offset, useWindowStack: false);
				}
				if (flag2)
				{
					ModSummaryWindow.DrawWindow(offset2, useWindowStack: false);
					TooltipHandler.DoTooltipGUI();
				}
			}
			else
			{
				DrawLongEventWindow(r);
				if (flag)
				{
					GameplayTipWindow.DrawWindow(offset, useWindowStack: true);
				}
			}
		}

		private static void DrawLongEventWindow(Rect statusRect)
		{
			Find.WindowStack.ImmediateWindow(62893994, statusRect, WindowLayer.Super, delegate
			{
				DrawLongEventWindowContents(statusRect.AtZero());
			});
		}

		public static void LongEventsUpdate(out bool sceneChanged)
		{
			sceneChanged = false;
			if (currentEvent != null)
			{
				if (currentEvent.eventActionEnumerator != null)
				{
					UpdateCurrentEnumeratorEvent();
				}
				else if (currentEvent.doAsynchronously)
				{
					UpdateCurrentAsynchronousEvent();
				}
				else
				{
					UpdateCurrentSynchronousEvent(out sceneChanged);
				}
			}
			if (currentEvent == null && eventQueue.Count > 0)
			{
				currentEvent = eventQueue.Dequeue();
				if (currentEvent.eventTextKey == null)
				{
					currentEvent.eventText = "";
				}
				else
				{
					currentEvent.eventText = currentEvent.eventTextKey.Translate();
				}
			}
		}

		public static void ExecuteWhenFinished(Action action)
		{
			toExecuteWhenFinished.Add(action);
			if ((currentEvent == null || currentEvent.ShouldWaitUntilDisplayed) && !executingToExecuteWhenFinished)
			{
				ExecuteToExecuteWhenFinished();
			}
		}

		public static void SetCurrentEventText(string newText)
		{
			lock (CurrentEventTextLock)
			{
				if (currentEvent != null)
				{
					currentEvent.eventText = newText;
				}
			}
		}

		private static void UpdateCurrentEnumeratorEvent()
		{
			try
			{
				float num = Time.realtimeSinceStartup + 0.1f;
				while (currentEvent.eventActionEnumerator.MoveNext())
				{
					if (num <= Time.realtimeSinceStartup)
					{
						return;
					}
				}
				(currentEvent.eventActionEnumerator as IDisposable)?.Dispose();
				currentEvent = null;
				eventThread = null;
				levelLoadOp = null;
				ExecuteToExecuteWhenFinished();
			}
			catch (Exception ex)
			{
				Log.Error("Exception from long event: " + ex);
				if (currentEvent != null)
				{
					(currentEvent.eventActionEnumerator as IDisposable)?.Dispose();
					if (currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
				}
				currentEvent = null;
				eventThread = null;
				levelLoadOp = null;
			}
		}

		private static void UpdateCurrentAsynchronousEvent()
		{
			if (eventThread == null)
			{
				eventThread = new Thread((ThreadStart)delegate
				{
					RunEventFromAnotherThread(currentEvent.eventAction);
				});
				eventThread.Start();
			}
			else
			{
				if (eventThread.IsAlive)
				{
					return;
				}
				bool flag = false;
				if (!currentEvent.levelToLoad.NullOrEmpty())
				{
					if (levelLoadOp == null)
					{
						levelLoadOp = SceneManager.LoadSceneAsync(currentEvent.levelToLoad);
					}
					else if (levelLoadOp.isDone)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
					ExecuteToExecuteWhenFinished();
				}
			}
		}

		private static void UpdateCurrentSynchronousEvent(out bool sceneChanged)
		{
			sceneChanged = false;
			if (!currentEvent.ShouldWaitUntilDisplayed)
			{
				try
				{
					if (currentEvent.eventAction != null)
					{
						currentEvent.eventAction();
					}
					if (!currentEvent.levelToLoad.NullOrEmpty())
					{
						SceneManager.LoadScene(currentEvent.levelToLoad);
						sceneChanged = true;
					}
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
					ExecuteToExecuteWhenFinished();
				}
				catch (Exception ex)
				{
					Log.Error("Exception from long event: " + ex);
					if (currentEvent != null && currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
					currentEvent = null;
					eventThread = null;
					levelLoadOp = null;
				}
			}
		}

		private static void RunEventFromAnotherThread(Action action)
		{
			CultureInfoUtility.EnsureEnglish();
			try
			{
				action?.Invoke();
			}
			catch (Exception ex)
			{
				Log.Error("Exception from asynchronous event: " + ex);
				try
				{
					if (currentEvent != null && currentEvent.exceptionHandler != null)
					{
						currentEvent.exceptionHandler(ex);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Exception was thrown while trying to handle exception. Exception: " + arg);
				}
			}
		}

		private static void ExecuteToExecuteWhenFinished()
		{
			if (executingToExecuteWhenFinished)
			{
				Log.Warning("Already executing.");
				return;
			}
			executingToExecuteWhenFinished = true;
			if (toExecuteWhenFinished.Count > 0)
			{
				DeepProfiler.Start("ExecuteToExecuteWhenFinished()");
			}
			for (int i = 0; i < toExecuteWhenFinished.Count; i++)
			{
				DeepProfiler.Start(toExecuteWhenFinished[i].Method.DeclaringType.ToString() + " -> " + toExecuteWhenFinished[i].Method.ToString());
				try
				{
					toExecuteWhenFinished[i]();
				}
				catch (Exception arg)
				{
					Log.Error("Could not execute post-long-event action. Exception: " + arg);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			if (toExecuteWhenFinished.Count > 0)
			{
				DeepProfiler.End();
			}
			toExecuteWhenFinished.Clear();
			executingToExecuteWhenFinished = false;
		}

		private static void DrawLongEventWindowContents(Rect rect)
		{
			if (currentEvent == null)
			{
				return;
			}
			if (Event.current.type == EventType.Repaint)
			{
				currentEvent.alreadyDisplayed = true;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			float num = 0f;
			if (levelLoadOp != null)
			{
				float f = 1f;
				if (!levelLoadOp.isDone)
				{
					f = levelLoadOp.progress;
				}
				TaggedString taggedString = "LoadingAssets".Translate() + " " + f.ToStringPercent();
				num = Text.CalcSize(taggedString).x;
				Widgets.Label(rect, taggedString);
			}
			else
			{
				lock (CurrentEventTextLock)
				{
					num = Text.CalcSize(currentEvent.eventText).x;
					Widgets.Label(rect, currentEvent.eventText);
				}
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			rect.xMin = rect.center.x + num / 2f;
			Widgets.Label(rect, (!currentEvent.UseAnimatedDots) ? "..." : GenText.MarchingEllipsis());
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
