namespace RimWorld
{
	public class GoodwillSituationWorker_NaturalEnemy : GoodwillSituationWorker
	{
		public override int GetNaturalGoodwillOffset(Faction other)
		{
			if (!other.def.naturalEnemy)
			{
				return 0;
			}
			return -130;
		}
	}
}
