namespace RimWorld
{
	public class GoodwillSituationWorker_PermanentEnemy : GoodwillSituationWorker
	{
		public override int GetMaxGoodwill(Faction other)
		{
			if (Faction.OfPlayerSilentFail == null)
			{
				return 100;
			}
			if (!ArePermanentEnemies(Faction.OfPlayer, other))
			{
				return 100;
			}
			return -100;
		}

		public static bool ArePermanentEnemies(Faction a, Faction b)
		{
			if (a.def.permanentEnemy || b.def.permanentEnemy)
			{
				return true;
			}
			if (a.def.permanentEnemyToEveryoneExceptPlayer && !b.IsPlayer)
			{
				return true;
			}
			if (b.def.permanentEnemyToEveryoneExceptPlayer && !a.IsPlayer)
			{
				return true;
			}
			if (a.def.permanentEnemyToEveryoneExcept != null && !a.def.permanentEnemyToEveryoneExcept.Contains(b.def))
			{
				return true;
			}
			if (b.def.permanentEnemyToEveryoneExcept != null && !b.def.permanentEnemyToEveryoneExcept.Contains(a.def))
			{
				return true;
			}
			return false;
		}
	}
}
