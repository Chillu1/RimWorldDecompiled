using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_OperateScanner : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ScannerDef);

		public ThingDef ScannerDef => def.scannerDef;

		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsOfDef(ScannerDef);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == pawn.Faction)
				{
					CompScanner compScanner = list[i].TryGetComp<CompScanner>();
					if (compScanner != null && compScanner.CanUseNow)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (building.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(building, 1, -1, null, forced))
			{
				return false;
			}
			if (!building.TryGetComp<CompScanner>().CanUseNow)
			{
				return false;
			}
			if (building.IsBurning())
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.OperateScanner, t, 1500, checkOverrideOnExpiry: true);
		}
	}
}
