using System.Linq;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AncientTemple : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		CellRect cellRect = CellRect.Empty;
		SketchResolveParams parms = new SketchResolveParams
		{
			sketch = new Sketch(),
			monumentOpen = false,
			monumentSize = new IntVec2(rp.rect.Width, rp.rect.Height),
			allowMonumentDoors = false,
			allowWood = false,
			allowFlammableWalls = false
		};
		if (rp.allowedMonumentThings != null)
		{
			parms.allowedMonumentThings = rp.allowedMonumentThings;
		}
		else
		{
			parms.allowedMonumentThings = new ThingFilter();
			parms.allowedMonumentThings.SetAllowAll(null, includeNonStorable: true);
		}
		parms.allowedMonumentThings.SetAllow(ThingDefOf.Drape, allow: false);
		Sketch sketch = RimWorld.SketchGen.SketchGen.Generate(SketchResolverDefOf.Monument, parms);
		sketch.Spawn(map, rp.rect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: false, clearEdificeWhereFloor: true, null, dormant: false, buildRoofsInstantly: true);
		CellRect rect = SketchGenUtility.FindBiggestRect(sketch, (IntVec3 x) => sketch.TerrainAt(x) != null && sketch.ThingsAt(x).All((SketchThing y) => y.def != ThingDefOf.Wall)).MovedBy(rp.rect.CenterCell);
		for (int num = 0; num < sketch.Things.Count; num++)
		{
			if (sketch.Things[num].def == ThingDefOf.Wall)
			{
				IntVec3 c = sketch.Things[num].pos + rp.rect.CenterCell;
				cellRect = ((!cellRect.IsEmpty) ? CellRect.FromLimits(Mathf.Min(cellRect.minX, c.x), Mathf.Min(cellRect.minZ, c.z), Mathf.Max(cellRect.maxX, c.x), Mathf.Max(cellRect.maxZ, c.z)) : CellRect.SingleCell(c));
			}
		}
		if (!rect.IsEmpty)
		{
			ResolveParams resolveParams = rp;
			resolveParams.rect = rect;
			if (rp.allowedMonumentThings != null)
			{
				resolveParams.allowedMonumentThings = rp.allowedMonumentThings;
			}
			else
			{
				resolveParams.allowedMonumentThings = new ThingFilter();
				resolveParams.allowedMonumentThings.SetAllowAll(null, includeNonStorable: true);
			}
			if (ModsConfig.RoyaltyActive)
			{
				resolveParams.allowedMonumentThings.SetAllow(ThingDefOf.Drape, allow: false);
			}
			BaseGen.symbolStack.Push("interior_ancientTemple", resolveParams);
		}
		if (rp.makeWarningLetter.HasValue && rp.makeWarningLetter.Value)
		{
			string signalTag = $"ancientTempleApproached-{Find.UniqueIDsManager.GetNextSignalTagID()}";
			RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
			obj.signalTag = signalTag;
			obj.Rect = cellRect.ExpandedBy(1).ClipInsideMap(map);
			obj.destroyIfUnfogged = true;
			GenSpawn.Spawn(obj, cellRect.CenterCell, map);
			SignalAction_Letter obj2 = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
			obj2.signalTag = signalTag;
			obj2.letterDef = LetterDefOf.ThreatBig;
			obj2.letterLabelKey = "LetterLabelAncientShrineWarning";
			obj2.letterMessageKey = "AncientShrineWarning";
			GenSpawn.Spawn(obj2, cellRect.CenterCell, map);
		}
	}
}
