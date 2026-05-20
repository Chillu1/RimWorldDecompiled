using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ExpectationDef : Def
	{
		public int order = -1;

		public int thoughtStage = -1;

		public float maxMapWealth = -1f;

		public float joyToleranceDropPerDay;

		public float ritualQualityOffset;

		public int joyKindsNeeded;

		public bool forRoles;

		public bool WealthTriggered => maxMapWealth >= 0f;

		public override IEnumerable<string> ConfigErrors()
		{
			if (order < 0)
			{
				yield return "order not defined";
			}
		}
	}
}
