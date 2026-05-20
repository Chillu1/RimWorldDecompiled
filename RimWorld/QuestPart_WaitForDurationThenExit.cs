using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_WaitForDurationThenExit : QuestPart_MakeLord
	{
		public IntVec3 point;

		public int durationTicks;

		protected override Lord MakeLord()
		{
			return LordMaker.MakeNewLord(faction, new LordJob_WaitForDurationThenExit(point, durationTicks), base.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref point, "point");
			Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
		}
	}
}
