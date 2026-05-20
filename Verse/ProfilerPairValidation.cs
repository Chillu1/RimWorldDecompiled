using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Verse;

public static class ProfilerPairValidation
{
	public static Stack<StackTrace> profilerSignatures = new Stack<StackTrace>();

	public static void BeginSample(string token)
	{
		profilerSignatures.Push(new StackTrace(1, fNeedFileInfo: true));
	}

	public static void EndSample()
	{
		StackTrace stackTrace = profilerSignatures.Pop();
		StackTrace stackTrace2 = new StackTrace(1, fNeedFileInfo: true);
		if (stackTrace2.FrameCount != stackTrace.FrameCount)
		{
			Log.Message($"Mismatch:\n{stackTrace}\n\n{stackTrace2}");
			return;
		}
		for (int i = 0; i < stackTrace2.FrameCount; i++)
		{
			if (stackTrace2.GetFrame(i).GetMethod() != stackTrace.GetFrame(i).GetMethod() && (!(stackTrace.GetFrame(i).GetMethod().DeclaringType == typeof(ProfilerThreadCheck)) || !(stackTrace2.GetFrame(i).GetMethod().DeclaringType == typeof(ProfilerThreadCheck))) && (!(stackTrace.GetFrame(i).GetMethod() == typeof(PathFinder).GetMethod("PfProfilerBeginSample", BindingFlags.Instance | BindingFlags.NonPublic)) || !(stackTrace2.GetFrame(i).GetMethod() == typeof(PathFinder).GetMethod("PfProfilerEndSample", BindingFlags.Instance | BindingFlags.NonPublic))))
			{
				Log.Message($"Mismatch:\n{stackTrace}\n\n{stackTrace2}");
				break;
			}
		}
	}
}
