using System;
using System.Collections.Generic;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoyalTitlePermitWorker_CallLaborers : RoyalTitlePermitWorker_Targeted
{
	private Faction calledFaction;

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!CanHitTarget(target))
		{
			if (target.IsValid && showMessages)
			{
				Messages.Message(def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput);
			}
			return false;
		}
		AcceptanceReport acceptanceReport = RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(target, map);
		if (!acceptanceReport.Accepted)
		{
			Messages.Message(acceptanceReport.Reason, new LookTargets(target.Cell, map), MessageTypeDefOf.RejectInput, historical: false);
		}
		return acceptanceReport.Accepted;
	}

	public override void DrawHighlight(LocalTargetInfo target)
	{
		GenDraw.DrawRadiusRing(caller.Position, base.RangeClamped, Color.white);
		RoyalTitlePermitWorker_CallShuttle.DrawShuttleGhost(target, map, ThingDefOf.Shuttle, ThingDefOf.Shuttle.defaultPlacingRot);
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		CallLaborers(target.Cell);
	}

	public override void OnGUI(LocalTargetInfo target)
	{
		if (!target.IsValid || !RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(target, map).Accepted)
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
	{
		if (AidDisabled_NewTemp(map, pawn, faction, out var reason))
		{
			yield return new FloatMenuOption(def.LabelCap + ": " + reason, null);
			yield break;
		}
		Action action = null;
		string description = def.LabelCap + " (" + "CommandCallLaborersNumLaborers".Translate(def.royalAid.pawnCount) + "): ";
		if (FillAidOption(pawn, faction, ref description, out var free))
		{
			action = delegate
			{
				BeginCallLaborers(pawn, map, faction, free);
			};
		}
		yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
	}

	private void BeginCallLaborers(Pawn pawn, Map map, Faction faction, bool free)
	{
		if (!faction.HostileTo(Faction.OfPlayer))
		{
			targetingParameters = new TargetingParameters();
			targetingParameters.canTargetLocations = true;
			targetingParameters.canTargetSelf = false;
			targetingParameters.canTargetPawns = false;
			targetingParameters.canTargetFires = false;
			targetingParameters.canTargetBuildings = true;
			targetingParameters.canTargetItems = true;
			caller = pawn;
			base.map = map;
			calledFaction = faction;
			base.free = free;
			float rangeActual = base.RangeClamped;
			targetingParameters.validator = (TargetInfo target) => (!(rangeActual > 0f) || !(target.Cell.DistanceTo(caller.Position) > rangeActual)) ? true : false;
			Find.Targeter.BeginTargeting(this);
		}
	}

	private void CallLaborers(IntVec3 landingCell)
	{
		QuestScriptDef permit_CallLaborers = QuestScriptDefOf.Permit_CallLaborers;
		Slate slate = new Slate();
		slate.Set("map", map);
		slate.Set("laborersCount", def.royalAid.pawnCount);
		slate.Set("permitFaction", calledFaction);
		slate.Set("laborersPawnKind", def.royalAid.pawnKindDef);
		slate.Set("laborersDurationDays", def.royalAid.aidDurationDays);
		slate.Set("landingCell", landingCell);
		QuestUtility.GenerateQuestAndMakeAvailable(permit_CallLaborers, slate);
		caller.royalty.GetPermit(def, calledFaction).Notify_Used();
		if (!free)
		{
			caller.royalty.TryRemoveFavor(calledFaction, def.royalAid.favorCost);
		}
	}
}
