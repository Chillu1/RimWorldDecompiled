using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class StudyUtility
{
	private static readonly HashSet<Pawn> tmpReservers = new HashSet<Pawn>();

	public static void TargetHoldingPlatformForEntity(Pawn carrier, Thing entity, bool transferBetweenPlatforms = false, Thing sourcePlatform = null)
	{
		Find.Targeter.BeginTargeting(TargetingParameters.ForBuilding(), delegate(LocalTargetInfo t)
		{
			if (carrier != null && !CanReserveForTransfer(t))
			{
				Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
			}
			else
			{
				foreach (Thing item in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
				{
					if (item is Building_HoldingPlatform building_HoldingPlatform && entity != building_HoldingPlatform.HeldPawn)
					{
						CompHoldingPlatformTarget compHoldingPlatformTarget = building_HoldingPlatform.HeldPawn?.TryGetComp<CompHoldingPlatformTarget>();
						if (compHoldingPlatformTarget != null && compHoldingPlatformTarget.targetHolder == t.Thing)
						{
							Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
							return;
						}
					}
				}
				CompHoldingPlatformTarget compHoldingPlatformTarget2 = entity.TryGetComp<CompHoldingPlatformTarget>();
				if (compHoldingPlatformTarget2 != null)
				{
					compHoldingPlatformTarget2.targetHolder = t.Thing;
				}
				if (carrier != null)
				{
					Job job = (transferBetweenPlatforms ? JobMaker.MakeJob(JobDefOf.TransferBetweenEntityHolders, sourcePlatform, t, entity) : JobMaker.MakeJob(JobDefOf.CarryToEntityHolder, t, entity));
					job.count = 1;
					carrier.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}
				if (t.Thing != null && !t.Thing.SafelyContains(entity))
				{
					Messages.Message("MessageTargetBelowMinimumContainmentStrength".Translate(t.Thing.Label, entity.Label), MessageTypeDefOf.ThreatSmall);
				}
			}
		}, delegate(LocalTargetInfo t)
		{
			if (ValidateTarget(t))
			{
				GenDraw.DrawTargetHighlight(t);
			}
		}, ValidateTarget, null, null, BaseContent.ClearTex, playSoundOnAction: true, delegate(LocalTargetInfo t)
		{
			CompEntityHolder compEntityHolder = t.Thing?.TryGetComp<CompEntityHolder>();
			if (compEntityHolder == null)
			{
				TaggedString label = "ChooseEntityHolder".Translate().CapitalizeFirst() + "...";
				Widgets.MouseAttachedLabel(label);
			}
			else
			{
				Pawn pawn = null;
				Pawn reserver;
				if (carrier != null)
				{
					pawn = t.Thing.Map.reservationManager.FirstRespectedReserver(t.Thing, carrier);
				}
				else if (t.Thing is Building_HoldingPlatform p && AlreadyReserved(p, out reserver))
				{
					pawn = reserver;
				}
				TaggedString label;
				if (pawn != null)
				{
					label = string.Format("{0}: {1}", "EntityHolderReservedBy".Translate(), pawn.LabelShortCap);
				}
				else
				{
					label = "FloatMenuContainmentStrength".Translate() + ": " + StatDefOf.ContainmentStrength.Worker.ValueToString(compEntityHolder.ContainmentStrength, finalized: false);
					label += "\n" + ("FloatMenuContainmentRequires".Translate(entity).CapitalizeFirst() + ": " + StatDefOf.MinimumContainmentStrength.Worker.ValueToString(entity.GetStatValue(StatDefOf.MinimumContainmentStrength), finalized: false)).Colorize(t.Thing.SafelyContains(entity) ? Color.white : Color.red);
				}
				Widgets.MouseAttachedLabel(label);
			}
		}, delegate
		{
			foreach (Building item2 in entity.MapHeld.listerBuildings.AllBuildingsColonistOfGroup(ThingRequestGroup.EntityHolder))
			{
				if (ValidateTarget(item2) && (carrier == null || CanReserveForTransfer(item2)))
				{
					GenDraw.DrawArrowPointingAt(item2.DrawPos);
				}
			}
		});
		bool CanReserveForTransfer(LocalTargetInfo t)
		{
			if (transferBetweenPlatforms)
			{
				if (t.HasThing)
				{
					return carrier.CanReserve(t.Thing);
				}
				return false;
			}
			return true;
		}
		bool ValidateTarget(LocalTargetInfo t)
		{
			if (t.HasThing && t.Thing.TryGetComp(out CompEntityHolder comp) && comp.HeldPawn == null)
			{
				if (carrier != null)
				{
					return carrier.CanReserveAndReach(t.Thing, PathEndMode.Touch, Danger.Some);
				}
				return true;
			}
			return false;
		}
	}

	public static bool TryFindResearchBench(Pawn pawn, out Building_ResearchBench bench)
	{
		bench = (Building_ResearchBench)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some), 9999f, (Thing t) => pawn.CanReserve(t) && (t.TryGetComp<CompPowerTrader>()?.PowerOn ?? true));
		return bench != null;
	}

	public static bool HoldingPlatformAvailableOnCurrentMap()
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap == null)
		{
			return false;
		}
		foreach (Building item in currentMap.listerBuildings.allBuildingsColonist)
		{
			if (item.TryGetComp<CompEntityHolder>(out var comp) && comp.Available && !AlreadyReserved(item, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAnomalyResearchCategory(this KnowledgeCategoryDef def)
	{
		if (ModsConfig.AnomalyActive && def != null)
		{
			if (def != KnowledgeCategoryDefOf.Basic)
			{
				return def == KnowledgeCategoryDefOf.Advanced;
			}
			return true;
		}
		return false;
	}

	public static bool AlreadyReserved(Thing p, out Pawn reserver)
	{
		tmpReservers.Clear();
		p.Map.reservationManager.ReserversOf(p, tmpReservers);
		reserver = tmpReservers.FirstOrDefault();
		if (reserver != null)
		{
			return true;
		}
		foreach (Thing item in p.Map.listerThings.ThingsInGroup(ThingRequestGroup.HoldingPlatformTarget))
		{
			if (item.TryGetComp<CompHoldingPlatformTarget>().targetHolder == p)
			{
				reserver = item as Pawn;
				return true;
			}
		}
		return false;
	}
}
