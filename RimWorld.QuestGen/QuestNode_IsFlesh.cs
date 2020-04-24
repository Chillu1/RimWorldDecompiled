using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsFlesh : QuestNode_RaceProperty
	{
		protected override bool Matches(RaceProperties raceProperties)
		{
			return raceProperties.IsFlesh;
		}
	}
}
