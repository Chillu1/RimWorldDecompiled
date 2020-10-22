using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class ThoughtWorker_MusicalInstrumentListeningBase : ThoughtWorker
	{
		protected abstract ThingDef InstrumentDef
		{
			get;
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
			{
				return false;
			}
			ThingDef def = InstrumentDef;
			return GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(def), PathEndMode.ClosestTouch, TraverseParms.For(p), def.building.instrumentRange, delegate(Thing thing)
			{
				Building_MusicalInstrument building_MusicalInstrument = thing as Building_MusicalInstrument;
				return building_MusicalInstrument != null && building_MusicalInstrument.IsBeingPlayed && Building_MusicalInstrument.IsAffectedByInstrument(def, building_MusicalInstrument.Position, p.Position, p.Map);
			}) != null;
		}
	}
}
