using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicArchotechEmanator_Major : ThoughtWorker_PsychicArchotechEmanator
	{
		protected override ThingDef EmanatorDef => ThingDefOf.MajorArchotechStructureStudiable;

		protected override int InnerRadius => 7;

		protected override int OuterRadius => 14;
	}
}
