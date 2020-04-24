using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlanRemove : Designator_Plan
	{
		public Designator_PlanRemove()
			: base(DesignateMode.Remove)
		{
			defaultLabel = "DesignatorPlanRemove".Translate();
			defaultDesc = "DesignatorPlanRemoveDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOff");
			soundSucceeded = SoundDefOf.Designate_PlanRemove;
			hotKey = KeyBindingDefOf.Misc8;
		}
	}
}
