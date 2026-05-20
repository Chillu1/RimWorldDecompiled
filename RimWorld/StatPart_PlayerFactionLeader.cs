using Verse;

namespace RimWorld
{
	public class StatPart_PlayerFactionLeader : StatPart
	{
		private float offset;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (PawnIsLeader(req))
			{
				val += offset;
			}
		}

		private bool PawnIsLeader(StatRequest req)
		{
			Thing thing = req.Thing;
			if (thing == null)
			{
				return false;
			}
			Faction faction = thing.Faction;
			if (faction != null && faction.IsPlayer)
			{
				return faction.leader == thing;
			}
			return false;
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (PawnIsLeader(req))
			{
				return "StatsReport_LeaderOffset".Translate() + ": " + offset.ToStringWithSign("0.#%");
			}
			return null;
		}
	}
}
