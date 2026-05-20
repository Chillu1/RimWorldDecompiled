using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DyeHair : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return null;
			}
			if (!pawn.style.nextHairColor.HasValue || pawn.style.nextHairColor == pawn.story.HairColor)
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.StylingStation), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x));
			if (thing == null)
			{
				return null;
			}
			Thing thing2 = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Dye), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, 1));
			if (thing2 == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.DyeHair, thing, thing2);
			job.count = 1;
			return job;
		}
	}
}
