using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GiveSpeechFacingTarget : ThinkNode_JobGiver
{
	public SoundDef soundDefMale;

	public SoundDef soundDefFemale;

	public bool faceSpectatorsIfPossible;

	public bool showSpeechBubbles = true;

	protected override Job TryGiveJob(Pawn pawn)
	{
		PawnDuty duty = pawn.mindState.duty;
		if (duty == null)
		{
			return null;
		}
		IntVec3 result = pawn.Position;
		if (!pawn.CanReserve(pawn.Position))
		{
			CellFinder.TryRandomClosewalkCellNear(result, pawn.Map, 2, out result, (IntVec3 c) => pawn.CanReserveAndReach(c, PathEndMode.OnCell, pawn.NormalMaxDanger()));
		}
		Rot4? rot = pawn.mindState?.duty?.overrideFacing;
		IntVec3 intVec = ((rot.HasValue && rot.Value.IsValid) ? (result + rot.Value.FacingCell) : duty.spectateRect.CenterCell);
		LordJob_Ritual lordJob_Ritual = pawn.GetLord()?.LordJob as LordJob_Ritual;
		Job job = JobMaker.MakeJob(JobDefOf.GiveSpeech, result, intVec);
		job.showSpeechBubbles = showSpeechBubbles;
		if (lordJob_Ritual != null && lordJob_Ritual.lord.CurLordToil is LordToil_Ritual lordToil_Ritual)
		{
			job.interaction = lordToil_Ritual.stage.BehaviorForRole(lordJob_Ritual.RoleFor(pawn).id).speakerInteraction;
		}
		job.speechSoundMale = soundDefMale ?? SoundDefOf.Speech_Leader_Male;
		job.speechSoundFemale = soundDefFemale ?? SoundDefOf.Speech_Leader_Female;
		job.speechFaceSpectatorsIfPossible = faceSpectatorsIfPossible;
		return job;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_GiveSpeechFacingTarget obj = (JobGiver_GiveSpeechFacingTarget)base.DeepCopy(resolve);
		obj.soundDefMale = soundDefMale;
		obj.soundDefFemale = soundDefFemale;
		obj.showSpeechBubbles = showSpeechBubbles;
		obj.faceSpectatorsIfPossible = faceSpectatorsIfPossible;
		return obj;
	}
}
