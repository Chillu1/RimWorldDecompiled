using Verse.AI;

namespace RimWorld
{
	public class LordToil_DefendHiveAggressively : LordToil_HiveRelated
	{
		public float distToHiveToAttack = 40f;

		public override void UpdateAllDuties()
		{
			FilterOutUnspawnedHives();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Hive hiveFor = GetHiveFor(lord.ownedPawns[i]);
				PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively, hiveFor, distToHiveToAttack);
				lord.ownedPawns[i].mindState.duty = duty;
			}
		}
	}
}
