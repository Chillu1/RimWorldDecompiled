using System;
using System.Collections.Generic;
using System.Threading;
using LudeonTK;
using UnityEngine;

namespace Verse;

public static class Log
{
	public class LogLock : IDisposable
	{
		public LogLock()
		{
			logDisablers = Interlocked.Increment(ref logDisablers);
		}

		public void Dispose()
		{
			logDisablers = Interlocked.Decrement(ref logDisablers);
		}
	}

	private static LogMessageQueue messageQueue = new LogMessageQueue();

	private static HashSet<int> usedKeys = new HashSet<int>();

	public static bool openOnMessage = false;

	private static bool currentlyLoggingError;

	private static int messageCount;

	private static bool reachedMaxMessagesLimit;

	private static object logLock = new object();

	private static int logDisablers;

	private const int StopLoggingAtMessageCount = 10000;

	public static IEnumerable<LogMessage> Messages => messageQueue.Messages;

	private static bool PreventLogging
	{
		get
		{
			if (!reachedMaxMessagesLimit)
			{
				return logDisablers > 0;
			}
			return true;
		}
	}

	public static LogLock LockMessages()
	{
		return new LogLock();
	}

	public static void ResetMessageCount()
	{
		lock (logLock)
		{
			messageCount = 0;
			if (reachedMaxMessagesLimit)
			{
				Debug.unityLogger.logEnabled = true;
				reachedMaxMessagesLimit = false;
				Message("Message logging is now once again on.");
			}
		}
	}

	public static void Message(string text)
	{
		lock (logLock)
		{
			if (!PreventLogging)
			{
				messageQueue.Enqueue(new LogMessage(LogMessageType.Message, text, StackTraceUtility.ExtractStackTrace()), out var repeatsCapped);
				if (!repeatsCapped)
				{
					Debug.Log(text);
					PostMessage();
				}
			}
		}
	}

	public static void Message(object obj)
	{
		Message(obj.ToString());
	}

	public static void Warning(string text)
	{
		lock (logLock)
		{
			if (PreventLogging)
			{
				return;
			}
			messageQueue.Enqueue(new LogMessage(LogMessageType.Warning, text, StackTraceUtility.ExtractStackTrace()), out var repeatsCapped);
			if (!repeatsCapped)
			{
				Debug.LogWarning(text);
				PostMessage();
				if (Prefs.OpenLogOnWarnings)
				{
					TryOpenLogWindow();
				}
			}
		}
	}

	public static void WarningOnce(string text, int key)
	{
		lock (logLock)
		{
			if (!PreventLogging && usedKeys.Add(key))
			{
				Warning(text);
			}
		}
	}

	public static void Error(string text)
	{
		lock (logLock)
		{
			if (PreventLogging)
			{
				return;
			}
			if (!currentlyLoggingError)
			{
				currentlyLoggingError = true;
				try
				{
					if (DebugSettings.pauseOnError && Current.ProgramState == ProgramState.Playing)
					{
						Find.TickManager.Pause();
					}
					messageQueue.Enqueue(new LogMessage(LogMessageType.Error, text, StackTraceUtility.ExtractStackTrace()), out var repeatsCapped);
					if (repeatsCapped)
					{
						return;
					}
					PostMessage();
					if (!PlayDataLoader.Loaded || Prefs.DevMode)
					{
						TryOpenLogWindow();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("An error occurred while logging an error: " + ex);
				}
				finally
				{
					currentlyLoggingError = false;
				}
			}
			Debug.LogError(text);
		}
	}

	public static void ErrorOnce(string text, int key)
	{
		lock (logLock)
		{
			if (!PreventLogging && usedKeys.Add(key))
			{
				Error(text);
			}
		}
	}

	public static void Clear()
	{
		lock (logLock)
		{
			EditWindow_Log.ClearSelectedMessage();
			messageQueue.Clear();
			ResetMessageCount();
		}
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
	}

	public static void Notify_MessageReceivedThreadedInternal(string msg, string stackTrace, LogType type)
	{
		if (++messageCount == 10000)
		{
			Warning("Reached max messages limit. Stopping logging to avoid spam.");
			reachedMaxMessagesLimit = true;
			Debug.unityLogger.logEnabled = false;
		}
	}
}
