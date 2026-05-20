using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_GiveSpeech : ThinkNode_JobGiver
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
		if (!(duty.focusSecond.Thing is Building_Throne building_Throne) || building_Throne.AssignedPawn != pawn)
		{
			return null;
		}
		if (!pawn.CanReach(building_Throne, PathEndMode.InteractionCell, Danger.None))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.GiveSpeech, duty.focusSecond);
		job.speechSoundMale = soundDefMale ?? SoundDefOf.Speech_Leader_Male;
		job.speechSoundFemale = soundDefFemale ?? SoundDefOf.Speech_Leader_Female;
		return job;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_GiveSpeech obj = (JobGiver_GiveSpeech)base.DeepCopy(resolve);
		obj.soundDefMale = soundDefMale;
		obj.soundDefFemale = soundDefFemale;
		return obj;
	}
}
