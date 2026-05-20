namespace RimWorld;

[DefOf]
public static class ComplexThreatDefOf
{
	[MayRequireIdeology]
	public static ComplexThreatDef SleepingInsects;

	[MayRequireIdeology]
	public static ComplexThreatDef Infestation;

	[MayRequireIdeology]
	public static ComplexThreatDef SleepingMechanoids;

	[MayRequireIdeology]
	public static ComplexThreatDef CryptosleepPods;

	static ComplexThreatDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ComplexThreatDefOf));
	}
}
