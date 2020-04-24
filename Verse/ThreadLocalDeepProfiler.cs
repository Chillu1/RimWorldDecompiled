using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Verse
{
	public class ThreadLocalDeepProfiler
	{
		private class Watcher
		{
			private string label;

			private Stopwatch watch;

			private List<Watcher> children;

			public double ElapsedMilliseconds => watch.Elapsed.TotalMilliseconds;

			public string Label => label;

			public Stopwatch Watch => watch;

			public List<Watcher> Children => children;

			public Watcher(string label)
			{
				this.label = label;
				watch = Stopwatch.StartNew();
				children = null;
			}

			public Watcher(string label, Stopwatch stopwatch)
			{
				this.label = label;
				watch = stopwatch;
				children = null;
			}

			public void AddChildResult(Watcher w)
			{
				if (children == null)
				{
					children = new List<Watcher>();
				}
				children.Add(w);
			}
		}

		private struct LabelTimeTuple
		{
			public string label;

			public double totalTime;

			public double selfTime;
		}

		private Stack<Watcher> watchers = new Stack<Watcher>();

		private static readonly string[] Prefixes;

		private const int MaxDepth = 50;

		static ThreadLocalDeepProfiler()
		{
			Prefixes = new string[50];
			for (int i = 0; i < 50; i++)
			{
				Prefixes[i] = "";
				for (int j = 0; j < i; j++)
				{
					Prefixes[i] += " -";
				}
			}
		}

		public void Start(string label = null)
		{
			if (Prefs.LogVerbose)
			{
				watchers.Push(new Watcher(label));
			}
		}

		public void End()
		{
			if (!Prefs.LogVerbose)
			{
				return;
			}
			if (watchers.Count == 0)
			{
				Log.Error("Ended deep profiling while not profiling.");
				return;
			}
			Watcher watcher = watchers.Pop();
			watcher.Watch.Stop();
			if (watchers.Count > 0)
			{
				watchers.Peek().AddChildResult(watcher);
			}
			else
			{
				Output(watcher);
			}
		}

		private void Output(Watcher root)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (UnityData.IsInMainThread)
			{
				stringBuilder.AppendLine("--- Main thread ---");
			}
			else
			{
				stringBuilder.AppendLine("--- Thread " + Thread.CurrentThread.ManagedThreadId + " ---");
			}
			List<Watcher> list = new List<Watcher>();
			list.Add(root);
			AppendStringRecursive(stringBuilder, root.Label, root.Children, root.ElapsedMilliseconds, 0, list);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			HotspotAnalysis(stringBuilder, list);
			Log.Message(stringBuilder.ToString());
		}

		private void HotspotAnalysis(StringBuilder sb, List<Watcher> allWatchers)
		{
			List<LabelTimeTuple> list = new List<LabelTimeTuple>();
			foreach (IGrouping<string, Watcher> item in from w in allWatchers
				group w by w.Label)
			{
				double num = 0.0;
				double num2 = 0.0;
				int num3 = 0;
				foreach (Watcher item2 in item)
				{
					num3++;
					num += item2.ElapsedMilliseconds;
					if (item2.Children != null)
					{
						foreach (Watcher child in item2.Children)
						{
							num2 += child.ElapsedMilliseconds;
						}
					}
				}
				list.Add(new LabelTimeTuple
				{
					label = num3 + "x " + item.Key,
					totalTime = num,
					selfTime = num - num2
				});
			}
			sb.AppendLine("Hotspot analysis");
			sb.AppendLine("----------------------------------------");
			foreach (LabelTimeTuple item3 in list.OrderByDescending((LabelTimeTuple e) => e.selfTime))
			{
				string[] obj = new string[6]
				{
					item3.label,
					" -> ",
					null,
					null,
					null,
					null
				};
				double selfTime = item3.selfTime;
				obj[2] = selfTime.ToString("0.0000");
				obj[3] = " ms (total (w/children): ";
				selfTime = item3.totalTime;
				obj[4] = selfTime.ToString("0.0000");
				obj[5] = " ms)";
				sb.AppendLine(string.Concat(obj));
			}
		}

		private void AppendStringRecursive(StringBuilder sb, string label, List<Watcher> children, double elapsedTime, int depth, List<Watcher> allWatchers)
		{
			if (children != null)
			{
				double num = elapsedTime;
				foreach (Watcher child in children)
				{
					num -= child.ElapsedMilliseconds;
				}
				sb.AppendLine(Prefixes[depth] + " " + elapsedTime.ToString("0.0000") + "ms (self: " + num.ToString("0.0000") + " ms) " + label);
			}
			else
			{
				sb.AppendLine(Prefixes[depth] + " " + elapsedTime.ToString("0.0000") + "ms " + label);
			}
			if (children != null)
			{
				allWatchers.AddRange(children);
				foreach (IGrouping<string, Watcher> item in from c in children
					group c by c.Label)
				{
					List<Watcher> list = new List<Watcher>();
					double num2 = 0.0;
					int num3 = 0;
					foreach (Watcher item2 in item)
					{
						if (item2.Children != null)
						{
							foreach (Watcher child2 in item2.Children)
							{
								list.Add(child2);
							}
						}
						num2 += item2.ElapsedMilliseconds;
						num3++;
					}
					if (num3 <= 1)
					{
						AppendStringRecursive(sb, item.Key, list, num2, depth + 1, allWatchers);
					}
					else
					{
						AppendStringRecursive(sb, num3 + "x " + item.Key, list, num2, depth + 1, allWatchers);
					}
				}
			}
		}
	}
}
