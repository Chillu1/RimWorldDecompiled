using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AcceptRole : ThinkNode_JobGiver
	{
		public SoundDef soundDefMale;

		public SoundDef soundDefFemale;

		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.AcceptRole, pawn.Position, duty.spectateRect.CenterCell + new IntVec3(0, 0, -1));
			job.speechSoundMale = soundDefMale ?? SoundDefOf.Speech_Leader_Male;
			job.speechSoundFemale = soundDefFemale ?? SoundDefOf.Speech_Leader_Female;
			job.speechFaceSpectatorsIfPossible = true;
			return job;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AcceptRole obj = (JobGiver_AcceptRole)base.DeepCopy(resolve);
			obj.soundDefMale = soundDefMale;
			obj.soundDefFemale = soundDefFemale;
			return obj;
		}
	}
}
