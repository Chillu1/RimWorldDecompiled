using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_Venerate : QuestPart_MakeLord
	{
		public Thing target;

		public int venerateDurationTicks;

		public string outSignalVenerationCompleted;

		public string inSignalForceExit;

		protected override Lord MakeLord()
		{
			return LordMaker.MakeNewLord(faction, new LordJob_Venerate(target, venerateDurationTicks, outSignalVenerationCompleted, inSignalForceExit), base.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref venerateDurationTicks, "venerateDurationTicks", 0);
			Scribe_Values.Look(ref inSignalForceExit, "inSignalForceExit");
		}
	}
}
