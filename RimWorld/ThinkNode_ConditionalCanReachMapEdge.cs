using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalCanReachMapEdge : ThinkNode_Conditional
{
	public bool allowFlyOutInstead;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalCanReachMapEdge obj = (ThinkNode_ConditionalCanReachMapEdge)base.DeepCopy(resolve);
		obj.allowFlyOutInstead = allowFlyOutInstead;
		return obj;
	}

	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.CanReachMapEdge())
		{
			return true;
		}
		if (allowFlyOutInstead)
		{
			if (pawn.RaceProps.canLeaveMapFlying && !pawn.Position.Roofed(pawn.Map) && pawn.Faction != Faction.OfPlayer && pawn.flight.CanEverFly)
			{
				return !pawn.IsQuestLodger();
			}
			return false;
		}
		return false;
	}
}
