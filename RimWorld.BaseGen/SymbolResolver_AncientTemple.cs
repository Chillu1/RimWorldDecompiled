using System.Linq;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientTemple : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			CellRect cellRect = CellRect.Empty;
			RimWorld.SketchGen.ResolveParams parms = default(RimWorld.SketchGen.ResolveParams);
			parms.sketch = new Sketch();
			parms.monumentOpen = false;
			parms.monumentSize = new IntVec2(rp.rect.Width, rp.rect.Height);
			parms.allowMonumentDoors = false;
			parms.allowWood = false;
			parms.allowFlammableWalls = false;
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
			sketch.Spawn(map, rp.rect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, clearEdificeWhereFloor: true, null, dormant: false, buildRoofsInstantly: true);
			CellRect rect = SketchGenUtility.FindBiggestRect(sketch, (IntVec3 x) => sketch.TerrainAt(x) != null && !sketch.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall)).MovedBy(rp.rect.CenterCell);
			for (int i = 0; i < sketch.Things.Count; i++)
			{
				if (sketch.Things[i].def == ThingDefOf.Wall)
				{
					IntVec3 c = sketch.Things[i].pos + rp.rect.CenterCell;
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
				int nextSignalTagID = Find.UniqueIDsManager.GetNextSignalTagID();
				string signalTag = "ancientTempleApproached-" + nextSignalTagID;
				SignalAction_Letter obj = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
				obj.signalTag = signalTag;
				obj.letter = LetterMaker.MakeLetter("LetterLabelAncientShrineWarning".Translate(), "AncientShrineWarning".Translate(), LetterDefOf.ThreatBig, new TargetInfo(cellRect.CenterCell, map));
				GenSpawn.Spawn(obj, cellRect.CenterCell, map);
				RectTrigger obj2 = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
				obj2.signalTag = signalTag;
				obj2.Rect = cellRect.ExpandedBy(1).ClipInsideMap(map);
				obj2.destroyIfUnfogged = true;
				GenSpawn.Spawn(obj2, cellRect.CenterCell, map);
			}
		}
	}
}
