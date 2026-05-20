using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RitualBehaviorWorker_Mutilation : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_Mutilation()
		{
		}

		public RitualBehaviorWorker_Mutilation(RitualBehaviorDef def)
			: base(def)
		{
		}

		protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
		{
			return new LordJob_Ritual_Mutilation(target, ritual, obligation, def.stages, assignments, organizer);
		}
	}
}
