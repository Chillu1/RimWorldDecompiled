using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_RevenantSpeed : StatPart
{
	protected virtual bool ActiveFor(Thing t)
	{
		if (!(t is Pawn pawn) || pawn.kindDef != PawnKindDefOf.Revenant || pawn.IsPsychologicallyInvisible())
		{
			return false;
		}
		return Mathf.Max(pawn.mindState.lastBecameVisibleTick, pawn.mindState.lastForcedVisibleTick) > 0;
	}

	private float GetFactor(Pawn p)
	{
		int num = Mathf.Max(p.mindState.lastBecameVisibleTick, p.mindState.lastForcedVisibleTick);
		int numTicks = Find.TickManager.TicksGame - num;
		return RevenantUtility.SpeedRangeFromBecameVisibleCurve.Evaluate(numTicks.TicksToSeconds());
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && ActiveFor(req.Thing))
		{
			val *= GetFactor((Pawn)req.Thing);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		return null;
	}
}
