using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public abstract class ConditionalStatAffecter
	{
		public List<StatModifier> statFactors;

		public List<StatModifier> statOffsets;

		public abstract string Label { get; }

		public abstract bool Applies(StatRequest req);
	}
}
