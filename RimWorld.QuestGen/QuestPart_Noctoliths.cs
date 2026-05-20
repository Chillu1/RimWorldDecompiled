using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld.QuestGen;

public class QuestPart_Noctoliths : QuestPart
{
	private float points;

	private string inSignalNoctolithDamaged;

	private string inSignalNoctolithKilled;

	private int damagedCount;

	private int killedCount;

	private List<Thing> damagedNoctoliths = new List<Thing>();

	private List<Thing> noctoliths = new List<Thing>();

	private List<float> healthPercentages = new List<float>();

	private MapParent mapParent;

	private static readonly FloatRange NoctolPointsFactorRange = new FloatRange(0.8f, 1f);

	private static readonly FloatRange HealthRangeToSpawnNoctols = new FloatRange(0f, 0.99f);

	public static readonly SimpleCurve DamagedNoctolithsToPointsScaleCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0.35f),
		new CurvePoint(2f, 0.35f),
		new CurvePoint(3f, 0.5f)
	};

	private static readonly IntRange TunnelDelayTicks = new IntRange(60, 120);

	public QuestPart_Noctoliths()
	{
	}

	public QuestPart_Noctoliths(MapParent mapParent, List<Thing> noctoliths, float points, string inSignalNoctolithDamaged, string inSignalNoctolithKilled)
	{
		this.mapParent = mapParent;
		this.noctoliths = noctoliths;
		this.points = points;
		this.inSignalNoctolithDamaged = inSignalNoctolithDamaged;
		this.inSignalNoctolithKilled = inSignalNoctolithKilled;
		for (int i = 0; i < 3; i++)
		{
			healthPercentages.Add(HealthRangeToSpawnNoctols.RandomInRange);
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (mapParent == null || !mapParent.HasMap)
		{
			return;
		}
		Map map = mapParent.Map;
		Thing arg2;
		if (signal.tag == inSignalNoctolithDamaged && signal.args.TryGetArg("SUBJECT", out Thing arg) && arg.Position.InBounds(map) && arg.def == ThingDefOf.Noctolith && damagedCount < 3 && (float)arg.HitPoints / (float)arg.MaxHitPoints <= healthPercentages[damagedCount] && !damagedNoctoliths.Contains(arg))
		{
			damagedNoctoliths.Add(arg);
			damagedCount++;
			SpawnNoctols(arg, map);
		}
		else if (signal.tag == inSignalNoctolithKilled && signal.args.TryGetArg("SUBJECT", out arg2) && noctoliths.Contains(arg2))
		{
			killedCount++;
			if (!noctoliths.Any((Thing x) => !x.DestroyedOrNull()))
			{
				Find.LetterStack.ReceiveLetter("DarknessLiftingEarlyLetterLabel".Translate(), "DarknessLiftingEarlyLetterText".Translate(), LetterDefOf.PositiveEvent);
				quest.End(QuestEndOutcome.Success);
			}
			else if (killedCount == 1)
			{
				Find.LetterStack.ReceiveLetter("DarknessWaveringLetterLabel".Translate(), "DarknessWaveringLetterText".Translate(), LetterDefOf.PositiveEvent);
			}
		}
	}

	private void SpawnNoctols(Thing noctolith, Map map)
	{
		IntVec3 position = noctolith.Position;
		float num = points * DamagedNoctolithsToPointsScaleCurve.Evaluate(damagedCount);
		List<Pawn> noctolsForPoints = GetNoctolsForPoints(num, map);
		if (!noctolsForPoints.Any())
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		Lord lord = map.lordManager.lords.FirstOrDefault((Lord l) => l.LordJob is LordJob_AssaultColony && l.faction == Faction.OfEntities);
		if (lord == null)
		{
			flag2 = true;
			lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), map);
		}
		foreach (Pawn item in noctolsForPoints)
		{
			if (CellFinder.TryFindRandomCellNear(position, map, 40, (IntVec3 c) => ValidSpawnCell(c, strict: true), out var result) || CellFinder.TryFindRandomCellNear(position, map, 40, (IntVec3 c) => ValidSpawnCell(c, strict: false), out result))
			{
				PawnGroundSpawner obj = (PawnGroundSpawner)ThingMaker.MakeThing(ThingDefOf.PawnGroundSpawner);
				obj.Init(item, TunnelDelayTicks);
				GenSpawn.Spawn(obj, result, map);
				flag = true;
				lord.AddPawn(item);
			}
		}
		SoundDefOf.Noctolith_Destroyed.PlayOneShotOnCamera();
		if (flag)
		{
			Find.LetterStack.ReceiveLetter("NoctolAttackLetterLabel".Translate(), "NoctolAttackLetterText".Translate(), LetterDefOf.ThreatBig, new LookTargets(noctolsForPoints));
		}
		else if (flag2)
		{
			map.lordManager.RemoveLord(lord);
		}
		bool ValidSpawnCell(IntVec3 c, bool strict)
		{
			if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map))
			{
				return false;
			}
			Region region = c.GetRegion(map);
			if (region != null && region.type == RegionType.Portal)
			{
				return false;
			}
			if (strict && map.glowGrid.GroundGlowAt(c) >= 0.01f)
			{
				return false;
			}
			return true;
		}
	}

	private List<Pawn> GetNoctolsForPoints(float points, Map map)
	{
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Noctols,
			tile = map.Tile,
			faction = Faction.OfEntities
		};
		pawnGroupMakerParms.points = ((points > 0f) ? points : StorytellerUtility.DefaultThreatPointsNow(map)) * NoctolPointsFactorRange.RandomInRange;
		pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, pawnGroupMakerParms.faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind) * 1.05f);
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms)?.ToList();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		damagedNoctoliths.Clear();
		healthPercentages.Clear();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref points, "points", 0f);
		Scribe_Values.Look(ref inSignalNoctolithDamaged, "inSignalNoctolithDamaged");
		Scribe_Values.Look(ref inSignalNoctolithKilled, "inSignalNoctolithKilled");
		Scribe_Values.Look(ref damagedCount, "damagedCount", 0);
		Scribe_Values.Look(ref killedCount, "killedCount", 0);
		Scribe_Collections.Look(ref damagedNoctoliths, "damagedNoctoliths", LookMode.Reference);
		Scribe_Collections.Look(ref noctoliths, "noctoliths", LookMode.Reference);
		Scribe_Collections.Look(ref healthPercentages, "healthPercentages", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			damagedNoctoliths.RemoveAll((Thing x) => x == null);
			noctoliths.RemoveAll((Thing x) => x == null);
		}
	}
}
