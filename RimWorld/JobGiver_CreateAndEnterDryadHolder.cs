using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobGiver_CreateAndEnterDryadHolder : ThinkNode_JobGiver
{
	public const int SquareRadius = 4;

	public abstract JobDef JobDef { get; }

	public virtual bool ExtraValidator(Pawn pawn, CompTreeConnection connectionComp)
	{
		return false;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return null;
		}
		if (pawn.connections == null || pawn.connections.ConnectedThings.NullOrEmpty())
		{
			return null;
		}
		foreach (Thing connectedThing in pawn.connections.ConnectedThings)
		{
			CompTreeConnection compTreeConnection = connectedThing.TryGetComp<CompTreeConnection>();
			if (compTreeConnection != null && ExtraValidator(pawn, compTreeConnection) && pawn.CanReach(connectedThing, PathEndMode.Touch, Danger.Deadly) && CellFinder.TryFindRandomCellNear(connectedThing.Position, pawn.Map, 4, (IntVec3 c) => GauranlenUtility.CocoonAndPodCellValidator(c, pawn.Map), out var _))
			{
				return JobMaker.MakeJob(JobDef, connectedThing);
			}
		}
		return null;
	}
}
