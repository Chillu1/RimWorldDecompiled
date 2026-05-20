using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_Dreadmeld : GenStep
{
	private const int HiveSize = 100;

	private const int HiveInteriorSize = 20;

	private const float MinSpawnDistFromGateExit = 20f;

	private static readonly IntRange NumFleshBulbsRange = new IntRange(2, 4);

	public override int SeedPart => 21341517;

	public override void Generate(Map map, GenStepParams parms)
	{
		PocketMapExit pitGateExit = (PocketMapExit)map.listerThings.ThingsOfDef(ThingDefOf.CaveExit).First();
		CellFinder.TryFindRandomCell(map, (IntVec3 c) => c.Standable(map) && !c.InHorDistOf(pitGateExit.Position, 20f) && c.DistanceToEdge(map) > 10, out var result);
		List<IntVec3> list = GridShapeMaker.IrregularLump(result, map, 100);
		List<IntVec3> list2 = GridShapeMaker.IrregularLump(result, map, 20);
		foreach (IntVec3 item in list)
		{
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Fleshmass), item, map, Rot4.Random).SetFaction(Faction.OfEntities);
			map.terrainGrid.SetTerrain(item, TerrainDefOf.Flesh);
		}
		foreach (IntVec3 item2 in list2)
		{
			foreach (Thing item3 in from t in item2.GetThingList(map).ToList()
				where t.def.destroyable
				select t)
			{
				item3.Destroy();
			}
		}
		int randomInRange = NumFleshBulbsRange.RandomInRange;
		for (int num = 0; num < randomInRange; num++)
		{
			if (CellFinder.TryFindRandomCellNear(result, map, 5, (IntVec3 c) => c.GetEdifice(map) == null, out var result2))
			{
				GenSpawn.Spawn(ThingDefOf.Fleshbulb, result2, map).SetFaction(Faction.OfEntities);
			}
		}
		GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Dreadmeld, Faction.OfEntities)), result, map, Rot4.Random);
		string signalTag = "dreadmeldApproached-" + Find.UniqueIDsManager.GetNextSignalTagID();
		CellRect rect = CellRect.FromCellList(list).ExpandedBy(2).ClipInsideMap(map);
		RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
		obj.signalTag = signalTag;
		obj.Rect = rect;
		obj.destroyIfUnfogged = true;
		GenSpawn.Spawn(obj, rect.CenterCell, map);
		SignalAction_Letter obj2 = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
		obj2.signalTag = signalTag;
		obj2.letterDef = LetterDefOf.ThreatBig;
		obj2.letterLabelKey = "LetterLabelDreadmeldWarning";
		obj2.letterMessageKey = "LetterDreadmeldWarning";
		GenSpawn.Spawn(obj2, rect.CenterCell, map);
	}
}
