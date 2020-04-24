using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ListeningToHarpsichord : ThoughtWorker_MusicalInstrumentListeningBase
	{
		protected override ThingDef InstrumentDef => ThingDefOf.Harpsichord;
	}
}
