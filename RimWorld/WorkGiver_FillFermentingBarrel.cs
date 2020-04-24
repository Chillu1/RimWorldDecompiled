using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FillFermentingBarrel : WorkGiver_Scanner
	{
		private static string TemperatureTrans;

		private static string NoWortTrans;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.FermentingBarrel);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public static void ResetStaticData()
		{
			TemperatureTrans = "BadTemperature".Translate().ToLower();
			NoWortTrans = "NoWort".Translate();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_FermentingBarrel building_FermentingBarrel = t as Building_FermentingBarrel;
			if (building_FermentingBarrel == null || building_FermentingBarrel.Fermented || building_FermentingBarrel.SpaceLeftForWort <= 0)
			{
				return false;
			}
			float ambientTemperature = building_FermentingBarrel.AmbientTemperature;
			CompProperties_TemperatureRuinable compProperties = building_FermentingBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
			if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
			{
				JobFailReason.Is(TemperatureTrans);
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (FindWort(pawn, building_FermentingBarrel) == null)
			{
				JobFailReason.Is(NoWortTrans);
				return false;
			}
			if (t.IsBurning())
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_FermentingBarrel barrel = (Building_FermentingBarrel)t;
			Thing t2 = FindWort(pawn, barrel);
			return JobMaker.MakeJob(JobDefOf.FillFermentingBarrel, t, t2);
		}

		private Thing FindWort(Pawn pawn, Building_FermentingBarrel barrel)
		{
			Predicate<Thing> validator = (Thing x) => (!x.IsForbidden(pawn) && pawn.CanReserve(x)) ? true : false;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Wort), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
		}
	}
}
