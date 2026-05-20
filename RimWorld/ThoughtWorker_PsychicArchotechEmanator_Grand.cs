using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicArchotechEmanator_Grand : ThoughtWorker_PsychicArchotechEmanator
	{
		protected override ThingDef EmanatorDef => ThingDefOf.GrandArchotechStructure;

		protected override int InnerRadius => 10;

		protected override int OuterRadius => 18;
	}
}
