using RimWorld;

namespace Verse;

public class HediffCompProperties_ChangeNeed : HediffCompProperties
{
	public NeedDef needDef;

	public float percentPerDay;

	public HediffCompProperties_ChangeNeed()
	{
		compClass = typeof(HediffComp_ChangeNeed);
	}
}
