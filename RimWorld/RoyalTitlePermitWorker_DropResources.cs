using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class RoyalTitlePermitWorker_DropResources : RoyalTitlePermitWorker_Targeted
{
	private Faction faction;

	private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		CallResources(target.Cell);
	}

	public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
	{
		if (map.generatorDef.isUnderground)
		{
			yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION")), null);
			yield break;
		}
		if (faction.HostileTo(Faction.OfPlayer))
		{
			yield return new FloatMenuOption("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
			yield break;
		}
		Action action = null;
		string description = def.LabelCap + ": ";
		if (FillAidOption(pawn, faction, ref description, out var free))
		{
			action = delegate
			{
				BeginCallResources(pawn, faction, map, free);
			};
		}
		yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
	}

	public override IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction)
	{
		if (!FillCaravanAidOption(pawn, faction, out var description, out free, out var disableNotEnoughFavor))
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action
		{
			defaultLabel = def.LabelCap + " (" + pawn.LabelShort + ")",
			defaultDesc = description,
			icon = CommandTex,
			action = delegate
			{
				Caravan caravan = pawn.GetCaravan();
				float num = caravan.MassUsage;
				List<ThingDefCountClass> itemsToDrop = def.royalAid.itemsToDrop;
				for (int i = 0; i < itemsToDrop.Count; i++)
				{
					num += itemsToDrop[i].thingDef.BaseMass * (float)itemsToDrop[i].count;
				}
				if (num > caravan.MassCapacity)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DropResourcesOverweightConfirm".Translate(), delegate
					{
						CallResourcesToCaravan(pawn, faction, free);
					}, destructive: true));
				}
				else
				{
					CallResourcesToCaravan(pawn, faction, free);
				}
			}
		};
		if (pawn.MapHeld != null && pawn.MapHeld.generatorDef.isUnderground)
		{
			command_Action.Disable("CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION")));
		}
		if (faction.HostileTo(Faction.OfPlayer))
		{
			command_Action.Disable("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
		}
		if (disableNotEnoughFavor)
		{
			command_Action.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
		}
		yield return command_Action;
	}

	private void BeginCallResources(Pawn caller, Faction faction, Map map, bool free)
	{
		targetingParameters = new TargetingParameters();
		targetingParameters.canTargetLocations = true;
		targetingParameters.canTargetBuildings = false;
		targetingParameters.canTargetPawns = false;
		base.caller = caller;
		base.map = map;
		this.faction = faction;
		base.free = free;
		float rangeActual = base.RangeClamped;
		targetingParameters.validator = delegate(TargetInfo target)
		{
			if (rangeActual > 0f && target.Cell.DistanceTo(caller.Position) > rangeActual)
			{
				return false;
			}
			if (target.Cell.Fogged(map))
			{
				return false;
			}
			return DropCellFinder.CanPhysicallyDropInto(target.Cell, map, canRoofPunch: true) ? true : false;
		};
		Find.Targeter.BeginTargeting(this);
	}

	private void CallResources(IntVec3 cell)
	{
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < def.royalAid.itemsToDrop.Count; i++)
		{
			Thing thing = ThingMaker.MakeThing(def.royalAid.itemsToDrop[i].thingDef);
			thing.stackCount = def.royalAid.itemsToDrop[i].count;
			list.Add(thing);
		}
		if (list.Any())
		{
			ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
			activeTransporterInfo.innerContainer.TryAddRangeOrTransfer(list);
			DropPodUtility.MakeDropPodAt(cell, map, activeTransporterInfo);
			Messages.Message("MessagePermitTransportDrop".Translate(faction.Named("FACTION")), new LookTargets(cell, map), MessageTypeDefOf.NeutralEvent);
			caller.royalty.GetPermit(def, faction).Notify_Used();
			if (!free)
			{
				caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
			}
		}
	}

	private void CallResourcesToCaravan(Pawn caller, Faction faction, bool free)
	{
		Caravan caravan = caller.GetCaravan();
		for (int i = 0; i < def.royalAid.itemsToDrop.Count; i++)
		{
			Thing thing = ThingMaker.MakeThing(def.royalAid.itemsToDrop[i].thingDef);
			thing.stackCount = def.royalAid.itemsToDrop[i].count;
			CaravanInventoryUtility.GiveThing(caravan, thing);
		}
		Messages.Message("MessagePermitTransportDropCaravan".Translate(faction.Named("FACTION"), caller.Named("PAWN")), caravan, MessageTypeDefOf.NeutralEvent);
		caller.royalty.GetPermit(def, faction).Notify_Used();
		if (!free)
		{
			caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
		}
	}
}
