using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GeneratePawnRandDevelopmentStage : QuestNode_GeneratePawn
	{
		public SlateRef<float> childChance;

		public SlateRef<float> adultChance;

		protected override DevelopmentalStage GetDevelopmentalStage(Slate slate)
		{
			if (!Find.Storyteller.difficulty.ChildrenAllowed)
			{
				return DevelopmentalStage.Adult;
			}
			float value = childChance.GetValue(slate);
			float value2 = adultChance.GetValue(slate);
			float num = value / (value + value2);
			if (!(Rand.Value <= num))
			{
				return DevelopmentalStage.Adult;
			}
			return DevelopmentalStage.Child;
		}
	}
}
