using System.Collections.Generic;

namespace RimWorld;

public class CompProperties_FacilityQualityBased : CompProperties_Facility
{
	public Dictionary<StatDef, Dictionary<QualityCategory, float>> statOffsetsPerQuality;

	public CompProperties_FacilityQualityBased()
	{
		compClass = typeof(CompFacilityQualityBased);
	}
}
