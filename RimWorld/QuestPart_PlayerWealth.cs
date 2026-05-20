using Verse;

namespace RimWorld
{
	public class QuestPart_PlayerWealth : QuestPartActivable
	{
		public float playerWealth = 100000f;

		public const int CheckInterval = 60;

		public override void QuestPartTick()
		{
			if (Find.TickManager.TicksGame % 60 == 0 && WealthUtility.PlayerWealth >= playerWealth)
			{
				Complete();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref playerWealth, "playerWealth", 0f);
		}
	}
}
