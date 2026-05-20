using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicArchotechEmanator_Core : ThoughtWorker_PsychicArchotechEmanator
	{
		protected override ThingDef EmanatorDef => ThingDefOf.ArchonexusCore;

		protected override int InnerRadius => 11;

		protected override int OuterRadius => 28;
	}
}
