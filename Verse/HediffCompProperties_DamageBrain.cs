using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_DamageBrain : HediffCompProperties
	{
		public IntRange damageAmount = IntRange.zero;

		public List<float> mtbDaysPerStage;

		public HediffCompProperties_DamageBrain()
		{
			compClass = typeof(HediffComp_DamageBrain);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (damageAmount == IntRange.zero)
			{
				yield return "damageAmount is not defined";
			}
			if (mtbDaysPerStage == null)
			{
				yield return "mtbDaysPerStage is not defined";
			}
			else if (mtbDaysPerStage.Count != parentDef.stages.Count)
			{
				yield return "mtbDaysPerStage count doesn't match Hediffs number of stages";
			}
		}
	}
}
