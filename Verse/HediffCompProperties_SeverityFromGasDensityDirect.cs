using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_SeverityFromGasDensityDirect : HediffCompProperties
	{
		public GasType gasType;

		public int intervalTicks = 60;

		public List<float> densityStages = new List<float>();

		public HediffCompProperties_SeverityFromGasDensityDirect()
		{
			compClass = typeof(HediffComp_SeverityFromGasDensityDirect);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (densityStages.NullOrEmpty())
			{
				yield return "densityStages is empty";
			}
			else if (parentDef.stages.NullOrEmpty())
			{
				yield return "has no stages";
			}
			else if (densityStages.Count != parentDef.stages.Count)
			{
				yield return "densityStages count doesn't match stages count";
			}
		}
	}
}
