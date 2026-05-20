using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_PlayerWealth : QuestPart_Filter
	{
		public float minPlayerWealth;

		protected override bool Pass(SignalArgs args)
		{
			return WealthUtility.PlayerWealth >= minPlayerWealth;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref minPlayerWealth, "minPlayerWealth", 0f);
		}
	}
}
