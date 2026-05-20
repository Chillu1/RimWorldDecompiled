using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MissingTongue : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			List<BodyPartRecord> partsWithTag = p.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.Tongue);
			if (!partsWithTag.Any())
			{
				return ThoughtState.Inactive;
			}
			foreach (BodyPartRecord item in partsWithTag)
			{
				if (!p.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(item) && p.health.hediffSet.PartIsMissing(item))
				{
					return true;
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
