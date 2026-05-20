using Verse;

namespace RimWorld;

public class StatPart_Outdoors : StatPart
{
	private float factorIndoors = 1f;

	private float factorOutdoors = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		val *= OutdoorsFactor(req);
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing.GetRoom() != null)
		{
			string text = ((!ConsideredOutdoors(req)) ? ((string)"Indoors".Translate()) : ((string)"Outdoors".Translate().CapitalizeFirst()));
			return text + ": x" + OutdoorsFactor(req).ToStringPercent();
		}
		return null;
	}

	private float OutdoorsFactor(StatRequest req)
	{
		if (ConsideredOutdoors(req))
		{
			return factorOutdoors;
		}
		return factorIndoors;
	}

	private bool ConsideredOutdoors(StatRequest req)
	{
		if (req.HasThing)
		{
			Room room = req.Thing.GetRoom();
			if (room != null)
			{
				if (room.OutdoorsForWork)
				{
					return true;
				}
				if (req.HasThing && req.Thing.Spawned && !req.Thing.Map.roofGrid.Roofed(req.Thing.Position))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}
}
