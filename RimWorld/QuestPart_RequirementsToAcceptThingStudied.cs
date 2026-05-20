using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptThingStudied : QuestPart_RequirementsToAccept
	{
		public Thing thing;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				CompStudiable compStudiable = thing?.TryGetComp<CompStudiable>();
				if (compStudiable != null && !compStudiable.Completed)
				{
					yield return thing;
				}
			}
		}

		public override AcceptanceReport CanAccept()
		{
			CompStudiable compStudiable = thing?.TryGetComp<CompStudiable>();
			if (compStudiable != null && !compStudiable.Completed)
			{
				return new AcceptanceReport("QuestRequiredThingStudied".Translate(thing));
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref thing, "thing");
		}
	}
}
