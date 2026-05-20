using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptPlayerWealth : QuestPart_RequirementsToAccept
	{
		public float requiredPlayerWealth = -1f;

		public override AcceptanceReport CanAccept()
		{
			if (WealthUtility.PlayerWealth < requiredPlayerWealth)
			{
				return new AcceptanceReport("QuestRequiredPlayerWealth".Translate(requiredPlayerWealth.ToStringMoney()));
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requiredPlayerWealth, "requiredPlayerWealth", 0f);
		}
	}
}
