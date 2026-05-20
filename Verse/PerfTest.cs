using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse;

public static class PerfTest
{
	public static string TestStandardMilliseconds()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		List<int> list = new List<int>();
		for (int i = 0; i < 100000; i++)
		{
			list.Add(i);
		}
		List<double> list2 = new List<double>();
		while (list.Count > 0)
		{
			int num = list[0];
			list.RemoveAt(0);
			list2.Add(Math.Sqrt(num));
		}
		double num2 = 0.0;
		for (int j = 0; j < list2.Count; j++)
		{
			num2 += list2[j];
		}
		stopwatch.Stop();
		return "Elapsed: " + stopwatch.Elapsed.ToString() + "\nMilliseconds: " + stopwatch.ElapsedMilliseconds + "\nSum: " + num2;
	}
}
