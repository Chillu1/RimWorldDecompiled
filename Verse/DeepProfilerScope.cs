using System;

namespace Verse
{
	public class DeepProfilerScope : IDisposable
	{
		public DeepProfilerScope(string label = null)
		{
			DeepProfiler.Start(label);
		}

		void IDisposable.Dispose()
		{
			DeepProfiler.End();
		}
	}
}
