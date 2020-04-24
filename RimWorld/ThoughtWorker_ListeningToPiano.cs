using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ListeningToPiano : ThoughtWorker_MusicalInstrumentListeningBase
	{
		protected override ThingDef InstrumentDef => ThingDefOf.Piano;
	}
}
