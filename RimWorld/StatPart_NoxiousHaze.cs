using Verse;

namespace RimWorld
{
	public class StatPart_NoxiousHaze : StatPart
	{
		private float multiplier = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (ActiveFor(req.Thing))
			{
				val *= multiplier;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				return "StatsReport_MultiplierFor".Translate(GameConditionDefOf.NoxiousHaze.label) + (": x" + multiplier.ToStringPercent());
			}
			return null;
		}

		private bool ActiveFor(Thing t)
		{
			if (!ModLister.CheckBiotech("Noxious haze stat part"))
			{
				return false;
			}
			if (t != null && t.def.deteriorateFromEnvironmentalEffects)
			{
				return NoxiousHazeUtility.IsExposedToNoxiousHaze(t);
			}
			return false;
		}
	}
}
