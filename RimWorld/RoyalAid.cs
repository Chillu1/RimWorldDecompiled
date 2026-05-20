using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoyalAid
{
	public int favorCost;

	public int points;

	public FloatRange? overrideAcceptableTemperatureRange;

	public int pawnCount;

	public PawnKindDef pawnKindDef;

	public float targetingRange;

	public bool targetingRequireLOS = true;

	public float aidDurationDays;

	public float radius;

	public int intervalTicks;

	public int explosionCount;

	public int warmupTicks;

	public FloatRange explosionRadiusRange;

	public List<ThingDefCountClass> itemsToDrop;
}
