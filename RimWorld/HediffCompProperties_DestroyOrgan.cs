using Verse;

namespace RimWorld;

public class HediffCompProperties_DestroyOrgan : HediffCompProperties
{
	public string messageText;

	public DamageDef damageType;

	public HediffCompProperties_DestroyOrgan()
	{
		compClass = typeof(HediffComp_DestroyOrgan);
	}
}
