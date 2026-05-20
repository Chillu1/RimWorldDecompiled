using System.Threading;
using Verse;

public class FleckParallelizationInfo
{
	public int startIndex;

	public int endIndex;

	public object data;

	public DrawBatch drawBatch = new DrawBatch();

	public ManualResetEvent doneEvent = new ManualResetEvent(initialState: false);
}
