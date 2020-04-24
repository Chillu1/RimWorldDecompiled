using Verse;

namespace RimWorld
{
	public class StatPart_GearAndInventoryMass : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetValue(req, out float value))
			{
				val += value;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetValue(req, out float value))
			{
				return "StatsReport_GearAndInventoryMass".Translate() + ": " + value.ToStringMassOffset();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => MassUtility.GearAndInventoryMass(x), (ThingDef x) => 0f, out value);
		}
	}
}
