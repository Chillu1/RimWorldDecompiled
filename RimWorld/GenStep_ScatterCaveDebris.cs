using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_ScatterCaveDebris : GenStep_Scatterer
{
	private static readonly IntRange CorpseRandomAgeRange = new IntRange(900000, 18000000);

	private Rot4 rotation;

	private static List<Rot4> edgesToCheck = new List<Rot4>();

	public override int SeedPart => 85037593;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter cave debris") && Find.World.HasCaves(map.Tile))
		{
			count = 1;
			base.Generate(map, parms);
		}
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		RoofDef roof = c.GetRoof(map);
		if (roof == null || !roof.isNatural)
		{
			return false;
		}
		int num = Rand.RangeInclusive(1, 4);
		for (int i = 0; i < 4; i++)
		{
			rotation = new Rot4(i % num);
			if (CanPlace(c, rotation, map))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanPlace(IntVec3 c, Rot4 r, Map map)
	{
		if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.NoPassClosedDoors)))
		{
			return false;
		}
		CellRect cellRect = GenAdj.OccupiedRect(c, r, ThingDefOf.AncientBed.size);
		foreach (IntVec3 item in cellRect)
		{
			if (item.GetEdifice(map) != null)
			{
				return false;
			}
		}
		edgesToCheck.Clear();
		if (cellRect.Width > cellRect.Height)
		{
			edgesToCheck.Add(Rot4.North);
			edgesToCheck.Add(Rot4.South);
		}
		else
		{
			edgesToCheck.Add(Rot4.East);
			edgesToCheck.Add(Rot4.West);
		}
		edgesToCheck.Shuffle();
		foreach (Rot4 item2 in edgesToCheck)
		{
			bool flag = true;
			foreach (IntVec3 edgeCell in cellRect.GetEdgeCells(item2))
			{
				IntVec3 c2 = edgeCell + item2.FacingCell;
				if (c2.InBounds(map) && c2.GetEdifice(map) != null && c2.GetEdifice(map).def.building.isNaturalRock)
				{
					flag = false;
					break;
				}
			}
			foreach (IntVec3 edgeCell2 in cellRect.GetEdgeCells(item2.Opposite))
			{
				IntVec3 c3 = edgeCell2 + item2.Opposite.FacingCell;
				if (c3.InBounds(map) && c3.GetEdifice(map) == null)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				edgesToCheck.Clear();
				return true;
			}
		}
		edgesToCheck.Clear();
		return false;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientBed), loc, map, rotation);
		IntVec3 result = IntVec3.Invalid;
		if (CellFinder.TryFindRandomCellNear(loc, map, 5, CellPredicate, out result))
		{
			FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_MoldyUniform);
		}
		if (CellFinder.TryFindRandomCellNear(loc, map, 5, CellPredicate, out result))
		{
			FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_CorpseBile);
		}
		if (Rand.Bool && CellFinder.TryFindRandomCellNear(loc, map, 5, CellPredicate, out result))
		{
			FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_ScatteredDocuments);
		}
		if (Rand.Bool && CellFinder.TryFindRandomCellNear(loc, map, 5, CellPredicate, out result))
		{
			FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_DriedBlood);
		}
		IntVec3 result2 = IntVec3.Invalid;
		if (Rand.Chance(0.25f) && CellFinder.TryFindRandomCellNear(loc, map, 5, CellPredicate, out result2))
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Drifter, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: true, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: true));
			pawn.Corpse.Age = CorpseRandomAgeRange.RandomInRange;
			pawn.relations.hidePawnRelations = true;
			pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
			GenSpawn.Spawn(pawn.Corpse, result2, map);
		}
		bool CellPredicate(IntVec3 c)
		{
			if (c.GetEdifice(map) != null || !c.Roofed(map))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Filth)
				{
					return false;
				}
			}
			return true;
		}
	}
}
