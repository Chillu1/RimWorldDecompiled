using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Archonexus : SymbolResolver
{
	private const int SuperstructureDistance = 11;

	private const int MinorSuperstructureDistance = 28;

	private const int MinorSuperstructureCount = 9;

	private static List<CellRect> MinorSupersturctureSites = new List<CellRect>();

	private static List<CellRect> MajorSupersturctureSites = new List<CellRect>();

	private const string MechanoidsWakeUpSignalPrefix = "ArchonexusMechanoidsWakeUp";

	public override void Resolve(ResolveParams rp)
	{
		rp.floorDef = TerrainDefOf.Sandstone_Smooth;
		rp.chanceToSkipFloor = 0.05f;
		MinorSupersturctureSites.Clear();
		MajorSupersturctureSites.Clear();
		if (rp.threatPoints > 0f && Faction.OfMechanoids != null)
		{
			string text = "ArchonexusMechanoidsWakeUp" + Find.UniqueIDsManager.GetNextSignalTagID();
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect.ExpandedBy(5);
			resolveParams.rectTriggerSignalTag = text;
			resolveParams.threatPoints = rp.threatPoints;
			BaseGen.symbolStack.Push("rectTrigger", resolveParams);
			ResolveParams resolveParams2 = rp;
			resolveParams2.sleepingMechanoidsWakeupSignalTag = text;
			resolveParams2.threatPoints = rp.threatPoints;
			BaseGen.symbolStack.Push("sleepingMechanoids", resolveParams2);
			ResolveParams resolveParams3 = rp;
			resolveParams3.sound = SoundDefOf.ArchonexusThreatsAwakened_Alarm;
			resolveParams3.soundOneShotActionSignalTag = text;
			BaseGen.symbolStack.Push("soundOneShotAction", resolveParams3);
		}
		ResolveParams resolveParams4 = rp;
		resolveParams4.dessicatedCorpseDensityRange = new FloatRange(0.003f, 0.006f);
		BaseGen.symbolStack.Push("desiccatedCorpses", resolveParams4);
		Vector3 v = IntVec3.North.ToVector3();
		ThingDef archonexusCore = ThingDefOf.ArchonexusCore;
		ResolveParams resolveParams5 = rp;
		resolveParams5.rect = CellRect.CenteredOn(rp.rect.CenterCell, archonexusCore.size.x, archonexusCore.size.z);
		resolveParams5.singleThingDef = ThingDefOf.ArchonexusCore;
		BaseGen.symbolStack.Push("thing", resolveParams5);
		resolveParams5.rect = resolveParams5.rect.ExpandedBy(1);
		BaseGen.symbolStack.Push("floor", resolveParams5);
		BaseGen.symbolStack.Push("clear", resolveParams5);
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = GenAdj.DiagonalDirections[i];
			CellRect cellRect = CellRect.CenteredOn(rp.rect.CenterCell + intVec * 11, ThingDefOf.MajorArchotechStructure.size.x, ThingDefOf.MajorArchotechStructure.size.z);
			ResolveParams resolveParams6 = rp;
			Thing thing = ThingMaker.MakeThing(ThingDefOf.MajorArchotechStructure);
			thing.TryGetComp<CompSpawnImmortalSubplantsAround>()?.Disable();
			resolveParams6.singleThingToSpawn = thing;
			resolveParams6.rect = cellRect;
			BaseGen.symbolStack.Push("thing", resolveParams6);
			ResolveParams resolveParams7 = resolveParams6;
			resolveParams7.rect = resolveParams6.rect.ExpandedBy(1);
			resolveParams7.clearRoof = true;
			BaseGen.symbolStack.Push("floor", resolveParams7);
			BaseGen.symbolStack.Push("clear", resolveParams7);
			MajorSupersturctureSites.Add(cellRect);
		}
		float num = 40f;
		for (int j = 0; j < 9; j++)
		{
			float angle = (float)j * num;
			Vector3 vect = v.RotatedBy(angle) * 28f;
			CellRect cellRect2 = CellRect.CenteredOn(rp.rect.CenterCell + vect.ToIntVec3(), ThingDefOf.ArchotechTower.size.x, ThingDefOf.ArchotechTower.size.z);
			ResolveParams resolveParams8 = rp;
			resolveParams8.singleThingDef = ThingDefOf.ArchotechTower;
			resolveParams8.rect = cellRect2;
			BaseGen.symbolStack.Push("thing", resolveParams8);
			ResolveParams resolveParams9 = resolveParams8;
			resolveParams9.rect = resolveParams8.rect.ExpandedBy(1);
			resolveParams9.clearRoof = true;
			BaseGen.symbolStack.Push("floor", resolveParams9);
			BaseGen.symbolStack.Push("clear", resolveParams9);
			MinorSupersturctureSites.Add(cellRect2);
		}
		rp.chanceToSkipFloor = 0.95f;
		BaseGen.symbolStack.Push("floor", rp);
		for (int k = 0; k < MajorSupersturctureSites.Count; k++)
		{
			BaseGenUtility.DoPathwayBetween(resolveParams5.rect.CenterCell, MajorSupersturctureSites[k].CenterCell, rp.floorDef);
		}
		for (int l = 0; l < MinorSupersturctureSites.Count; l++)
		{
			CellRect current = MinorSupersturctureSites[l];
			int index = GenMath.PositiveMod(l - 1, MinorSupersturctureSites.Count);
			BaseGenUtility.DoPathwayBetween(MinorSupersturctureSites[index].CenterCell, current.CenterCell, rp.floorDef);
			BaseGenUtility.DoPathwayBetween(MajorSupersturctureSites.MinBy((CellRect c) => c.CenterCell.DistanceToSquared(current.CenterCell)).CenterCell, current.CenterCell, rp.floorDef);
		}
		foreach (IntVec3 item in rp.rect)
		{
			if (!(item.DistanceTo(resolveParams5.rect.CenterCell) > 28f))
			{
				Plant plant = item.GetPlant(BaseGen.globalSettings.map);
				if (plant != null && plant.def.destroyable)
				{
					plant.Destroy();
				}
				Building edifice = item.GetEdifice(BaseGen.globalSettings.map);
				if (edifice != null && edifice.def.destroyable)
				{
					edifice.Destroy();
				}
				BaseGen.globalSettings.map.roofGrid.SetRoof(item, null);
			}
		}
		BaseGenUtility.DoPathwayBetween(resolveParams5.rect.CenterCell, resolveParams5.rect.CenterCell + IntVec3.South * 25, rp.floorDef);
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		foreach (CellRect minorSupersturctureSite in MinorSupersturctureSites)
		{
			orGenerateVar.Add(minorSupersturctureSite);
		}
		foreach (CellRect majorSupersturctureSite in MajorSupersturctureSites)
		{
			orGenerateVar.Add(majorSupersturctureSite);
		}
		MinorSupersturctureSites.Clear();
		MajorSupersturctureSites.Clear();
	}
}
