using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatPart_CorpseCasket : StatPart
{
	public int offsetOccupied;

	public List<ThingDef> thingDefs;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ApplyTo(req))
		{
			val += offsetOccupied;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (!ApplyTo(req))
		{
			return null;
		}
		return string.Concat("StatsReport_OccupiedCorpseCasket".Translate() + ": ", offsetOccupied.ToString());
	}

	private bool ApplyTo(StatRequest req)
	{
		if (req.Thing is Building_CorpseCasket { HasCorpse: not false } && offsetOccupied != 0)
		{
			if (thingDefs != null)
			{
				return thingDefs.Contains(req.Thing.def);
			}
			return true;
		}
		return false;
	}
}
