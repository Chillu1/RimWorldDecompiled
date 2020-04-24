using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsAnimal : QuestNode_RaceProperty
	{
		protected override bool Matches(RaceProperties raceProperties)
		{
			return raceProperties.Animal;
		}
	}
}
