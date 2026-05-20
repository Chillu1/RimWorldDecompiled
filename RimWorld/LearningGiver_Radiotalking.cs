using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Radiotalking : LearningGiver
{
	public override bool CanGiveDesire => ResearchProjectDefOf.MicroelectronicsBasics.IsFinished;

	private bool TryFindCommsConsole(Pawn pawn, out Thing commsConsole)
	{
		commsConsole = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.CommsConsole), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, (Thing t) => t is Building_CommsConsole { CanUseCommsNow: not false } building_CommsConsole && pawn.CanReserve(building_CommsConsole) && !building_CommsConsole.IsForbidden(pawn));
		return commsConsole != null;
	}

	public override bool CanDo(Pawn pawn)
	{
		if (!base.CanDo(pawn))
		{
			return false;
		}
		Thing commsConsole;
		return TryFindCommsConsole(pawn, out commsConsole);
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!TryFindCommsConsole(pawn, out var commsConsole))
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, commsConsole);
	}
}
