using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnGroundSpawner : GroundSpawner, IThingHolder
{
	private IntRange spawnDelay;

	private ThingOwner<Pawn> innerContainer;

	protected override IntRange ResultSpawnDelay => spawnDelay;

	public PawnGroundSpawner()
	{
		dustMoteSpawnMTB = 0.5f;
		filthSpawnMTB = 0.5f;
		filthSpawnRadius = 1f;
		innerContainer = new ThingOwner<Pawn>(this, oneStackOnly: true);
	}

	public void Init(Pawn pawnToSpawn, IntRange delayTicksRange)
	{
		spawnDelay = delayTicksRange;
		innerContainer.ClearAndDestroyContentsOrPassToWorld();
		innerContainer.TryAdd(pawnToSpawn);
	}

	protected override void Spawn(Map map, IntVec3 loc)
	{
		innerContainer.TryDropAll(loc, map, ThingPlaceMode.Direct, null, null, playDropSound: false);
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref spawnDelay, "spawnDelay");
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
	}
}
