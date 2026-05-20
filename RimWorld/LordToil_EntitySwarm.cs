using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public abstract class LordToil_EntitySwarm : LordToil
{
	private PawnPath path;

	private const int SwarmRadius = 7;

	private const int MoveInterval = 600;

	protected LordToilData_EntitySwarm Data => (LordToilData_EntitySwarm)data;

	protected abstract DutyDef GetDutyDef();

	public LordToil_EntitySwarm(IntVec3 start, IntVec3 dest)
	{
		data = new LordToilData_EntitySwarm
		{
			pos = start,
			dest = dest,
			lastMoved = GenTicks.TicksGame
		};
	}

	private void GetPath()
	{
		path = lord.Map.pathFinder.FindPathNow(Data.pos, Data.dest, TraverseParms.For(TraverseMode.NoPassClosedDoors));
	}

	public override void Init()
	{
		GetPath();
		for (int i = 0; i < 7; i++)
		{
			Data.pos = path.ConsumeNextNode();
		}
	}

	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			lord.ownedPawns[i].mindState.duty = new PawnDuty(GetDutyDef());
		}
	}

	public override void LordToilTick()
	{
		if (path == null)
		{
			GetPath();
		}
		if (path.NodesLeftCount <= 1 || !path.Found || lord.ownedPawns.Count == 0)
		{
			lord.ReceiveMemo("TravelArrived");
			return;
		}
		if (Find.TickManager.TicksGame > Data.lastMoved + 600)
		{
			Data.pos = path.ConsumeNextNode();
			Data.lastMoved = Find.TickManager.TicksGame;
		}
		int index = Find.TickManager.TicksGame % lord.ownedPawns.Count;
		Pawn pawn = lord.ownedPawns[index];
		if (!pawn.Position.InHorDistOf(Data.pos, 7f) && !pawn.pather.Moving)
		{
			CellFinder.TryFindRandomReachableCellNearPosition(pawn.Position, Data.pos, lord.Map, 7f, TraverseParms.For(pawn), (IntVec3 x) => x.Standable(lord.Map), null, out var result);
			if (!result.IsValid)
			{
				lord.RemovePawn(pawn);
			}
			pawn.mindState.duty = new PawnDuty(GetDutyDef(), result);
		}
		base.LordToilTick();
	}

	public override void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
	{
		lord.Notify_PawnLost(detector, PawnLostCondition.LeftVoluntarily);
	}
}
