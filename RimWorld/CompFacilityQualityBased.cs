using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompFacilityQualityBased : CompFacility
{
	private List<StatModifier> statOffsets = new List<StatModifier>();

	public new CompProperties_FacilityQualityBased Props => (CompProperties_FacilityQualityBased)props;

	public override List<StatModifier> StatOffsets => statOffsets;

	public virtual void PostQualitySet()
	{
		SetStatOffsets();
	}

	private void SetStatOffsets()
	{
		statOffsets.Clear();
		if (!parent.TryGetQuality(out var qc))
		{
			Log.Error("Could not get parent quality for CompFacilityQualityBased of " + parent.def.defName);
			return;
		}
		foreach (KeyValuePair<StatDef, Dictionary<QualityCategory, float>> item in Props.statOffsetsPerQuality)
		{
			statOffsets.Add(new StatModifier
			{
				stat = item.Key,
				value = item.Value[qc]
			});
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			SetStatOffsets();
		}
	}
}
