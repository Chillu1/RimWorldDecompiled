using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_ExitOnShuttle : QuestPart_MakeLord
	{
		public Thing shuttle;

		public bool addFleeToil = true;

		public QuestPart_ExitOnShuttle()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttles are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657212);
			}
		}

		protected override Lord MakeLord()
		{
			return LordMaker.MakeNewLord(faction, new LordJob_ExitOnShuttle(shuttle, addFleeToil), base.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
		}
	}
}
