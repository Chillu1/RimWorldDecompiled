using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class GroundSpawner : ThingWithComps
{
	protected int secondarySpawnTick;

	protected float dustMoteSpawnMTB = 0.2f;

	protected float filthSpawnMTB = 0.3f;

	protected float filthSpawnRadius = 3f;

	private Sustainer sustainer;

	private Effecter sustainedFx;

	private static readonly IntRange DefaultSpawnDelay = new IntRange(1560, 1800);

	private static List<ThingDef> filthTypes = new List<ThingDef>();

	protected virtual IntRange ResultSpawnDelay => DefaultSpawnDelay;

	protected virtual SoundDef SustainerSound => SoundDefOf.Tunnel;

	public int TicksUntilSpawn => secondarySpawnTick - Find.TickManager.TicksGame;

	protected virtual bool SpawnRubble => true;

	public static void ResetStaticData()
	{
		filthTypes.Clear();
		filthTypes.Add(ThingDefOf.Filth_Dirt);
		filthTypes.Add(ThingDefOf.Filth_Dirt);
		filthTypes.Add(ThingDefOf.Filth_Dirt);
		filthTypes.Add(ThingDefOf.Filth_RubbleRock);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref secondarySpawnTick, "secondarySpawnTick", 0);
		Scribe_Values.Look(ref dustMoteSpawnMTB, "dustMoteSpawnMTB", 0.2f);
		Scribe_Values.Look(ref filthSpawnMTB, "filthSpawnMTB", 0.3f);
		Scribe_Values.Look(ref filthSpawnRadius, "filthSpawnRadius", 3f);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange;
		}
		CreateFX();
	}

	protected override void Tick()
	{
		base.Tick();
		if (!base.Spawned)
		{
			return;
		}
		Sustainer sustainer = this.sustainer;
		if (sustainer != null && !sustainer.Ended)
		{
			this.sustainer.Maintain();
		}
		sustainedFx?.EffectTick(this, this);
		if (SpawnRubble && Rand.MTBEventOccurs(filthSpawnMTB, 60f, 1f))
		{
			if (CellFinder.TryFindRandomReachableNearbyCell(this.OccupiedRect().RandomCell, base.Map, filthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out var result))
			{
				FilthMaker.TryMakeFilth(result, base.Map, filthTypes.RandomElement());
			}
			this.OccupiedRect().RandomCell.GetPlant(base.Map)?.Destroy();
		}
		if (secondarySpawnTick <= Find.TickManager.TicksGame)
		{
			this.sustainer?.End();
			Map map = base.Map;
			IntVec3 position = base.Position;
			def.building?.groundSpawnerCompleteEffecter?.SpawnMaintained(position, map);
			sustainedFx?.Cleanup();
			Thing.allowDestroyNonDestroyable = true;
			Destroy();
			Thing.allowDestroyNonDestroyable = false;
			Spawn(map, position);
		}
	}

	protected virtual void Spawn(Map map, IntVec3 loc)
	{
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
	}

	private void CreateFX()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			sustainer = SustainerSound?.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			sustainedFx = def.building?.groundSpawnerSustainedEffecter?.Spawn(this, base.Map);
		});
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Set spawn delay",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 0; i < 6; i++)
				{
					int ticks = i * 60;
					list.Add(new FloatMenuOption(i + "s", delegate
					{
						secondarySpawnTick = Find.TickManager.TicksGame + ticks;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
		stringBuilder.AppendLineIfNotEmpty();
		stringBuilder.Append("Emergence".Translate() + ": " + TicksUntilSpawn.ToStringTicksToPeriod());
		return stringBuilder.ToString();
	}
}
