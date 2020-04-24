using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordJob_VoluntarilyJoinable : LordJob
	{
		public override bool AddFleeToil => false;

		public virtual float VoluntaryJoinPriorityFor(Pawn p)
		{
			return 0f;
		}
	}
}
