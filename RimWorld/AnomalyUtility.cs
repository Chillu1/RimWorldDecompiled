using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;
using Verse.Sound;

namespace RimWorld;

public static class AnomalyUtility
{
	private static readonly FloatRange UnnaturalCorpseAgeRangeDays = new FloatRange(3f, 15f);

	private const float DuplicateSicknessSeverityPerDay = 0.1f;

	private static readonly CachedTexture OpenCodexGizmoIcon = new CachedTexture("UI/Icons/OpenCodex");

	private static readonly List<IntVec3> cells = new List<IntVec3>();

	private static List<Pawn> tmpEntities = new List<Pawn>();

	private static List<EntityCodexEntryDef> tmpCodexEntries = new List<EntityCodexEntryDef>();

	public static UnnaturalCorpse MakeUnnaturalCorpse(Pawn pawn)
	{
		Pawn pawn2 = Find.PawnDuplicator.Duplicate(pawn);
		pawn2.apparel.DestroyAll();
		pawn2.health.SetDead();
		pawn2.SetFaction(null);
		UnnaturalCorpse obj = (UnnaturalCorpse)ThingMaker.MakeThing(pawn.def.race.unnaturalCorpseDef);
		obj.InnerPawn = pawn2;
		obj.Age = Mathf.RoundToInt(UnnaturalCorpseAgeRangeDays.RandomInRange * 60000f);
		return obj;
	}

	public static bool TryGetNearbyUnseenCell(Pawn pawn, out IntVec3 pos)
	{
		if (!pawn.SpawnedOrAnyParentSpawned)
		{
			pos = IntVec3.Invalid;
			return false;
		}
		Map mapHeld = pawn.MapHeld;
		CellRect view = Find.CameraDriver.CurrentViewRect.ExpandedBy(1);
		IntVec3 positionHeld = pawn.PositionHeld;
		foreach (IntVec3 item in GenRadial.RadialCellsAround(positionHeld, 4.9f, useCenter: false))
		{
			if (IsValidUnseenCell(view, item, mapHeld))
			{
				cells.Add(item);
			}
		}
		if (cells.Empty())
		{
			foreach (IntVec3 item2 in GenRadial.RadialCellsAround(positionHeld, 9.9f, useCenter: false))
			{
				if (IsValidUnseenCell(view, item2, mapHeld))
				{
					cells.Add(item2);
				}
			}
		}
		bool flag = cells.Any();
		pos = (flag ? cells.RandomElement() : IntVec3.Invalid);
		cells.Clear();
		return flag;
	}

	public static bool IsValidUnseenCell(CellRect view, IntVec3 cell, Map map)
	{
		if ((map != Find.CurrentMap || !view.Contains(cell)) && !cell.Fogged(map) && cell.Walkable(map) && cell.GetFence(map) == null)
		{
			return cell.GetDoor(map) == null;
		}
		return false;
	}

