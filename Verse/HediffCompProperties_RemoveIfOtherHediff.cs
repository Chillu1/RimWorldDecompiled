using System.Collections.Generic;

namespace Verse;

public class HediffCompProperties_RemoveIfOtherHediff : HediffCompProperties_MessageBase
{
	public List<HediffDef> hediffs = new List<HediffDef>();

	public IntRange? stages;

	public int mtbHours;

	public HediffCompProperties_RemoveIfOtherHediff()
	{
		compClass = typeof(HediffComp_RemoveIfOtherHediff);
	}
}
