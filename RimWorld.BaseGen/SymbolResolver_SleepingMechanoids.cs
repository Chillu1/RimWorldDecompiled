using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_SleepingMechanoids : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (base.CanResolve(rp) && rp.threatPoints.HasValue)
		{
			return Faction.OfMechanoids != null;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		LordJob_SleepThenAssaultColony lordJob = new LordJob_SleepThenAssaultColony(Faction.OfMechanoids, rp.sendWokenUpMessage ?? true);
		Lord lord = LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob, BaseGen.globalSettings.map);
		PawnKindDef[] array = PawnUtility.GetCombatPawnKindsForPoints(MechClusterGenerator.MechKindSuitableForCluster, rp.threatPoints.Value, (PawnKindDef pk) => 1f / pk.combatPower).ToArray();
		float num = (float)Math.Min(rp.rect.Width, rp.rect.Height) / 2f;
		Vector3 v = IntVec3.North.ToVector3();
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			ResolveParams resolveParams = rp;
			IntVec3 spawnCell = IntVec3.Invalid;
			float angle = 360f / (float)array.Length * (float)num2;
			Vector3 vect = v.RotatedBy(angle) * num;
			if (CellFinder.TryFindRandomCellNear(rp.rect.CenterCell + vect.ToIntVec3(), BaseGen.globalSettings.map, 10, (IntVec3 c) => !usedRects.Any((CellRect r) => r.Contains(c)), out var result) && SiteGenStepUtility.TryFindSpawnCellAroundOrNear(rp.rect, result, BaseGen.globalSettings.map, out spawnCell))
			{
				resolveParams.rect = CellRect.CenteredOn(spawnCell, 1, 1);
				resolveParams.singlePawnKindDef = array[num2];
				resolveParams.singlePawnLord = lord;
				resolveParams.faction = Faction.OfMechanoids;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
		if (array.Length != 0 && rp.sleepingMechanoidsWakeupSignalTag != null)
		{
			SignalAction_DormancyWakeUp obj = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
			obj.signalTag = rp.sleepingMechanoidsWakeupSignalTag;
			obj.lord = lord;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
