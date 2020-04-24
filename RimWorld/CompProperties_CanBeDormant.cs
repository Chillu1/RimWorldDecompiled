using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_CanBeDormant : CompProperties
	{
		public bool startsDormant;

		public string wakeUpSignalTag = "CompCanBeDormant.WakeUp";

		public float maxDistAwakenByOther = 40f;

		public bool canWakeUpFogged = true;

		public string awakeStateLabelKey = "AwokeDaysAgo";

		public string dormantStateLabelKey = "DormantCompInactive";

		public CompProperties_CanBeDormant()
		{
			compClass = typeof(CompCanBeDormant);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (!parentDef.receivesSignals)
			{
				yield return "ThingDefs with CanBeDormant component must have receivesSignals set to true, otherwise wakeup won't work properly!";
			}
		}
	}
}
