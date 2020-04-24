using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_Disorientation : HediffCompProperties
	{
		public float wanderMtbHours = -1f;

		public float wanderRadius;

		public int singleWanderDurationTicks = -1;

		public HediffCompProperties_Disorientation()
		{
			compClass = typeof(HediffComp_Disorientation);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (wanderMtbHours <= 0f)
			{
				yield return "wanderMtbHours must be greater than zero";
			}
			if (singleWanderDurationTicks <= 0)
			{
				yield return "singleWanderDurationTicks must be greater than zero";
			}
			if (wanderRadius <= 0f)
			{
				yield return "wanderRadius must be greater than zero";
			}
		}
	}
}
