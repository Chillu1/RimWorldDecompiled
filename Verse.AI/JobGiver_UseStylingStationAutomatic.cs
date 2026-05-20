using RimWorld;

namespace Verse.AI
{
	public class JobGiver_UseStylingStationAutomatic : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return null;
			}
			if (Find.TickManager.TicksGame < pawn.style.nextStyleChangeAttemptTick)
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.StylingStation), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x));
			if (thing == null)
			{
				pawn.style.ResetNextStyleChangeAttemptTick();
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.UseStylingStationAutomatic, thing);
		}
	}
}
