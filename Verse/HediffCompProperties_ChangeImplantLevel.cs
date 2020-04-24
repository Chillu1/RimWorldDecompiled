using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_ChangeImplantLevel : HediffCompProperties
	{
		public HediffDef implant;

		public int levelOffset;

		public List<ChangeImplantLevel_Probability> probabilityPerStage;

		public HediffCompProperties_ChangeImplantLevel()
		{
			compClass = typeof(HediffComp_ChangeImplantLevel);
		}

		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (implant == null)
			{
				yield return "implant is null";
			}
			else if (!typeof(Hediff_ImplantWithLevel).IsAssignableFrom(implant.hediffClass))
			{
				yield return "implant is not Hediff_ImplantWithLevel";
			}
			if (levelOffset == 0)
			{
				yield return "levelOffset is 0";
			}
			if (probabilityPerStage == null)
			{
				yield return "probabilityPerStage is not defined";
			}
			else if (probabilityPerStage.Count != parentDef.stages.Count)
			{
				yield return "probabilityPerStage count doesn't match Hediffs number of stages";
			}
		}
	}
}
