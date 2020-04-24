using Verse;

namespace RimWorld
{
	public class StatPart_WorkTableOutdoors : StatPart
	{
		public const float WorkRateFactor = 0.9f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				val *= 0.9f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				return "Outdoors".Translate() + ": x" + 0.9f.ToStringPercent();
			}
			return null;
		}

		public static bool Applies(Thing t)
		{
			return Applies(t.def, t.Map, t.Position);
		}

		public static bool Applies(ThingDef def, Map map, IntVec3 c)
		{
			if (def.building == null)
			{
				return false;
			}
			if (map == null)
			{
				return false;
			}
			return c.GetRoom(map, RegionType.Set_All)?.PsychologicallyOutdoors ?? false;
		}
	}
}
