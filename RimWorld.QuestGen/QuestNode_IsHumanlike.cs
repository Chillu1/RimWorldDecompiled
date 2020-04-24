using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsHumanlike : QuestNode_RaceProperty
	{
		protected override bool Matches(RaceProperties raceProperties)
		{
			return raceProperties.Humanlike;
		}
	}
}
