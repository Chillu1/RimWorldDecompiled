using System.Collections.Generic;

namespace Verse.Sound
{
	public class SoundSizeAggregator
	{
		private List<ISizeReporter> reporters = new List<ISizeReporter>();

		private float testSize;

		public float AggregateSize
		{
			get
			{
				if (reporters.Count == 0)
				{
					return testSize;
				}
				float num = 0f;
				foreach (ISizeReporter reporter in reporters)
				{
					num += reporter.CurrentSize();
				}
				return num;
			}
		}

		public SoundSizeAggregator()
		{
			testSize = Rand.Value * 3f;
			testSize *= testSize;
		}

		public void RegisterReporter(ISizeReporter newRep)
		{
			reporters.Add(newRep);
		}

		public void RemoveReporter(ISizeReporter oldRep)
		{
			reporters.Remove(oldRep);
		}
	}
}
