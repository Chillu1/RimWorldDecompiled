using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class RoyalTitlePermitWorker_OrbitalStrike : RoyalTitlePermitWorker_Targeted
	{
		private Faction faction;

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			if (!CanHitTarget(target))
			{
				if (target.IsValid)
				{
					Messages.Message(def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput);
				}
				return false;
			}
			return true;
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			GenDraw.DrawRadiusRing(caller.Position, def.royalAid.targetingRange, Color.white);
			GenDraw.DrawRadiusRing(target.Cell, def.royalAid.radius + def.royalAid.explosionRadiusRange.max, Color.white);
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			CallBombardment(target.Cell);
		}

		public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			if (faction.HostileTo(Faction.OfPlayer))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
				yield break;
			}
			string description = def.LabelCap + ": ";
			Action action = null;
			if (FillAidOption(pawn, faction, ref description, out var free))
			{
				action = delegate
				{
					BeginCallBombardment(pawn, faction, map, free);
				};
			}
			yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
		}

		private void BeginCallBombardment(Pawn caller, Faction faction, Map map, bool free)
		{
			targetingParameters = new TargetingParameters();
			targetingParameters.canTargetLocations = true;
			targetingParameters.canTargetSelf = true;
			targetingParameters.canTargetFires = true;
			targetingParameters.canTargetItems = true;
			base.caller = caller;
			base.map = map;
			this.faction = faction;
			base.free = free;
			targetingParameters.validator = delegate(TargetInfo target)
			{
				if (def.royalAid.targetingRange > 0f && target.Cell.DistanceTo(caller.Position) > def.royalAid.targetingRange)
				{
					return false;
				}
				return (!target.Cell.Fogged(map)) ? true : false;
			};
			Find.Targeter.BeginTargeting(this);
		}

		private void CallBombardment(IntVec3 targetCell)
		{
			Bombardment obj = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, targetCell, map);
			obj.impactAreaRadius = def.royalAid.radius;
			obj.explosionRadiusRange = def.royalAid.explosionRadiusRange;
			obj.bombIntervalTicks = def.royalAid.intervalTicks;
			obj.randomFireRadius = 1;
			obj.explosionCount = def.royalAid.explosionCount;
			obj.warmupTicks = def.royalAid.warmupTicks;
			obj.instigator = caller;
			SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera();
			caller.royalty.GetPermit(def, faction).Notify_Used();
			if (!free)
			{
				caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
			}
		}
	}
}
