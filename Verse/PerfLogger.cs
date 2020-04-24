using System.Diagnostics;
using System.Text;

namespace Verse
{
	public static class PerfLogger
	{
		public static StringBuilder currentLog = new StringBuilder();

		private static long start;

		private static long current;

		private static int indent;

		public static void Reset()
		{
			currentLog = null;
			start = Stopwatch.GetTimestamp();
			current = start;
		}

		public static void Flush()
		{
			Log.Message((currentLog != null) ? currentLog.ToString() : "");
			Reset();
		}

		public static void Record(string label)
		{
			long timestamp = Stopwatch.GetTimestamp();
			if (currentLog == null)
			{
				currentLog = new StringBuilder();
			}
			currentLog.AppendLine(string.Format("{0}: {3}{1} ({2})", (timestamp - start) * 1000 / Stopwatch.Frequency, label, (timestamp - current) * 1000 / Stopwatch.Frequency, new string(' ', indent * 2)));
			current = timestamp;
		}

		public static void Indent()
		{
			indent++;
		}

		public static void Outdent()
		{
			indent--;
		}

		public static float Duration()
		{
			return (float)(Stopwatch.GetTimestamp() - start) / (float)Stopwatch.Frequency;
		}
	}
}
