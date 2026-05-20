using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompFleshmassHeart : CompGrowsFleshmassTendrils
	{
		private static readonly IntRange FleshbeastSpawnIntervalRange = new IntRange(60, 240);

		private const int MinDistanceBetweenSpecialMass = 4;

		private const int TargetNerveBundleCount = 3;

		private const int MaxNerveBundlesPerGrowthCycle = 1;

		private const int MaxSpittersPerGrowthCycle = 1;

		private const float InitialFleshbeastPointMultiplier = 0.75f;

		private const int SmallGrowthMTBHours = 2;

		private static readonly SimpleCurve TargetSpittersCountByThreatPointsCurve = new SimpleCurve
		{
			new CurvePoint(200f, 1f),
			new CurvePoint(800f, 2f),
			new CurvePoint(3000f, 4f),
			new CurvePoint(10000f, 7f)
		};

		public float threatPoints;

		private float fleshbeastPoints;

		private int nextFleshbeast = -99999;

		private int lastGrowthCycle = -99999;

		private int nerveBundlesThisGrowthCycle;

		private int spittersThisGrowthCycle;

		private List<Thing> nerveBundles = new List<Thing>();

		private List<Thing> spitters = new List<Thing>();

		public Building_FleshmassHeart Heart => parent as Building_FleshmassHeart;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref threatPoints, "threatPoints", 0f);
			Scribe_Values.Look(ref fleshbeastPoints, "fleshbeastPoints", 0f);
			Scribe_Values.Look(ref nextFleshbeast, "nextFleshbeast", 0);
			Scribe_Values.Look(ref lastGrowthCycle, "lastGrowthCycle", 0);
			Scribe_Values.Look(ref nerveBundlesThisGrowthCycle, "nerveBundlesThisGrowthCycle", 0);
			Scribe_Values.Look(ref spittersThisGrowthCycle, "spittersThisGrowthCycle", 0);
			Scribe_Collections.Look(ref nerveBundles, "nerveBundles", LookMode.Reference);
			Scribe_Collections.Look(ref spitters, "spitters", LookMode.Reference);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				if (threatPoints <= 0f)
				{
					threatPoints = StorytellerUtility.DefaultThreatPointsNow(base.Map);
				}
				fleshbeastPoints = Mathf.RoundToInt(threatPoints * 0.75f);
				lastGrowthCycle = Find.TickManager.TicksGame;
			}
			base.PostSpawnSetup(respawningAfterLoad);
		}

		public override void CompTick()
		{
			int num = Find.TickManager.TicksGame - lastGrowthCycle;
			if (base.Props.maxGrowthCycleHours > 0 && num > base.Props.maxGrowthCycleHours * 2500)
			{
				StartGrowthCycle();
			}
			else if (base.Props.mtbGrowthCycleHours > 0 && num > base.Props.minGrowthCycleHours * 2500 && Rand.MTBEventOccurs(base.Props.mtbGrowthCycleHours, 2500f, 1f))
			{
				StartGrowthCycle();
			}
			if (Rand.MTBEventOccurs(2f, 2500f, 1f))
			{
				StartMiniGrowth();
			}
			if (base.Props.spawnFleshbeasts && fleshbeastPoints > 0f && Find.TickManager.TicksGame >= nextFleshbeast)
			{
				fleshbeastPoints -= SpawnFleshBeast();
				nextFleshbeast = Find.TickManager.TicksGame + FleshbeastSpawnIntervalRange.RandomInRange;
			}
			base.CompTick();
		}

		private void StartGrowthCycle()
		{
			float num = StorytellerUtility.DefaultThreatPointsNow(base.Map);
			float num2 = FleshbeastUtility.ExistingFleshBeastThreat(base.Map);
			growthPoints += Mathf.RoundToInt(base.Props.growthCyclePointsByThreat?.Evaluate(num) ?? 0f);
			fleshbeastPoints += Mathf.Clamp(base.Props.fleshbeastPointsByThreat?.Evaluate(num) ?? 0f, 0f, num - num2);
			lastGrowthCycle = Find.TickManager.TicksGame;
			nerveBundlesThisGrowthCycle = 0;
			spittersThisGrowthCycle = 0;
		}

		private void StartMiniGrowth()
		{
			float num = StorytellerUtility.DefaultThreatPointsNow(base.Map);
			growthPoints += Mathf.RoundToInt(base.Props.growthCyclePointsByThreat?.Evaluate(num / 5f) ?? 0f);
		}

		protected override void Grow()
		{
			if (nerveBundles.Count < 3 && nerveBundlesThisGrowthCycle < 1)
			{
				Thing thing = TrySpawnSpecialMass(ThingDefOf.NerveBundle);
				if (thing != null)
				{
					Messages.Message("NerveBundleGrewMessage".Translate(), thing, MessageTypeDefOf.NeutralEvent);
					nerveBundles.Add(thing);
					nerveBundlesThisGrowthCycle++;
					return;
				}
			}
			if ((float)spitters.Count < TargetSpittersCountByThreatPointsCurve.Evaluate(threatPoints) && spittersThisGrowthCycle < 1)
			{
				Thing thing2 = TrySpawnSpecialMass(ThingDefOf.FleshmassSpitter);
				if (thing2 != null)
				{
					spitters.Add(thing2);
					spittersThisGrowthCycle++;
					return;
				}
			}
			base.Grow();
		}

		private Thing TrySpawnSpecialMass(ThingDef def)
		{
			if (base.ContiguousFleshmass.EnumerableNullOrEmpty())
			{
				return null;
			}
			for (int i = 0; i < 10; i++)
			{
				IntVec3 intVec = base.ContiguousFleshmass.RandomElement();
				if (CanSpawnSpecialMass(def, intVec))
				{
					Thing thing = GenSpawn.Spawn(def, intVec, base.Map);
					thing.TryGetComp<CompFleshmassHeartChild>().heart = Heart;
					thing.SetFaction(Heart.Faction);
					return thing;
				}
			}
			return null;
		}

		private bool CanSpawnSpecialMass(ThingDef def, IntVec3 pos)
		{
			if (!GenAdj.OccupiedRect(pos, Rot4.North, def.size).InBounds(parent.Map))
			{
				return false;
			}
			if (parent.Position.InHorDistOf(pos, 4f))
			{
				return false;
			}
			foreach (Thing nerveBundle in nerveBundles)
			{
				if (nerveBundle.Position.InHorDistOf(pos, 4f))
				{
					return false;
				}
			}
			foreach (Thing spitter in spitters)
			{
				if (spitter.Position.InHorDistOf(pos, 4f))
				{
					return false;
				}
			}
			foreach (IntVec3 item in GenAdj.CellsOccupiedBy(pos, Rot4.North, def.size))
			{
				if (item.GetEdifice(parent.Map)?.def != ThingDefOf.Fleshmass_Active)
				{
					return false;
				}
				if (def == ThingDefOf.FleshmassSpitter && item.Roofed(parent.Map))
				{
					return false;
				}
			}
			return true;
		}

		private float SpawnFleshBeast()
		{
			PawnKindDef kind = PickFleshBeastToSpawn();
			Faction ofEntities = Faction.OfEntities;
			float? fixedBiologicalAge = 0f;
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, ofEntities, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge));
			if (parent is Building_FleshmassHeart building_FleshmassHeart)
			{
				building_FleshmassHeart.DefendHeartLord.AddPawn(pawn);
			}
			IntVec3 intVec = FindValidSpawnPoint();
			IntVec3 intVec2 = CellFinder.StandableCellNear(intVec, base.Map, 5f);
			if (intVec2 == IntVec3.Invalid)
			{
				return pawn.kindDef.combatPower;
			}
			GenSpawn.Spawn(pawn, intVec2, base.Map, WipeMode.FullRefund);
			pawn.rotationTracker.FaceCell(intVec2);
			EffecterDefOf.MeatExplosionExtraLarge.Spawn(intVec, base.Map).Cleanup();
			return pawn.kindDef.combatPower;
		}

		private PawnKindDef PickFleshBeastToSpawn()
		{
			PawnKindDef pawnKindDef = FleshbeastUtility.AllFleshbeasts.RandomElement();
			if (fleshbeastPoints < pawnKindDef.combatPower)
			{
				return PawnKindDefOf.Fingerspike;
			}
			return pawnKindDef;
		}

		private IntVec3 FindValidSpawnPoint()
		{
			if (contiguousFleshmass.Empty())
			{
				return parent.OccupiedRect().Cells.RandomElement();
			}
			for (int i = 0; i < 20; i++)
			{
				IntVec3 intVec = contiguousFleshmass.RandomElement();
				if (AdjacentFleshmass(intVec) < 4)
				{
					return intVec;
				}
			}
			contiguousFleshmass.Shuffle();
			foreach (IntVec3 item in contiguousFleshmass)
			{
				if (AdjacentFleshmass(item) < 4)
				{
					return item;
				}
			}
			return IntVec3.Invalid;
		}

		public void Notify_ChildDied(Thing thing)
		{
			if (thing.def == ThingDefOf.NerveBundle)
			{
				nerveBundles.Remove(thing);
			}
			if (thing.def == ThingDefOf.FleshmassSpitter)
			{
				spitters.Remove(thing);
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			foreach (Thing nerveBundle in nerveBundles)
			{
				nerveBundle.TryGetComp<CompFleshmassHeartChild>()?.Notify_HeartDied();
			}
			foreach (Thing spitter in spitters)
			{
				spitter.TryGetComp<CompFleshmassHeartChild>()?.Notify_HeartDied();
			}
		}

		public override void PostSwapMap()
		{
			base.PostSwapMap();
			nerveBundles.RemoveWhere((Thing nb) => !nb.Spawned);
			spitters.RemoveWhere((Thing s) => !s.Spawned);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Start growth cycle",
					defaultDesc = "Start growth cycle",
					action = StartGrowthCycle
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Add 100 growth points",
					defaultDesc = "Add 100 growth points",
					action = delegate
					{
						growthPoints += 100;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Add 200 fleshbeast points",
					defaultDesc = "Add 200 fleshbeast points",
					action = delegate
					{
						fleshbeastPoints += 200f;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Do Tachycardiac Overload",
					defaultDesc = "Start Tachycardiac overload",
					action = delegate
					{
						(parent as Building_FleshmassHeart)?.StartTachycardiacOverload();
					}
				};
			}
		}
	}
}
