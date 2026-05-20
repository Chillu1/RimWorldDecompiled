namespace RimWorld;

[DefOf]
public static class TrainableDefOf
{
	public static TrainableDef Tameness;

	public static TrainableDef Obedience;

	public static TrainableDef Release;

	[MayRequireOdyssey]
	public static TrainableDef AttackTarget;

	[MayRequireOdyssey]
	public static TrainableDef Comfort;

	[MayRequireOdyssey]
	public static TrainableDef Forage;

	[MayRequireOdyssey]
	public static TrainableDef Dig;

	[MayRequireOdyssey]
	public static TrainableDef EggSpew;

	[MayRequireOdyssey]
	public static TrainableDef SludgeSpew;

	static TrainableDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(TrainableDefOf));
	}
}
