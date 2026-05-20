using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompMechhiveAssembler : ThingComp
{
	private static readonly SimpleCurve MaxThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 200f),
		new CurvePoint(1000f, 250f),
		new CurvePoint(5000f, 750f),
		new CurvePoint(10000f, 1250f)
	};

	private static readonly SimpleCurve SpawnIntervalCurve = new SimpleCurve
	{
		new CurvePoint(100f, 35000f),
		new CurvePoint(1000f, 30000f),
		new CurvePoint(5000f, 20000f),
		new CurvePoint(10000f, 15000f)
	};

	private int lastSpawnTick;

	private float mapThreatPoints = -1f;

	[Unsaved(false)]
	private CompCerebrexCore core;

	[Unsaved(false)]
	private CompCanBeDormant dormancyCompCached;

	private CompProperties_MechhiveAssembler Props => (CompProperties_MechhiveAssembler)props;

	private CompCanBeDormant DormancyComp => dormancyCompCached ?? (dormancyCompCached = parent.TryGetComp<CompCanBeDormant>());

	private bool Active
	{
		get
		{
			if (core != null)
			{
				return DormancyComp.Awake;
			}
			return false;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref lastSpawnTick, "lastSpawnTick", 0);
		Scribe_Values.Look(ref mapThreatPoints, "mapThreatPoints", 0f);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		List<Thing> list = parent.Map.listerThings.ThingsOfDef(ThingDefOf.CerebrexCore);
		if (!list.NullOrEmpty())
		{
			core = list[0]?.TryGetComp<CompCerebrexCore>();
		}
		if (!respawningAfterLoad)
		{
			mapThreatPoints = (parent.Map.Parent as Site)?.ActualThreatPoints ?? (-1f);
			if (mapThreatPoints < 0f)
			{
				Log.Error("Mechhive assembler failed to get threat points from site");
				mapThreatPoints = StorytellerUtility.DefaultThreatPointsNow(parent.Map);
			}
			lastSpawnTick = Find.TickManager.TicksGame;
		}
	}

	public override void CompTick()
	{
		if (Active)
		{
			float num = (float)lastSpawnTick + SpawnIntervalCurve.Evaluate(mapThreatPoints);
			if ((float)Find.TickManager.TicksGame > num)
			{
				lastSpawnTick = Find.TickManager.TicksGame;
				SpawnMech();
			}
		}
	}

	private void SpawnMech()
	{
		float pointsPerAssembler = MaxThreatPointsCurve.Evaluate(mapThreatPoints);
		float remainingPoints = pointsPerAssembler * 4f;
		foreach (Pawn ownedPawn in core.AssemblerLord.ownedPawns)
		{
			remainingPoints -= ownedPawn.kindDef.combatPower;
		}
		IEnumerable<PawnGenOption> source = Props.options.Where((PawnGenOption x) => x.Cost < pointsPerAssembler && x.Cost < remainingPoints);
		if (source.Any())
		{
			PawnGenOption pawnGenOption = source.RandomElementByWeight((PawnGenOption x) => x.selectionWeight);
			if (pawnGenOption != null)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(pawnGenOption.kind, core.AssemblerLord.faction);
				IntVec3 loc = CellFinder.StandableCellNear(parent.Position, parent.Map, 5f);
				GenSpawn.Spawn(pawn, loc, parent.Map);
				core.AssemblerLord.AddPawn(pawn);
				remainingPoints -= pawnGenOption.Cost;
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		if (Active)
		{
			return "ProducingMechanoids".Translate();
		}
		return null;
	}
}