	public static bool TryDuplicatePawn(Pawn originalPawn, IntVec3 targetCell, Map map, out Pawn duplicatePawn, Faction faction = null, bool allowCreepjoiners = false, bool randomOutcome = false, bool negativeOutcomes = true)
	{
		if (!allowCreepjoiners && originalPawn.IsCreepJoiner)
		{
			duplicatePawn = null;
			return false;
		}
		duplicatePawn = Find.PawnDuplicator.Duplicate(originalPawn);
		if (faction != null)
		{
			duplicatePawn.SetFaction(faction);
		}
		map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(targetCell, map), targetCell, 60);
		SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(targetCell, map));
		if (randomOutcome && negativeOutcomes)
		{
			int num = Rand.RangeInclusive(0, 3);
			if (num == 1 && (duplicatePawn.health.hediffSet.HasHediffOrWillBecome(HediffDefOf.OrganDecay) || duplicatePawn.health.hediffSet.HasHediffOrWillBecome(HediffDefOf.OrganDecayCreepjoiner)))
			{
				num++;
			}
			if (num == 2 && duplicatePawn.health.hediffSet.HasHediffOrWillBecome(HediffDefOf.CrumblingMind))
			{
				num++;
			}
			switch (num)
			{
			case 0:
				AddDuplicateSickness(originalPawn, duplicatePawn);
				break;
			case 1:
				duplicatePawn.health.AddHediff(HediffDefOf.OrganDecayUndiagnosedDuplicaton);
				break;
			case 2:
				duplicatePawn.health.AddHediff(HediffDefOf.CrumblingMindUndiagnosedDuplication);
				break;
			case 3:
				duplicatePawn.SetFaction(Faction.OfEntities);
				Find.LetterStack.ReceiveLetter("ObeliskHostileDuplicateLetterLabel".Translate(), "ObeliskHostileDuplicateLetter".Translate(originalPawn.Named("PAWN")), LetterDefOf.ThreatBig, duplicatePawn);
				break;
			default:
				Log.Error("Unhandled outcome in pawn duplication " + num);
				break;
			}
		}
		else if (negativeOutcomes)
		{
			AddDuplicateSickness(originalPawn, duplicatePawn);
		}
		Find.PawnDuplicator.AddDuplicate(originalPawn.duplicate.duplicateOf, originalPawn);
		Find.PawnDuplicator.AddDuplicate(duplicatePawn.duplicate.duplicateOf, duplicatePawn);
		GenSpawn.Spawn(duplicatePawn, targetCell, map);
		return duplicatePawn != null;
	}

	private static void AddDuplicateSickness(Pawn originalPawn, Pawn duplicatePawn)
	{
		Hediff_DuplicateSickness hediff_DuplicateSickness = (Hediff_DuplicateSickness)originalPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DuplicateSickness);
		if (hediff_DuplicateSickness == null)
		{
			hediff_DuplicateSickness = (Hediff_DuplicateSickness)HediffMaker.MakeHediff(HediffDefOf.DuplicateSickness, originalPawn);
			originalPawn.health.AddHediff(hediff_DuplicateSickness);
		}
		else
		{
			hediff_DuplicateSickness.GetComp<HediffComp_SeverityPerDay>().severityPerDay = 0.1f;
		}
		Hediff_DuplicateSickness hediff = (Hediff_DuplicateSickness)HediffMaker.MakeHediff(HediffDefOf.DuplicateSickness, duplicatePawn);
		duplicatePawn.health.AddHediff(hediff);
	}

	public static bool Inhumanized(this Pawn pawn)
	{
		if (ModsConfig.AnomalyActive && pawn.health?.hediffSet != null)
		{
			return pawn.health.hediffSet.HasHediff(HediffDefOf.Inhumanized);
		}
		return false;
	}

	public static void Rehumanize(this Pawn pawn)
	{
		if (ModsConfig.AnomalyActive)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Inhumanized);
			if (firstHediffOfDef == null)
			{
				Log.Error("Tried to re-humanized a pawn that was not inhumanized.");
			}
			else
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}

	public static string GetBiosignatureName(int biosignature)
	{
		GrammarRequest request = new GrammarRequest
		{
			Includes = { RulePackDefOf.NamerBiosignature }
		};
		Rand.PushState(biosignature);
		string result = GrammarResolver.Resolve("r_root", request);
		Rand.PopState();
		return result;
	}

	public static Pawn FindEntityOnPlatform(Map map, EntityQueryType queryType)
	{
		tmpEntities.Clear();
		foreach (Building_HoldingPlatform item in map.listerBuildings.AllBuildingsColonistOfClass<Building_HoldingPlatform>())
		{
			CompHoldingPlatformTarget compHoldingPlatformTarget = item.HeldPawn?.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget == null)
			{
				continue;
			}
			switch (queryType)
			{
			case EntityQueryType.Any:
				tmpEntities.Add(item.HeldPawn);
				break;
			case EntityQueryType.ForRelease:
				if (ContainmentUtility.InitiateEscapeMtbDays(item.HeldPawn) > 0f)
				{
					tmpEntities.Add(item.HeldPawn);
				}
				break;
			case EntityQueryType.ForSlaughter:
				if (compHoldingPlatformTarget.Props.canBeExecuted)
				{
					tmpEntities.Add(item.HeldPawn);
				}
				break;
			}
		}
		if (tmpEntities.TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	public static bool ShouldNotifyCodex(Thing thing, EntityDiscoveryType discoveryType, out List<EntityCodexEntryDef> entries)
	{
		entries = null;
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (thing.Spawned && thing.Position.Fogged(thing.Map))
		{
			return false;
		}
		if (thing is Pawn pawn && pawn.IsHiddenFromPlayer())
		{
			return false;
		}
		entries = GetCodexEntriesFor(thing);
		if (entries.NullOrEmpty())
		{
			return false;
		}
		if (discoveryType != EntityDiscoveryType.Unfog)
		{
			if (discoveryType == EntityDiscoveryType.Spawn && MapGenerator.mapBeingGenerated == thing.MapHeld)
			{
				foreach (EntityCodexEntryDef entry in entries)
				{
					if (entry.startDiscovered)
					{
						return true;
					}
					if (entry.discoveryType == discoveryType && entry.allowDiscoveryWhileMapGenerating)
					{
						return true;
					}
				}
				return false;
			}
			foreach (EntityCodexEntryDef entry2 in entries)
			{
				if (entry2.discoveryType == discoveryType)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static Gizmo OpenCodexGizmo(Thing thing)
	{
		if (!ModLister.CheckAnomaly("Codex gizmo"))
		{
			return null;
		}
		if ((Find.Anomaly.HighestLevelReached <= 0 && Find.Storyteller.difficulty.AnomalyPlaystyleDef.generateMonolith) || Find.Selector.SelectedObjects.Count != 1)
		{
			return null;
		}
		if (thing is Pawn { Drafted: not false })
		{
			return null;
		}
		List<EntityCodexEntryDef> entries = GetCodexEntriesFor(thing);
		if (entries.NullOrEmpty())
		{
			return null;
		}
		return new Command_Action
		{
			defaultLabel = "EntityCodex".Translate() + "...",
			defaultDesc = "EntityCodexGizmoTip".Translate(),
			icon = OpenCodexGizmoIcon.Texture,
			action = delegate
			{
				Find.WindowStack.Add(new Dialog_EntityCodex(entries.FirstOrDefault()));
			}
		};
	}

	public static List<EntityCodexEntryDef> GetCodexEntriesFor(Thing thing)
	{
		tmpCodexEntries.Clear();
		if (thing.def.entityCodexEntry != null)
		{
			tmpCodexEntries.Add(thing.def.entityCodexEntry);
		}
		if (thing is Pawn { IsMutant: not false } pawn && pawn.mutant.HasTurned && pawn.mutant.Def.codexEntry != null)
		{
			tmpCodexEntries.Add(pawn.mutant.Def.codexEntry);
		}
		if (thing is UnnaturalCorpse)
		{
			tmpCodexEntries.Add(EntityCodexEntryDefOf.UnnaturalCorpse);
		}
		return tmpCodexEntries;
	}
}
