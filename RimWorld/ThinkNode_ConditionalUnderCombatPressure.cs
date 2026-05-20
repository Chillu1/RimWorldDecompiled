using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalUnderCombatPressure : ThinkNode_Conditional
{
	public float maxThreatDistance = 2f;

	public int minCloseTargets = 2;

	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.Spawned && !pawn.Downed)
		{
			return PawnUtility.EnemiesAreNearby(pawn, 9, passDoors: true, maxThreatDistance, minCloseTargets);
		}
		return false;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalUnderCombatPressure obj = (ThinkNode_ConditionalUnderCombatPressure)base.DeepCopy(resolve);
		obj.maxThreatDistance = maxThreatDistance;
		obj.minCloseTargets = minCloseTargets;
		return obj;
	}
}
