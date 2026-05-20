using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse;

public class PriorityWork : IExposable
{
	private Pawn pawn;

	private IntVec3 prioritizedCell = IntVec3.Invalid;

	private WorkGiverDef prioritizedWorkGiver;

	private int prioritizeTick = Find.TickManager.TicksGame;

	private const int Timeout = 30000;

	public bool IsPrioritized
	{
		get
		{
			if (prioritizedCell.IsValid)
			{
				if (Find.TickManager.TicksGame < prioritizeTick + 30000)
				{
					return true;
				}
				Clear();
			}
			return false;
		}
	}

	public IntVec3 Cell => prioritizedCell;

	public WorkGiverDef WorkGiver => prioritizedWorkGiver;

	public PriorityWork()
	{
	}

	public PriorityWork(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref prioritizedCell, "prioritizedCell");
		Scribe_Defs.Look(ref prioritizedWorkGiver, "prioritizedWorkGiver");
		Scribe_Values.Look(ref prioritizeTick, "prioritizeTick", 0);
	}

	public void Set(IntVec3 prioritizedCell, WorkGiverDef prioritizedWorkGiver)
	{
		this.prioritizedCell = prioritizedCell;
		this.prioritizedWorkGiver = prioritizedWorkGiver;
		prioritizeTick = Find.TickManager.TicksGame;
	}

	public void Clear()
	{
		prioritizedCell = IntVec3.Invalid;
		prioritizedWorkGiver = null;
		prioritizeTick = 0;
	}

	public void ClearPrioritizedWorkAndJobQueue()
	{
		Clear();
		pawn.jobs.ClearQueuedJobs();
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if ((!IsPrioritized && (pawn.CurJob == null || !pawn.CurJob.playerForced || !pawn.jobs.IsCurrentJobPlayerInterruptible()) && !pawn.jobs.jobQueue.AnyPlayerForced) || pawn.Drafted || pawn.Deathresting)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandClearPrioritizedWork".Translate();
		command_Action.defaultDesc = "CommandClearPrioritizedWorkDesc".Translate();
		command_Action.icon = TexCommand.ClearPrioritizedWork;
		command_Action.activateSound = SoundDefOf.Tick_Low;
		command_Action.action = delegate
		{
			ClearPrioritizedWorkAndJobQueue();
			if (pawn.CurJob.playerForced && pawn.jobs.IsCurrentJobPlayerInterruptible())
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		};
		command_Action.hotKey = KeyBindingDefOf.Designator_Cancel;
		command_Action.groupKeyIgnoreContent = 6165612;
		yield return command_Action;
	}
}
