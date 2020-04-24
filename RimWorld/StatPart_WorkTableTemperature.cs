using Verse;

namespace RimWorld
{
	public class StatPart_WorkTableTemperature : StatPart
	{
		public const float WorkRateFactor = 0.7f;

		public const float MinTemp = 9f;

		public const float MaxTemp = 35f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				val *= 0.7f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				return "BadTemperature".Translate().CapitalizeFirst() + ": x" + 0.7f.ToStringPercent();
			}
			return null;
		}

		public static bool Applies(Thing t)
		{
			if (!t.Spawned)
			{
				return false;
			}
			return Applies(t.def, t.Map, t.Position);
		}

		public static bool Applies(ThingDef tDef, Map map, IntVec3 c)
		{
			if (map == null)
			{
				return false;
			}
			if (tDef.building == null)
			{
				return false;
			}
			float temperatureForCell = GenTemperature.GetTemperatureForCell(c, map);
			if (!(temperatureForCell < 9f))
			{
				return temperatureForCell > 35f;
			}
			return true;
		}
	}
}
