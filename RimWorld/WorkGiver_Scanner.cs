using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Scanner : WorkGiver
	{
		public virtual ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);

		public virtual int MaxRegionsToScanBeforeGlobalSearch => -1;

		public virtual bool Prioritized => false;

		public virtual bool AllowUnreachable => false;

		public virtual PathEndMode PathEndMode => PathEndMode.Touch;

		public virtual IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			yield break;
		}

		public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return null;
		}

		public virtual Danger MaxPathDanger(Pawn pawn)
		{
			return pawn.NormalMaxDanger();
		}

		public virtual bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobOnThing(pawn, t, forced) != null;
		}

		public virtual Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return null;
		}

		public virtual bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return JobOnCell(pawn, c, forced) != null;
		}

		public virtual Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			return null;
		}

		public virtual float GetPriority(Pawn pawn, TargetInfo t)
		{
			return 0f;
		}

		public virtual string PostProcessedGerund(Job job)
		{
			return def.gerund;
		}

		public float GetPriority(Pawn pawn, IntVec3 cell)
		{
			return GetPriority(pawn, new TargetInfo(cell, pawn.Map));
		}
	}
}
