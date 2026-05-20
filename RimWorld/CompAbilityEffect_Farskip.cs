using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Farskip : CompAbilityEffect
{
	private new CompProperties_AbilityFarskip Props => (CompProperties_AbilityFarskip)props;

	public override void Apply(GlobalTargetInfo target)
	{
		Caravan caravan = parent.pawn.GetCaravan();
		Map targetMap = (target.WorldObject as MapParent)?.Map;
		IntVec3 targetCell = IntVec3.Invalid;
		List<Pawn> list = PawnsToSkip().ToList();
		if (parent.pawn.Spawned)
		{
			foreach (Pawn item in list)
			{
				parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(item, item.Map), item.Position, 60);
			}
			SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
		}
		if (ShouldEnterMap(target))
		{
			Pawn pawn = AlliedPawnOnMap(targetMap);
			if (pawn != null)
			{
				targetCell = pawn.Position;
			}
			else
			{
				targetCell = parent.pawn.Position;
			}
		}
		if (targetCell.IsValid)
		{
			foreach (Pawn item2 in list)
			{
				if (item2.Spawned)
				{
					item2.teleporting = true;
					item2.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
					AbilityUtility.DoClamor(item2.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
					item2.teleporting = false;
				}
				CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, (IntVec3 cell) => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
				GenSpawn.Spawn(item2, result, targetMap);
				if (item2.drafter != null && item2.IsColonistPlayerControlled && !item2.Downed)
				{
					item2.drafter.Drafted = true;
				}
				item2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, addBattleLog: false);
				item2.Notify_Teleported();
				CompAbilityEffect_Teleport.SendSkipUsedSignal(item2, parent.pawn);
				if (item2.IsPrisoner)
				{
					item2.guest.WaitInsteadOfEscapingForDefaultTicks();
				}
				parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(item2, item2.Map), item2.Position, 60, targetMap);
				SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result, item2.Map));
				if ((item2.IsColonist || item2.RaceProps.packAnimal || item2.IsColonyMech) && item2.Map.IsPlayerHome)
				{
					item2.inventory.UnloadEverything = true;
				}
			}
			caravan?.Destroy();
			return;
		}
		if (target.WorldObject is Caravan caravan2 && caravan2.Faction == parent.pawn.Faction)
		{
			if (caravan != null)
			{
				caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
				caravan2.Notify_Merged(new List<Caravan> { caravan });
				caravan.Destroy();
				return;
			}
			{
				foreach (Pawn item3 in list)
				{
					caravan2.AddPawn(item3, addCarriedPawnToWorldPawnsIfAny: true);
					item3.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
					AbilityUtility.DoClamor(item3.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
				}
				return;
			}
		}
		if (caravan != null)
		{
			caravan.Tile = target.Tile;
			caravan.pather.StopDead();
			return;
		}
		CaravanMaker.MakeCaravan(list, parent.pawn.Faction, target.Tile, addToWorldPawnsIfNotAlready: false);
		foreach (Pawn item4 in list)
		{
			item4.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
		}
	}

	public override IEnumerable<PreCastAction> GetPreCastActions()
	{
		yield return new PreCastAction
		{
			action = delegate
			{
				foreach (Pawn item in PawnsToSkip())
				{
					FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(item, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
					dataAttachedOverlay.link.detachAfterTicks = 5;
					item.Map.flecks.CreateFleck(dataAttachedOverlay);
				}
			},
			ticksAwayFromCast = 5
		};
	}

	private IEnumerable<Pawn> PawnsToSkip()
	{
		Caravan caravan = parent.pawn.GetCaravan();
		if (caravan != null)
		{
			foreach (Pawn pawn2 in caravan.pawns)
			{
				yield return pawn2;
			}
			yield break;
		}
		bool homeMap = parent.pawn.Map.IsPlayerHome;
		bool sentLodgerMessage = false;
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.pawn.Position, parent.pawn.Map, parent.def.EffectRadius, useCenter: true))
		{
			if (!(item is Pawn pawn))
			{
				continue;
			}
			if (pawn.IsQuestLodger())
			{
				if (!sentLodgerMessage)
				{
					Messages.Message("MessageLodgersCantFarskip".Translate(), pawn, MessageTypeDefOf.NegativeEvent, historical: false);
				}
				sentLodgerMessage = true;
			}
			else if (!pawn.Dead && (pawn.IsColonist || pawn.IsPrisonerOfColony || (!homeMap && pawn.IsAnimal && pawn.Faction != null && pawn.Faction.IsPlayer) || pawn.IsColonySubhuman))
			{
				yield return pawn;
			}
		}
	}

	private Pawn AlliedPawnOnMap(Map targetMap)
	{
		return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault((Pawn p) => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer && !PawnsToSkip().Contains(p));
	}

	private bool ShouldEnterMap(GlobalTargetInfo target)
	{
		if (target.WorldObject is Caravan caravan && caravan.Faction == parent.pawn.Faction)
		{
			return false;
		}
		if (target.WorldObject is MapParent { HasMap: not false } mapParent)
		{
			if (AlliedPawnOnMap(mapParent.Map) == null)
			{
				return mapParent.Map == parent.pawn.Map;
			}
			return true;
		}
		return false;
	}

	private bool ShouldJoinCaravan(GlobalTargetInfo target)
	{
		if (target.WorldObject is Caravan caravan)
		{
			return caravan.Faction == parent.pawn.Faction;
		}
		return false;
	}

	public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
	{
		Caravan caravan = parent.pawn.GetCaravan();
		if (caravan != null && caravan.ImmobilizedByMass)
		{
			return false;
		}
		Caravan caravan2 = target.WorldObject as Caravan;
		if (caravan != null && caravan == caravan2)
		{
			return false;
		}
		if (!ShouldEnterMap(target) && !ShouldJoinCaravan(target))
		{
			return false;
		}
		return base.Valid(target, throwMessages);
	}

	public override bool CanApplyOn(GlobalTargetInfo target)
	{
		if (target.WorldObject is MapParent { Map: not null } mapParent && AlliedPawnOnMap(mapParent.Map) == null)
		{
			return false;
		}
		return base.CanApplyOn(target);
	}

	public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
	{
		Pawn pawn = PawnsToSkip().FirstOrDefault((Pawn p) => p.IsQuestLodger());
		if (pawn != null)
		{
			return Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), confirmAction);
		}
		return null;
	}

	public override string WorldMapExtraLabel(GlobalTargetInfo target)
	{
		Caravan caravan = parent.pawn.GetCaravan();
		if (caravan != null && caravan.ImmobilizedByMass)
		{
			return "CaravanImmobilizedByMass".Translate();
		}
		if (!Valid(target))
		{
			return "AbilityNeedAllyToSkip".Translate();
		}
		if (ShouldJoinCaravan(target))
		{
			return "AbilitySkipToJoinCaravan".Translate();
		}
		return "AbilitySkipToRandomAlly".Translate();
	}
}
