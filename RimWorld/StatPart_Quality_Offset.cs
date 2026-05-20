using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class StatPart_Quality_Offset : StatPart
{
	public float offsetAwful;

	public float offsetPoor;

	public float offsetNormal;

	public float offsetGood;

	public float offsetExcellent;

	public float offsetMasterwork;

	public float offsetLegendary;

	public List<ThingDef> thingDefs;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ApplyTo(req))
		{
			val += QualityOffset(req.QualityCategory);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (!ApplyTo(req))
		{
			return null;
		}
		return string.Concat("StatsReport_QualityOffset".Translate() + ": ", QualityOffset(req.QualityCategory).ToString());
	}

	private bool ApplyTo(StatRequest req)
	{
		if (thingDefs != null && (req.Def == null || !thingDefs.Contains(req.Def)))
		{
			if (req.Thing != null)
			{
				return thingDefs.Contains(req.Thing.def);
			}
			return false;
		}
		return true;
	}

	private float QualityOffset(QualityCategory qc)
	{
		return qc switch
		{
			QualityCategory.Awful => offsetAwful, 
			QualityCategory.Poor => offsetPoor, 
			QualityCategory.Normal => offsetNormal, 
			QualityCategory.Good => offsetGood, 
			QualityCategory.Excellent => offsetExcellent, 
			QualityCategory.Masterwork => offsetMasterwork, 
			QualityCategory.Legendary => offsetLegendary, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
