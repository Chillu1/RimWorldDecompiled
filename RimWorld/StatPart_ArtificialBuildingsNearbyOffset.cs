using Verse;

namespace RimWorld
{
	public class StatPart_ArtificialBuildingsNearbyOffset : StatPart
	{
		public SimpleCurve curve;

		public float radius = 10f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetArtificialBuildingOffset(req, out var offset))
			{
				val += offset;
			}
		}

		private bool TryGetArtificialBuildingOffset(StatRequest req, out float offset)
		{
			if (!req.HasThing || !req.Thing.Spawned)
			{
				offset = 0f;
				return false;
			}
			offset = curve.Evaluate(req.Thing.Map.listerArtificialBuildingsForMeditation.GetForCell(req.Thing.Position, radius).Count);
			return true;
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetArtificialBuildingOffset(req, out var offset))
			{
				return "StatsReport_NearbyArtificialStructures".Translate().CapitalizeFirst() + ": " + offset.ToStringPercent();
			}
			return null;
		}
	}
}
