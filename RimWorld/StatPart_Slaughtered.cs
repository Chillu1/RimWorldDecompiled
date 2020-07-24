using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatPart_Slaughtered : StatPart
	{
		private float factor;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (CanApply(req))
			{
				val *= factor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (CanApply(req))
			{
				return "StatsReport_HasHediffExplanation".Translate() + ": x" + factor.ToStringPercent();
			}
			return null;
		}

		private bool CanApply(StatRequest req)
		{
			Pawn pawn;
			if (!req.HasThing || (pawn = (req.Thing as Pawn)) == null)
			{
				return false;
			}
			if (!pawn.def.race.Animal || pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			if (!pawn.health.hediffSet.HasHediff(HediffDefOf.ExecutionCut))
			{
				return false;
			}
			int num = 0;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (typeof(Hediff_Injury).IsAssignableFrom(hediffs[i].def.hediffClass) && !hediffs[i].IsPermanent())
				{
					num++;
					if (num > 1)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
