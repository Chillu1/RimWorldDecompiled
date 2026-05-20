using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class StatPart_TerrainMoveSpeed : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (!req.HasThing || !(req.Thing is Pawn pawn))
		{
			return null;
		}
		if (pawn.kindDef.moveSpeedFactorByTerrainTag == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, float> item in pawn.kindDef.moveSpeedFactorByTerrainTag)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			TaggedString taggedString = ("TerrainTag" + item.Key).Translate();
			stringBuilder.AppendLine("StatsReport_TerrainSpeedMultiplier".Translate(taggedString) + ": x" + item.Value.ToStringPercent());
		}
		return stringBuilder.ToString();
	}
}
