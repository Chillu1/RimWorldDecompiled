using System.Collections.Generic;

namespace Verse.AI.Group;

public class JobSyncTracker : IExposable
{
	public JobCondition allowedTerminalStates;

	public string dutyTag;

	public string thinkNodeTag;

	private int initialCount;

	private HashSet<Pawn> pawns;

	private ILoadReferenceable source;

	public bool AllPawnsDone => pawns.Count == 0;

	public bool AnyPawnsDone => initialCount != pawns.Count;

	public int PawnDoneCount => initialCount - pawns.Count;

	public IEnumerable<Pawn> BlockingPawns => pawns;

	public bool WaitingOnPawn(Pawn pawn)
	{
		return pawns.Contains(pawn);
	}

	protected JobSyncTracker()
	{
	}

	public JobSyncTracker(IEnumerable<Pawn> pawns, ILoadReferenceable source, string dutyTag = null, string thinkNodeTag = null, JobCondition allowedTerminalStates = JobCondition.Succeeded)
	{
		this.pawns = new HashSet<Pawn>(pawns);
		this.allowedTerminalStates = allowedTerminalStates;
		this.dutyTag = dutyTag;
		initialCount = this.pawns.Count;
		this.source = source;
		this.thinkNodeTag = thinkNodeTag;
	}

	public void ForcePawnFinished(Pawn pawn)
	{
		pawns.Remove(pawn);
	}

	public void Notify_PawnJobDone(Pawn pawn, string dutyTag, string thinkNodeTag, ILoadReferenceable source, JobCondition condition)
	{
		if (pawns.Contains(pawn) && (allowedTerminalStates & condition) != JobCondition.None && (this.dutyTag == null || !(this.dutyTag != dutyTag)) && (this.source == null || this.source == source) && (this.thinkNodeTag == null || !(this.thinkNodeTag != thinkNodeTag)))
		{
			pawns.Remove(pawn);
		}
	}

	public void Notify_PawnJobDone(Pawn pawn, Job job, JobCondition condition)
	{
		Notify_PawnJobDone(pawn, job.dutyTag, job.jobGiver?.tag, job.source, condition);
	}

	public void Notify_PawnLost(Pawn pawn)
	{
		if (pawns.Remove(pawn))
		{
			initialCount--;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref allowedTerminalStates, "allowedTerminalStates", JobCondition.None);
		Scribe_Values.Look(ref dutyTag, "dutyTag");
		Scribe_Values.Look(ref initialCount, "initialCount", 0);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref source, "source");
	}
}
