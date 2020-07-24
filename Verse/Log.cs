using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class Log
	{
		private static LogMessageQueue messageQueue = new LogMessageQueue();

		private static HashSet<int> usedKeys = new HashSet<int>();

		public static bool openOnMessage = false;

		private static bool currentlyLoggingError;

		private static int messageCount;

		private const int StopLoggingAtMessageCount = 1000;

		public static IEnumerable<LogMessage> Messages => messageQueue.Messages;

		private static bool ReachedMaxMessagesLimit => messageCount >= 1000;

		public static void ResetMessageCount()
		{
			bool reachedMaxMessagesLimit = ReachedMaxMessagesLimit;
			messageCount = 0;
			if (reachedMaxMessagesLimit)
			{
				Message("Message logging is now once again on.");
			}
		}

		public static void Message(string text, bool ignoreStopLoggingLimit = false)
		{
			if (ignoreStopLoggingLimit || !ReachedMaxMessagesLimit)
			{
				Debug.Log(text);
				messageQueue.Enqueue(new LogMessage(LogMessageType.Message, text, StackTraceUtility.ExtractStackTrace()));
				PostMessage();
			}
		}

		public static void Warning(string text, bool ignoreStopLoggingLimit = false)
		{
			if (ignoreStopLoggingLimit || !ReachedMaxMessagesLimit)
			{
				Debug.LogWarning(text);
				messageQueue.Enqueue(new LogMessage(LogMessageType.Warning, text, StackTraceUtility.ExtractStackTrace()));
				PostMessage();
			}
		}

		public static void Error(string text, bool ignoreStopLoggingLimit = false)
		{
			if (!ignoreStopLoggingLimit && ReachedMaxMessagesLimit)
			{
				return;
			}
			Debug.LogError(text);
			if (currentlyLoggingError)
			{
				return;
			}
			currentlyLoggingError = true;
			try
			{
				if (Prefs.PauseOnError && Current.ProgramState == ProgramState.Playing)
				{
					Find.TickManager.Pause();
				}
				messageQueue.Enqueue(new LogMessage(LogMessageType.Error, text, StackTraceUtility.ExtractStackTrace()));
				PostMessage();
				if (!PlayDataLoader.Loaded || Prefs.DevMode)
				{
					TryOpenLogWindow();
				}
			}
			catch (Exception arg)
			{
				Debug.LogError("An error occurred while logging an error: " + arg);
			}
			finally
			{
				currentlyLoggingError = false;
			}
		}

		public static void ErrorOnce(string text, int key, bool ignoreStopLoggingLimit = false)
		{
			if ((ignoreStopLoggingLimit || !ReachedMaxMessagesLimit) && !usedKeys.Contains(key))
			{
				usedKeys.Add(key);
				Error(text, ignoreStopLoggingLimit);
			}
		}

		public static void Clear()
		{
			EditWindow_Log.ClearSelectedMessage();
			messageQueue.Clear();
			ResetMessageCount();
		}

		public static void TryOpenLogWindow()
		{
			if (StaticConstructorOnStartupUtility.coreStaticAssetsLoaded || UnityData.IsInMainThread)
			{
				EditWindow_Log.TryAutoOpen();
			}
		}

		private static void PostMessage()
		{
			if (openOnMessage)
			{
				TryOpenLogWindow();
				EditWindow_Log.SelectLastMessage(expandDetailsPane: true);
			}
			messageCount++;
			if (messageCount == 1000 && ReachedMaxMessagesLimit)
			{
				Warning("Reached max messages limit. Stopping logging to avoid spam.", ignoreStopLoggingLimit: true);
			}
		}
	}
}
