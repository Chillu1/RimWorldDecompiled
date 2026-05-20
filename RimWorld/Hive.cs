using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class Hive : Building, IAttackTarget, ILoadReferenceable
{
	public const int PawnSpawnRadius = 2;

	public const float MaxSpawnedPawnsPoints = 500f;

	public const float InitialPawnsPoints = 200f;

	public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();

	public static readonly string MemoAttackedByEnemy = "HiveAttacked";

	public static readonly string MemoDeSpawned = "HiveDeSpawned";

	public static readonly string MemoBurnedBadly = "HiveBurnedBadly";

	public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";

	public CompCanBeDormant CompDormant => GetComp<CompCanBeDormant>();

	Thing IAttackTarget.Thing => this;

	public float TargetPriorityFactor => 0.4f;

	public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

	public CompSpawnerPawn PawnSpawner => GetComp<CompSpawnerPawn>();

	public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
	{
		if (!base.Spawned)
		{
			return true;
		}
		CompCanBeDormant comp = GetComp<CompCanBeDormant>();
		if (comp != null && !comp.Awake)
		{
			return true;
		}
		return false;
	}

	public static void ResetStaticData()
	{
		spawnablePawnKinds.Clear();
		spawnablePawnKinds.Add(PawnKindDefOf.Megascarab);
		spawnablePawnKinds.Add(PawnKindDefOf.Spelopede);
		spawnablePawnKinds.Add(PawnKindDefOf.Megaspider);
		if (ModsConfig.OdysseyActive)
		{
			spawnablePawnKinds.Add(PawnKindDefOf.Locust);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (base.Faction == null)
		{
			SetFaction(Faction.OfInsects);
		}
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (base.Spawned && !CompDormant.Awake && !base.Position.Fogged(base.Map))
		{
			CompDormant.WakeUp();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.DeSpawn(mode);
		if (HiveUtility.TotalSpawnedHivesCount(map, filterFogged: true) == 0)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				item.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.DefeatedInsectHive);
			}
		}
		if (map.generatorDef != MapGeneratorDefOf.InsectLair)
		{
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].ReceiveMemo(MemoDeSpawned);
			}
		}
		HiveUtility.Notify_HiveDespawned(this, map);
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (!questTags.NullOrEmpty())
		{
			bool flag = false;
			List<Thing> list = base.Map.listerThings.ThingsOfDef(def);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is Hive hive && hive != this && hive.CompDormant.Awake && !hive.questTags.NullOrEmpty() && QuestUtility.AnyMatchingTags(hive.questTags, questTags))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				QuestUtility.SendQuestTargetSignals(questTags, "AllHivesDestroyed");
			}
		}
		base.Destroy(mode);
	}

	public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
		{
			GetComp<CompSpawnerPawn>().Lord?.ReceiveMemo(MemoAttackedByEnemy);
		}
		if (dinfo.Def == DamageDefOf.Flame && (float)HitPoints < (float)base.MaxHitPoints * 0.3f)
		{
			GetComp<CompSpawnerPawn>().Lord?.ReceiveMemo(MemoBurnedBadly);
		}
		base.PostApplyDamage(dinfo, totalDamageDealt);
	}

	public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
	{
		if (base.Spawned && (!dinfo.HasValue || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
		{
			List<Lord> lords = base.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].ReceiveMemo(MemoDestroyedNonRoofCollapse);
			}
		}
		base.Kill(dinfo, exactCulprit);
	}

	public override bool PreventPlayerSellingThingsNearby(out string reason)
	{
		if (PawnSpawner.spawnedPawns.Count > 0 && PawnSpawner.spawnedPawns.Any((Pawn p) => !p.Downed))
		{
			reason = def.label;
			return true;
		}
		reason = null;
		return false;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
		{
			yield return questRelatedGizmo;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			bool value = false;
			Scribe_Values.Look(ref value, "active", defaultValue: false);
			if (value)
			{
				CompDormant.WakeUp();
			}
		}
	}
}
