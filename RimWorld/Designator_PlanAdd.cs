using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlanAdd : Designator_Plan
	{
		public Designator_PlanAdd()
			: base(DesignateMode.Add)
		{
			defaultLabel = "DesignatorPlan".Translate();
			defaultDesc = "DesignatorPlanDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn");
			soundSucceeded = SoundDefOf.Designate_PlanAdd;
			hotKey = KeyBindingDefOf.Misc9;
		}
	}
}
