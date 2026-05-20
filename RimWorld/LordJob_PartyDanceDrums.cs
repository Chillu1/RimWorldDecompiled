using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class LordJob_PartyDanceDrums : LordJob_Ritual
	{
		public LordJob_PartyDanceDrums()
		{
		}

		public LordJob_PartyDanceDrums(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
			: base(selectedTarget, ritual, obligation, allStages, assignments, organizer)
		{
		}

		protected override LordToil_Ritual MakeToil(RitualStage stage)
		{
			if (!ModLister.CheckIdeology("Drum party toil"))
			{
				return null;
			}
			return new LordToil_PartyDanceDrums(spot, this, stage, organizer);
		}
	}
}
