using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class CompSpawnerPawn : ThingComp
{
	public int nextPawnSpawnTick = -1;

	public int pawnsLeftToSpawn = -1;

	public List<Pawn> spawnedPawns = new List<Pawn>();

	public bool aggressive = true;

	public bool canSpawnPawns = true;

	private PawnKindDef chosenKind;

	private static readonly SimpleCurve RateFactorForBugCount = new SimpleCurve
	{
		{ 0f, 1f },
		{ 5f, 0.5f }
	};

	private CompCanBeDormant dormancyCompCached;

	private CompProperties_SpawnerPawn Props => (CompProperties_SpawnerPawn)props;

	public Lord Lord => FindLordToJoin(parent, Props.lordJob, Props.shouldJoinParentLord, null, Props.lordJoinRadius);

	private float SpawnedPawnsPoints
	{
		get
		{
			FilterOutUnspawnedPawns();
			float num = 0f;
			for (int i = 0; i < spawnedPawns.Count; i++)
			{
				num += spawnedPawns[i].kindDef.combatPower;
			}
			return num;
		}
	}

	public bool Active
	{
		get
		{
			if (pawnsLeftToSpawn == 0)
			{
				return false;
			}
			return !Dormant;
		}
	}

	public CompCanBeDormant DormancyComp => dormancyCompCached ?? (dormancyCompCached = parent.TryGetComp<CompCanBeDormant>());

	public bool Dormant
	{
		get
		{
			if (DormancyComp != null)
			{
				return !DormancyComp.Awake;
			}
			return false;
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		if (chosenKind == null)
		{
			chosenKind = RandomPawnKindDef();
		}
		if (Props.maxPawnsToSpawn != IntRange.Zero)
		{
			pawnsLeftToSpawn = Props.maxPawnsToSpawn.RandomInRange;
		}
	}

	public static Lord FindLordToJoin(Thing spawner, Type lordJobType, bool shouldTryJoinParentLord, Func<Thing, List<Pawn>> spawnedPawnSelector = null, float lordJoinRadius = 2.1474836E+09f)
	{
		if (spawner.Spawned)
		{
			if (shouldTryJoinParentLord)
			{
				Lord lord = (spawner as Building)?.GetLord();
				if (lord != null)
				{
					return lord;
				}
			}
			if (spawnedPawnSelector == null)
			{
				spawnedPawnSelector = (Thing s) => s.TryGetComp<CompSpawnerPawn>()?.spawnedPawns;
			}
			Predicate<Pawn> hasJob = delegate(Pawn x)
			{
				Lord lord2 = x.GetLord();
				return lord2 != null && lord2.LordJob.GetType() == lordJobType;
			};
			Pawn foundPawn = null;
			RegionTraverser.BreadthFirstTraverse(spawner.GetRegion(), (Region from, Region to) => true, delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsOfDef(spawner.def);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Faction == spawner.Faction)
					{
						List<Pawn> list2 = spawnedPawnSelector(list[i]);
						if (list2 != null)
						{
							foundPawn = list2.Find(hasJob);
						}
						if (foundPawn != null && foundPawn.Position.InHorDistOf(spawner.Position, lordJoinRadius))
						{
							return true;
						}
					}
				}
				return false;
			}, 40);
			if (foundPawn != null)
			{
				return foundPawn.GetLord();
			}
		}
		return null;
	}

	public static Lord CreateNewLord(Thing byThing, bool aggressive, float defendRadius, Type lordJobType)
	{
		if (!CellFinder.TryFindRandomCellNear(byThing.Position, byThing.Map, 5, (IntVec3 c) => c.Standable(byThing.Map) && byThing.Map.reachability.CanReach(c, byThing, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)), out var result))
		{
			Log.Error("Found no place for pawns to defend " + byThing);
			result = IntVec3.Invalid;
		}
		return LordMaker.MakeNewLord(byThing.Faction, Activator.CreateInstance(lordJobType, new SpawnedPawnParams
		{
			aggressive = aggressive,
			defendRadius = defendRadius,
			defSpot = result,
			spawnerThing = byThing
		}) as LordJob, byThing.Map);
	}

	private void SpawnInitialPawns()
	{
		for (int i = 0; i < Props.initialPawnsCount; i++)
		{
			if (!TrySpawnPawn(out var _))
			{
				break;
			}
		}
		SpawnPawnsUntilPoints(Props.initialPawnsPoints);
		CalculateNextPawnSpawnTick();
	}

	public void SpawnPawnsUntilPoints(float points)
	{
		int num = 0;
		while (SpawnedPawnsPoints < points)
		{
			num++;
			if (num > 1000)
			{
				Log.Error("Too many iterations.");
				break;
			}
			if (!TrySpawnPawn(out var _))
			{
				break;
			}
		}
		CalculateNextPawnSpawnTick();
	}

	private void CalculateNextPawnSpawnTick()
	{
		CalculateNextPawnSpawnTick(Props.pawnSpawnIntervalDays.RandomInRange * 60000f);
	}

	public void CalculateNextPawnSpawnTick(float delayTicks)
	{
		if (Find.Storyteller.difficulty.enemyReproductionRateFactor > 0f)
		{
			nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(delayTicks / (RateFactorForBugCount.Evaluate(spawnedPawns.Count) * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}
		else
		{
			nextPawnSpawnTick = Find.TickManager.TicksGame + (int)delayTicks;
		}
	}

	private void FilterOutUnspawnedPawns()
	{
		for (int num = spawnedPawns.Count - 1; num >= 0; num--)
		{
			if (!spawnedPawns[num].Spawned)
			{
				spawnedPawns.RemoveAt(num);
			}
		}
	}

	private PawnKindDef RandomPawnKindDef()
	{
		float curPoints = SpawnedPawnsPoints;
		IEnumerable<PawnKindDef> source = Props.spawnablePawnKinds;
		if (Props.maxSpawnedPawnsPoints > -1f)
		{
			source = source.Where((PawnKindDef x) => curPoints + x.combatPower <= Props.maxSpawnedPawnsPoints);
		}
		if (source.TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	private bool TrySpawnPawn(out Pawn pawn)
	{
		if (!canSpawnPawns)
		{
			pawn = null;
			return false;
		}
		if (!Props.chooseSingleTypeToSpawn)
		{
			chosenKind = RandomPawnKindDef();
		}
		if (chosenKind == null)
		{
			pawn = null;
			return false;
		}
		PawnGenerationRequest request = new PawnGenerationRequest(chosenKind, parent.Faction);
		if (chosenKind.RaceProps.IsMechanoid)
		{
			request.AllowedDevelopmentalStages = DevelopmentalStage.Newborn;
		}
		else
		{
			int index = chosenKind.lifeStages.Count - 1;
			request.FixedBiologicalAge = chosenKind.race.race.lifeStageAges[index].minAge;
		}
		pawn = PawnGenerator.GeneratePawn(request);
		spawnedPawns.Add(pawn);
		GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, Props.pawnSpawnRadius), parent.Map);
		(Lord ?? CreateNewLord(parent, aggressive, Props.defendRadius, Props.lordJob)).AddPawn(pawn);
		Props.spawnSound?.PlayOneShot(parent);
		if (pawnsLeftToSpawn > 0)
		{
			pawnsLeftToSpawn--;
		}
		SendMessage();
		return true;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && Active && nextPawnSpawnTick == -1)
		{
			SpawnInitialPawns();
		}
	}

	public override void CompTick()
	{
		if (Active && parent.Spawned && nextPawnSpawnTick == -1)
		{
			SpawnInitialPawns();
		}
		if (!parent.Spawned)
		{
			return;
		}
		FilterOutUnspawnedPawns();
		if (Active && Find.TickManager.TicksGame >= nextPawnSpawnTick)
		{
			if ((Props.maxSpawnedPawnsPoints < 0f || SpawnedPawnsPoints < Props.maxSpawnedPawnsPoints) && Find.Storyteller.difficulty.enemyReproductionRateFactor > 0f && TrySpawnPawn(out var pawn) && pawn.caller != null)
			{
				pawn.caller.DoCall();
			}
			CalculateNextPawnSpawnTick();
		}
	}

	public void SendMessage()
	{
		if (!Props.spawnMessageKey.NullOrEmpty() && MessagesRepeatAvoider.MessageShowAllowed(Props.spawnMessageKey, 0.1f))
		{
			Messages.Message(Props.spawnMessageKey.Translate(), parent, MessageTypeDefOf.NegativeEvent);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Spawn pawn";
			command_Action.icon = TexCommand.ReleaseAnimals;
			command_Action.action = delegate
			{
				TrySpawnPawn(out var _);
			};
			yield return command_Action;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!Props.showNextSpawnInInspect || nextPawnSpawnTick <= 0 || chosenKind == null)
		{
			return null;
		}
		if (pawnsLeftToSpawn == 0 && !Props.noPawnsLeftToSpawnKey.NullOrEmpty())
		{
			return Props.noPawnsLeftToSpawnKey.Translate();
		}
		string text;
		if (!Dormant)
		{
			text = (Props.nextSpawnInspectStringKey ?? "SpawningNextPawnIn").Translate(chosenKind.LabelCap, (nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToDays());
		}
		else
		{
			if (Props.nextSpawnInspectStringKeyDormant == null)
			{
				return null;
			}
			text = Props.nextSpawnInspectStringKeyDormant.Translate() + ": " + chosenKind.LabelCap;
		}
		if (pawnsLeftToSpawn > 0 && !Props.pawnsLeftToSpawnKey.NullOrEmpty())
		{
			text = string.Concat(text, "\n" + Props.pawnsLeftToSpawnKey.Translate() + ": ", pawnsLeftToSpawn.ToString());
		}
		return text;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref nextPawnSpawnTick, "nextPawnSpawnTick", 0);
		Scribe_Values.Look(ref pawnsLeftToSpawn, "pawnsLeftToSpawn", -1);
		Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
		Scribe_Values.Look(ref aggressive, "aggressive", defaultValue: false);
		Scribe_Values.Look(ref canSpawnPawns, "canSpawnPawns", defaultValue: true);
		Scribe_Defs.Look(ref chosenKind, "chosenKind");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			spawnedPawns.RemoveAll((Pawn x) => x == null);
			if (pawnsLeftToSpawn == -1 && Props.maxPawnsToSpawn != IntRange.Zero)
			{
				pawnsLeftToSpawn = Props.maxPawnsToSpawn.RandomInRange;
			}
		}
	}
}
