using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_EscortPawn : QuestPart_MakeLord
	{
		public Pawn escortee;

		public Thing shuttle;

		protected override Lord MakeLord()
		{
			return LordMaker.MakeNewLord(faction, new LordJob_EscortPawn(escortee, shuttle), base.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref escortee, "escortee");
			Scribe_References.Look(ref shuttle, "shuttle");
		}
	}
}
