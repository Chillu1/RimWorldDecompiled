using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class RevenantUtility
{
	private const float SurroundingPawnsRadius = 12f;

	public const float NearbyTargetRange = 10f;

	private const int EscapeDist = 200;

	public const float EscapedRadius = 20f;

	public static readonly FloatRange SearchForTargetCooldownRangeDays = new FloatRange(1.8f, 3.2f);

	public const int WaitBeforeBecomingInvisibleTicks = 140;

	public const int CheckForAnyTargetMTB = 2500;

	public const int CheckForNearbyTargetMTB = 900;

	public static readonly SimpleCurve SpeedRangeFromBecameVisibleCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(30f, 2.8f)
	};

	public static readonly SimpleCurve RevealRangeFromNumColonistsCurve = new SimpleCurve
	{
		new CurvePoint(2f, 26f),
		new CurvePoint(3f, 18f),
		new CurvePoint(8f, 16f),
		new CurvePoint(15f, 10f)
	};

	public static readonly SimpleCurve HypnotizeDurationSecondsFromNumColonistsCurve = new SimpleCurve
	{
		new CurvePoint(2f, 15f),
		new CurvePoint(4f, 11f),
		new CurvePoint(10f, 6f)
	};

	private static HashSet<Pawn> tmpTargets = new HashSet<Pawn>();

	public static int NumSpawnedUnhypnotizedColonists(Map map)
	{
		if (map == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
		{
			if (!item.health.hediffSet.HasHediff(HediffDefOf.RevenantHypnosis))
			{
				num++;
			}
		}
		return num;
	}

	public static Pawn ScanForTarget(Pawn pawn, bool forced = false)
	{
		tmpTargets.Clear();
		TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
		RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate(Region x)
		{
			List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn2 = (Pawn)list[i];
				if (ValidTarget(pawn2))
				{
					tmpTargets.Add(pawn2);
				}
			}
			return false;
		});
		if (tmpTargets.TryRandomElement(out var result) && (forced || NearbyHumanlikePawnCount(result.Position, result.Map, 12f) < 5))
		{
			return result;
		}
		return null;
	}

	public static Pawn GetClosestTargetInRadius(Pawn pawn, float radius)
	{
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
		float num = float.MaxValue;
		Pawn result = null;
		foreach (Pawn item in list)
		{
			if (ValidTarget(item) && pawn.Position.InHorDistOf(item.Position, radius) && (float)item.Position.DistanceToSquared(pawn.Position) < num && GenSight.LineOfSightToThing(pawn.Position, item, pawn.Map))
			{
				num = item.Position.DistanceToSquared(pawn.Position);
				result = item;
			}
		}
		return result;
	}

	public static IntVec3 FindEscapeCell(Pawn pawn)
	{
		IntVec3 result = IntVec3.Invalid;
		CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 200, (IntVec3 x) => x.Standable(pawn.Map) && NearbyHumanlikePawnCount(x, pawn.Map, 20f) == 0, out result);
		return result;
	}

	public static int NearbyHumanlikePawnCount(IntVec3 pos, Map map, float radius)
	{
		int num = 0;
		foreach (Pawn item in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
		{
			if (ValidTarget(item) && !item.Downed && pos.InHorDistOf(item.Position, radius))
			{
				num++;
			}
		}
		return num;
	}

	public static bool ValidTarget(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike && pawn.Faction != Faction.OfEntities && !pawn.IsSubhuman)
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.RevenantHypnosis);
		}
		return false;
	}

	public static void CreateRevenantSmear(Pawn pawn)
	{
		FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_RevenantSmear, out var outFilth);
		if (outFilth != null)
		{
			((FilthRevenantSmear)outFilth).revenant = pawn;
			pawn.TryGetComp<CompRevenant>().revenantLastLeftSmear = Find.TickManager.TicksGame;
		}
	}

	public static void OnRevenantDeath(Pawn pawn, Map map)
	{
		Find.LetterStack.ReceiveLetter("LetterLabelRevenantKilled".Translate(), "LetterRevenantKilled".Translate(), LetterDefOf.PositiveEvent, new LookTargets(pawn.PositionHeld, map));
		((DyingRevenant)GenSpawn.Spawn(ThingDefOf.DyingRevenant, pawn.Position, map)).InitWith(pawn);
	}
}
