using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class LordJob_Ritual_Mutilation : LordJob_Ritual
	{
		public List<Pawn> mutilatedPawns = new List<Pawn>();

		public LordJob_Ritual_Mutilation()
		{
		}

		public LordJob_Ritual_Mutilation(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
			: base(selectedTarget, ritual, obligation, allStages, assignments, organizer)
		{
		}

		protected override bool ShouldCallOffBecausePawnNoLongerOwned(Pawn p)
		{
			if (base.ShouldCallOffBecausePawnNoLongerOwned(p))
			{
				return !mutilatedPawns.Contains(p);
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref mutilatedPawns, "mutilatedPawns", LookMode.Reference);
		}
	}
}
