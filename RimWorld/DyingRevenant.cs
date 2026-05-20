using Verse;

namespace RimWorld;

public class DyingRevenant : Thing
{
	private int bioSignature;

	private int completeTick;

	protected override void Tick()
	{
		if (Find.TickManager.TicksGame >= completeTick)
		{
			Complete();
		}
	}

	public void InitWith(Pawn revenant)
	{
		bioSignature = revenant.TryGetComp<CompRevenant>().biosignature;
		Effecter effecter = EffecterDefOf.RevenantDeath.SpawnMaintained(base.Position, base.Map);
		completeTick = base.TickSpawned + effecter.ticksLeft;
	}

	public void Complete()
	{
		if (FilthMaker.TryMakeFilth(base.PositionHeld, base.Map, ThingDefOf.Filth_RevenantBloodPool))
		{
			EffecterDefOf.RevenantKilledCompleteBurst.SpawnMaintained(base.PositionHeld, base.Map);
			foreach (IntVec3 item in CellRect.CenteredOn(base.PositionHeld, 2))
			{
				Plant plant = item.GetPlant(base.Map);
				if (plant != null && plant.MaxHitPoints < 100)
				{
					plant.Destroy();
				}
			}
		}
		Thing thing = ThingMaker.MakeThing(ThingDefOf.RevenantSpine);
		thing.TryGetComp<CompBiosignatureOwner>().biosignature = bioSignature;
		GenSpawn.Spawn(thing, base.PositionHeld, base.Map);
		Destroy();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref completeTick, "completeTick", 0);
		Scribe_Values.Look(ref bioSignature, "bioSignature", 0);
	}
}
