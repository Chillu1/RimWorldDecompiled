using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptComp_BedThought : PreceptComp_Thought
	{
		public class FacilityData
		{
			public ThingDef def;
		}

		public FacilityData requireFacility;

		public override void Notify_AddBedThoughts(Pawn pawn, Precept precept)
		{
			base.Notify_AddBedThoughts(pawn, precept);
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
			if (requireFacility == null || HasActiveLinkedFacility(pawn))
			{
				Thought_Memory newThought = ThoughtMaker.MakeThought(thought, precept);
				pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
			}
		}

		private bool HasActiveLinkedFacility(Pawn pawn)
		{
			CompAffectedByFacilities compAffectedByFacilities = pawn.CurrentBed()?.TryGetComp<CompAffectedByFacilities>();
			if (compAffectedByFacilities == null)
			{
				return false;
			}
			foreach (Thing item in compAffectedByFacilities.LinkedFacilitiesListForReading)
			{
				if (item.def == requireFacility.def && compAffectedByFacilities.IsFacilityActive(item))
				{
					return true;
				}
			}
			return false;
		}

		public override IEnumerable<string> ConfigErrors(PreceptDef parent)
		{
			foreach (string item in base.ConfigErrors(parent))
			{
				yield return item;
			}
			if (requireFacility != null && requireFacility.def == null)
			{
				yield return "<requireFacility> has null <def>";
			}
		}
	}
}
