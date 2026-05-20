using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class LordJob_Ritual_ChildBirth : LordJob_Ritual
	{
		public LordJob_Ritual_ChildBirth()
		{
		}

		public LordJob_Ritual_ChildBirth(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null, IntVec3? spotOverride = null)
			: base(selectedTarget, ritual, obligation, allStages, assignments, organizer, spotOverride)
		{
		}

		protected override bool RitualFinished(float progress, bool cancelled)
		{
			return progress == 1f;
		}
	}
}
