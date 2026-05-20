using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class CompPawnSpawnOnWakeup : ThingComp
{
	public float points;

	public List<Pawn> spawnedPawns = new List<Pawn>();

	private CompProperties_PawnSpawnOnWakeup Props => (CompProperties_PawnSpawnOnWakeup)props;

	public bool CanSpawn => points > 0f;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		points = Props.points.RandomInRange;
	}

	public override void CompTick()
	{
		for (int num = spawnedPawns.Count - 1; num >= 0; num--)
		{
			if (!spawnedPawns[num].Spawned)
			{
				spawnedPawns.RemoveAt(num);
			}
		}
		if (points != 0f)
		{
			bool flag = parent.GetComp<CompCanBeDormant>()?.Awake ?? true;
			if (points > 0f && flag && parent.Spawned)
			{
				Spawn();
			}
		}
	}

	private IntVec3 GetSpawnPosition()
	{
		if (!Props.dropInPods)
		{
			return parent.Position;
		}
		Predicate<IntVec3> validator = delegate(IntVec3 c)
		{
			if (!DropCellFinder.IsGoodDropSpot(c, parent.MapHeld, allowFogged: false, canRoofPunch: true))
			{
				return false;
			}
			float num = c.DistanceTo(parent.Position);
			return num >= (float)Props.pawnSpawnRadius.min && num <= (float)Props.pawnSpawnRadius.max;
		};
		if (CellFinder.TryFindRandomCellNear(parent.Position, parent.MapHeld, Props.pawnSpawnRadius.max, validator, out var result))
		{
			return result;
		}
		return IntVec3.Invalid;
	}

	private List<Thing> GeneratePawns()
	{
		List<Thing> list = new List<Thing>();
		float pointsLeft;
		PawnKindDef result;
		for (pointsLeft = points; pointsLeft > 0f && Props.spawnablePawnKinds.Where((PawnKindDef p) => p.combatPower <= pointsLeft).TryRandomElement(out result); pointsLeft -= result.combatPower)
		{
			int index = result.lifeStages.Count - 1;
			PawnKindDef kind = result;
			Faction faction = parent.Faction;
			float? fixedBiologicalAge = result.race.race.lifeStageAges[index].minAge;
			list.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge)));
		}
		points = 0f;
		return list;
	}

	private void Spawn()
	{
		Lord lord = CompSpawnerPawn.FindLordToJoin(parent, Props.lordJob, Props.shouldJoinParentLord, (Thing spawner) => spawner.TryGetComp<CompPawnSpawnOnWakeup>()?.spawnedPawns);
		if (lord == null)
		{
			lord = CompSpawnerPawn.CreateNewLord(parent, Props.aggressive, Props.defendRadius, Props.lordJob);
		}
		IntVec3 spawnPosition = GetSpawnPosition();
		if (!spawnPosition.IsValid)
		{
			return;
		}
		List<Thing> list = GeneratePawns();
		if (Props.dropInPods)
		{
			DropPodUtility.DropThingsNear(spawnPosition, parent.MapHeld, list, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: true, allowFogged: true, parent.Faction);
		}
		List<IntVec3> occupiedCells = new List<IntVec3>();
		foreach (Thing item in list)
		{
			if (!Props.dropInPods)
			{
				IntVec3 intVec = CellFinder.RandomClosewalkCellNear(spawnPosition, parent.Map, Props.pawnSpawnRadius.RandomInRange, (IntVec3 c) => !occupiedCells.Contains(c));
				if (!intVec.IsValid)
				{
					intVec = CellFinder.RandomClosewalkCellNear(spawnPosition, parent.Map, Props.pawnSpawnRadius.RandomInRange);
				}
				GenSpawn.Spawn(item, intVec, parent.Map);
				occupiedCells.Add(intVec);
			}
			lord.AddPawn((Pawn)item);
			spawnedPawns.Add((Pawn)item);
			item.TryGetComp<CompCanBeDormant>()?.WakeUp();
			if (Props.mentalState != null)
			{
				((Pawn)item).mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.CocoonDisturbed);
			}
		}
		if (Props.spawnEffecter != null)
		{
			Effecter effecter = new Effecter(Props.spawnEffecter);
			effecter.Trigger(parent, TargetInfo.Invalid);
			effecter.Cleanup();
		}
		if (Props.spawnSound != null)
		{
			Props.spawnSound.PlayOneShot(parent);
		}
		if (Props.activatedMessageKey != null)
		{
			Messages.Message(Props.activatedMessageKey.Translate(), spawnedPawns, MessageTypeDefOf.ThreatBig);
		}
		if (Props.destroyAfterSpawn && !parent.Destroyed)
		{
			parent.Destroy();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Prefs.DevMode && DebugSettings.godMode)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Spawn";
			command_Action.action = Spawn;
			yield return command_Action;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref points, "points", 0f);
		Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			spawnedPawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
