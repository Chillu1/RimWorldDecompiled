using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompObelisk_Mutator : CompObelisk_ExplodingSpawner
{
	private const int TreeSearchStepSize = 20;

	private static readonly IntRange TunnelDelayTicks = new IntRange(60, 120);

	private static readonly List<Plant> TmpTrees = new List<Plant>();

	protected override IntRange SpawnIntervalTicks => new IntRange(30, 90);

	public override void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false)
	{
		int num = (triggeredByPlayer ? Rand.RangeInclusive(0, 3) : Rand.RangeInclusive(0, 5));
		switch (num)
		{
		case 0:
			DoMutation(interactor, HediffDefOf.Tentacle, "ObeliskTentacleLetterLabel", "ObeliskTentacleLetter", "MutatorObeliskFailedArmLetterLabel", "MutatorObeliskFailedArmLetter");
			break;
		case 1:
			DoMutation(interactor, HediffDefOf.FleshWhip, "ObeliskFleshWhipLetterLabel", "ObeliskFleshWhipLetter", "MutatorObeliskFailedArmLetterLabel", "MutatorObeliskFailedArmLetter");
			break;
		case 2:
			DoMutation(interactor, HediffDefOf.FleshmassLung, "ObeliskFleshmassLungLetterLabel", "ObeliskFleshmassLungLetter", "MutatorObeliskFailedLungLetterLabel", "MutatorObeliskFailedLungLetter");
			break;
		case 3:
			DoMutation(interactor, HediffDefOf.FleshmassStomach, "ObeliskFleshmassStomachLetterLabel", "ObeliskFleshmassStomachLetter", "MutatorObeliskFailedStomachLetterLabel", "MutatorObeliskFailedStomachLetter");
			break;
		case 4:
		{
			if (TryMutatingRandomAnimal(out var mutatedAnimal, out var resultBeast))
			{
				lastInteractionEffectTick = Find.TickManager.TicksGame;
				Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("ObeliskAnimalMutationLetterLabel".Translate(), "ObeliskAnimalMutationLetter".Translate(interactor.Named("PAWN"), mutatedAnimal.Named("ANIMAL")), LetterDefOf.ThreatSmall, resultBeast));
			}
			break;
		}
		case 5:
		{
			if (TryMutateRandomTree(out var tree))
			{
				lastInteractionEffectTick = Find.TickManager.TicksGame;
				Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("ObeliskTreeMutationLetterLabel".Translate(), "ObeliskTreeMutationLetter".Translate(interactor.Named("PAWN")), LetterDefOf.NeutralEvent, tree));
			}
			break;
		}
		default:
			Log.Error("Unhandled outcome in mutator obelisk trigger interaction " + num);
			break;
		}
	}

	private void DoMutation(Pawn pawn, HediffDef mutation, string letterLabel, string letter, string failedLabel, string failedLetter)
	{
		if (FleshbeastUtility.TryGiveMutation(pawn, mutation))
		{
			lastInteractionEffectTick = Find.TickManager.TicksGame;
			Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(letterLabel.Translate(), letter.Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent, pawn));
		}
		else
		{
			Find.LetterStack.ReceiveLetter(failedLabel.Translate(), failedLetter.Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent, pawn);
		}
	}

	public override void OnActivityActivated()
	{
		base.OnActivityActivated();
		Find.LetterStack.ReceiveLetter("MutatorObeliskLetterLabel".Translate(), "MutatorObeliskLetter".Translate(), LetterDefOf.ThreatBig, parent);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (activated && !base.ActivityComp.Deactivated && explodeTick <= 0 && Find.TickManager.TicksGame >= nextSpawnTick && warmupComplete)
		{
			nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks.RandomInRange;
			IntVec3 result;
			if (TryMutatingRandomAnimal(out var mutatedAnimal, out var resultBeast))
			{
				pointsRemaining -= resultBeast.kindDef.combatPower;
				base.Lord.AddPawn(resultBeast);
				Messages.Message("MutatorObeliskMutated".Translate(mutatedAnimal.Named("PAWN")), resultBeast, MessageTypeDefOf.ThreatSmall);
			}
			else if (CellFinder.TryFindRandomCell(parent.Map, IsValidSpawnCell, out result))
			{
				Pawn pawn = FleshbeastUtility.SpawnFleshbeastFromGround(result, parent.Map, TunnelDelayTicks);
				base.Lord.AddPawn(pawn);
				EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
				pointsRemaining -= pawn.kindDef.combatPower;
				Messages.Message("MutatorObeliskSpawned".Translate(), pawn, MessageTypeDefOf.ThreatSmall);
			}
			if (pointsRemaining <= 0f)
			{
				PrepareExplosion();
			}
		}
	}

	private bool TryMutatingRandomAnimal(out Pawn mutatedAnimal, out Pawn resultBeast)
	{
		resultBeast = null;
		mutatedAnimal = null;
		parent.Map.mapPawns.AllPawnsSpawned.Where((Pawn pawn2) => pawn2.Faction == null && pawn2.IsAnimal && !pawn2.Position.Fogged(parent.Map)).TryRandomElement(out var result);
		if (result == null)
		{
			return false;
		}
		mutatedAnimal = result;
		Pawn pawn = FleshbeastUtility.SpawnFleshbeastFromPawn(result, false, false);
		resultBeast = pawn;
		EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
		return true;
	}

	private bool TryMutateRandomTree(out HarbingerTree tree)
	{
		tree = null;
		TmpTrees.Clear();
		for (int i = 0; i < 6; i++)
		{
			int num = 20 * i;
			int num2 = Mathf.Max(20 * (i - 1) - 1, 1);
			foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, num2, num))
			{
				if (item.InBounds(parent.Map) && item.TryGetFirstThing<Plant>(parent.Map, out var thing) && thing.def.plant.IsTree && thing.def.plant.treeCategory != TreeCategory.Super && thing.def != ThingDefOf.Plant_Fibercorn)
				{
					TmpTrees.Add(thing);
				}
			}
			if (TmpTrees.Any())
			{
				break;
			}
		}
		if (TmpTrees.Empty())
		{
			return false;
		}
		Plant plant = TmpTrees.RandomElement();
		TmpTrees.Clear();
		tree = (HarbingerTree)ThingMaker.MakeThing(ThingDefOf.Plant_TreeHarbinger);
		GenSpawn.Spawn(tree, plant.Position, plant.Map);
		plant.Destroy();
		EffecterDefOf.ObeliskSpark.Spawn(tree.Position, tree.Map).Cleanup();
		return true;
	}

	private bool IsValidSpawnCell(IntVec3 c)
	{
		if (c.Standable(parent.Map) && !c.GetTerrain(parent.Map).IsWater)
		{
			return !c.Fogged(parent.Map);
		}
		return false;
	}
}
