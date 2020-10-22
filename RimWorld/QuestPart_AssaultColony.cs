using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_AssaultColony : QuestPart_MakeLord
	{
		protected override Lord MakeLord()
		{
			return LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction), base.Map);
		}
	}
}
