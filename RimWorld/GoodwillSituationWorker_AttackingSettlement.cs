using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class GoodwillSituationWorker_AttackingSettlement : GoodwillSituationWorker
	{
		private const int MaxGoodwill = -80;

		public override int GetMaxGoodwill(Faction other)
		{
			if (!IsAttackingSettlement(other))
			{
				return 100;
			}
			return -80;
		}

		private bool IsAttackingSettlement(Faction other)
		{
			if (Current.ProgramState == ProgramState.Entry)
			{
				return false;
			}
			return SettlementUtility.IsPlayerAttackingAnySettlementOf(other);
		}
	}
}
