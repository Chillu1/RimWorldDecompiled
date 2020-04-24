using System.Collections.Generic;

namespace Verse.Sound
{
	public abstract class AudioGrain
	{
		public abstract IEnumerable<ResolvedGrain> GetResolvedGrains();
	}
}
