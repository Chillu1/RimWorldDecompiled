using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptComp_SituationalThought : PreceptComp_Thought
	{
		public override IEnumerable<TraitRequirement> TraitsAffecting => ThoughtUtility.GetNullifyingTraits(thought);
	}
}
