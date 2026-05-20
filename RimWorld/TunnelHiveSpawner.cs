using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

[StaticConstructorOnStartup]
public class TunnelHiveSpawner : GroundSpawner
{
	public bool spawnHive = true;

	public float insectsPoints;

	public bool spawnedByInfestationThingComp;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref spawnHive, "spawnHive", defaultValue: true);
		Scribe_Values.Look(ref insectsPoints, "insectsPoints", 0f);
		Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
	}

	protected override void Spawn(Map map, IntVec3 loc)
	{
		if (spawnHive)
		{
			HiveUtility.SpawnHive(loc, map, WipeMode.FullRefund, spawnInsectsImmediately: false, canSpawnHives: true, canSpawnInsects: true, dormant: false, aggressive: true, spawnJellyImmediately: true, spawnSludge: false).questTags = questTags;
		}
		if (!(insectsPoints > 0f))
		{
			return;
		}
		insectsPoints = Mathf.Max(insectsPoints, Hive.spawnablePawnKinds.Min((PawnKindDef x) => x.combatPower));
		float pointsLeft = insectsPoints;
		List<Pawn> list = new List<Pawn>();
		int num = 0;
		while (pointsLeft > 0f)
		{
			num++;
			if (num > 1000)
			{
				Log.Error("Too many iterations.");
				break;
			}
			if (!Hive.spawnablePawnKinds.Where((PawnKindDef x) => x.combatPower <= pointsLeft).TryRandomElement(out var result))
			{
				break;
			}
			Pawn pawn = PawnGenerator.GeneratePawn(result, Faction.OfInsects);
			GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(loc, map, 2), map);
			pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
			list.Add(pawn);
			pointsLeft -= result.combatPower;
			if (ModsConfig.BiotechActive)
			{
				PollutionUtility.Notify_TunnelHiveSpawnedInsect(pawn);
			}
		}
		if (list.Any())
		{
			LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_AssaultColony(Faction.OfInsects, canKidnap: true, canTimeoutOrFlee: false), map, list);
		}
	}
}
