using System.Collections.Generic;
using Verse;

public static class FleckUtility
{
	private static List<FleckParallelizationInfo> parallelizationInfosPool = new List<FleckParallelizationInfo>();

	public static FleckParallelizationInfo GetParallelizationInfo()
	{
		if (parallelizationInfosPool.Count == 0)
		{
			return new FleckParallelizationInfo();
		}
		return parallelizationInfosPool.Pop();
	}

	public static void ReturnParallelizationInfo(FleckParallelizationInfo info)
	{
		info.doneEvent.Reset();
		info.drawBatch.Flush(draw: false);
		parallelizationInfosPool.Add(info);
	}
}
