using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Verse.AI;

namespace Verse
{
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
				Log.Message($"Mismatch:\n{stackTrace.ToString()}\n\n{stackTrace2.ToString()}");
				return;
			}
			int num = 0;
			while (true)
			{
				if (num < stackTrace2.FrameCount)
				{
					if (stackTrace2.GetFrame(num).GetMethod() != stackTrace.GetFrame(num).GetMethod() && (!(stackTrace.GetFrame(num).GetMethod().DeclaringType == typeof(ProfilerThreadCheck)) || !(stackTrace2.GetFrame(num).GetMethod().DeclaringType == typeof(ProfilerThreadCheck))) && (!(stackTrace.GetFrame(num).GetMethod() == typeof(PathFinder).GetMethod("PfProfilerBeginSample", BindingFlags.Instance | BindingFlags.NonPublic)) || !(stackTrace2.GetFrame(num).GetMethod() == typeof(PathFinder).GetMethod("PfProfilerEndSample", BindingFlags.Instance | BindingFlags.NonPublic))))
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			Log.Message($"Mismatch:\n{stackTrace.ToString()}\n\n{stackTrace2.ToString()}");
		}
	}
}
