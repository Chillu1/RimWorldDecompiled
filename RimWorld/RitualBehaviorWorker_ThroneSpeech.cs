using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RitualBehaviorWorker_ThroneSpeech : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_ThroneSpeech()
		{
		}

		public RitualBehaviorWorker_ThroneSpeech(RitualBehaviorDef def)
			: base(def)
		{
		}

		protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
		{
			return new LordJob_Joinable_Speech(target, organizer, ritual, def.stages, assignments, titleSpeech: true);
		}
	}
}
