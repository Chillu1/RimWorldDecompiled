using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_MoveInBossgroup : LordToil
	{
		private static readonly FloatRange EscortRadiusRanged = new FloatRange(5f, 10f);

		private List<Pawn> bosses = new List<Pawn>();

		public override bool AllowSatisfyLongNeeds => false;

		public override bool ForceHighStoryDanger => true;

		public LordToil_MoveInBossgroup(IEnumerable<Pawn> bosses)
		{
			this.bosses.AddRange(bosses);
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (bosses.Contains(lord.ownedPawns[i]))
				{
					bosses[i].mindState.duty = new PawnDuty(bosses[i].RaceProps.dutyBoss ?? DutyDefOf.AssaultColony);
				}
				else
				{
					lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Escort, bosses.RandomElement(), EscortRadiusRanged.RandomInRange);
				}
			}
		}
	}
}
