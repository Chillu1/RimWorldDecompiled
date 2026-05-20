using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Scanner : CompProperties
{
	public float scanFindMtbDays;

	public float scanFindGuaranteedDays = -1f;

	public StatDef scanSpeedStat;

	public SoundDef soundWorking;

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (scanFindMtbDays <= 0f)
		{
			yield return "scanFindMtbDays not set";
		}
	}
}
