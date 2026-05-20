using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class InsectAmbush : Thing
{
	private static readonly IntRange TunnelDelayTicks = new IntRange(240, 480);

	private static readonly FloatRange RadiusRange = new FloatRange(5f, 8f);

	private static readonly SimpleCurve AmbushThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 100f),
		new CurvePoint(1000f, 300f),
		new CurvePoint(5000f, 1500f)
	};

	private float radius;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref radius, "radius", 0f);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		radius = RadiusRange.RandomInRange;
	}

	protected override void Tick()
	{
		if (!this.IsHashIntervalTick(60))
		{
			return;
		}
		Map map = base.Map;
		int num = GenRadial.NumCellsInRadius(radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = base.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			foreach (Thing thing in c.GetThingList(map))
			{
				if (thing is Pawn { IsColonistPlayerControlled: not false } pawn && GenSight.LineOfSightToThing(pawn.Position, this, base.Map))
				{
					Activate();
					return;
				}
			}
		}
	}

	private void Activate()
	{
		float points = AmbushThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			points = points,
			faction = Faction.OfInsects
		}).ToList();
		List<Thing> list2 = new List<Thing>();
		foreach (Pawn item in list)
		{
			if (CellFinder.TryFindRandomReachableNearbyCell(base.Position, base.Map, 5f, TraverseParms.For(TraverseMode.ByPawn), (IntVec3 cell) => cell.Standable(base.Map), null, out var result))
			{
				PawnGroundSpawner pawnGroundSpawner = (PawnGroundSpawner)ThingMaker.MakeThing(ThingDefOf.PawnGroundSpawner);
				pawnGroundSpawner.Init(item, TunnelDelayTicks);
				GenSpawn.Spawn(pawnGroundSpawner, result, base.Map);
				list2.Add(pawnGroundSpawner);
			}
		}
		Find.LetterStack.ReceiveLetter("InsectAmbushLetter".Translate(), "InsectAmbushLetterText".Translate(), LetterDefOf.ThreatBig, list2);
		LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_AssaultColony(), base.Map, list);
		Destroy();
	}
}
