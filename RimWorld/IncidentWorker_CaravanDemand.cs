using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_CaravanDemand : IncidentWorker
{
	private static readonly FloatRange DemandAsPercentageOfCaravan = new FloatRange(0.05f, 0.2f);

	private static readonly FloatRange IncidentPointsFactorRange = new FloatRange(1f, 1.7f);

	private const float DemandAnimalsWeight = 0.15f;

	private const float DemandColonistOrPrisonerWeight = 0.15f;

	private const float DemandItemsWeight = 1.5f;

	private const float MaxDemandedAnimalsPct = 0.6f;

	private const float MinDemandedMarketValue = 300f;

	private const float MaxDemandedMarketValue = 3500f;

	private const float TrashMarketValueThreshold = 50f;

	private const float IgnoreApparelMarketValueThreshold = 500f;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Faction faction;
		if (CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile))
		{
			return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out faction);
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.points *= IncidentPointsFactorRange.RandomInRange;
		Caravan caravan = (Caravan)parms.target;
		if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out parms.faction))
		{
			return false;
		}
		List<ThingCount> demands = GenerateDemands(caravan);
		if (demands.NullOrEmpty())
		{
			return false;
		}
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms);
		defaultPawnGroupMakerParms.generateFightersOnly = true;
		defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
		List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		if (attackers.Count == 0)
		{
			Log.Error("Caravan demand incident couldn't generate any enemies even though min points have been checked. faction=" + defaultPawnGroupMakerParms.faction?.ToString() + "(" + ((defaultPawnGroupMakerParms.faction != null) ? defaultPawnGroupMakerParms.faction.def.ToString() : "null") + ") parms=" + parms);
			return false;
		}
		CameraJumper.TryJumpAndSelect(caravan);
		DiaNode diaNode = new DiaNode(GenerateMessageText(parms.faction, attackers.Count, demands, caravan));
		DiaOption diaOption = new DiaOption("CaravanDemand_Give".Translate());
		diaOption.action = delegate
		{
			ActionGive(caravan, demands, attackers);
		};
		diaOption.resolveTree = true;
		diaNode.options.Add(diaOption);
		DiaOption diaOption2 = new DiaOption("CaravanDemand_Fight".Translate());
		diaOption2.action = delegate
		{
			ActionFight(caravan, attackers);
		};
		diaOption2.resolveTree = true;
		diaNode.options.Add(diaOption2);
		TaggedString taggedString = "CaravanDemandTitle".Translate(parms.faction.Name);
		Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, parms.faction, delayInteractivity: true, radioMode: false, taggedString));
		Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString, parms.faction));
		return true;
	}

	private List<ThingCount> GenerateDemands(Caravan caravan)
	{
		float num = 1.8f;
		float num2 = Rand.Value * num;
		if (num2 < 0.15f)
		{
			List<ThingCount> list = TryGenerateColonistOrPrisonerDemand(caravan);
			if (!list.NullOrEmpty())
			{
				return list;
			}
		}
		if (num2 < 0.3f)
		{
			List<ThingCount> list2 = TryGenerateAnimalsDemand(caravan);
			if (!list2.NullOrEmpty())
			{
				return list2;
			}
		}
		List<ThingCount> list3 = TryGenerateItemsDemand(caravan);
		if (!list3.NullOrEmpty())
		{
			return list3;
		}
		if (Rand.Bool)
		{
			List<ThingCount> list4 = TryGenerateColonistOrPrisonerDemand(caravan);
			if (!list4.NullOrEmpty())
			{
				return list4;
			}
			List<ThingCount> list5 = TryGenerateAnimalsDemand(caravan);
			if (!list5.NullOrEmpty())
			{
				return list5;
			}
		}
		else
		{
			List<ThingCount> list6 = TryGenerateAnimalsDemand(caravan);
			if (!list6.NullOrEmpty())
			{
				return list6;
			}
			List<ThingCount> list7 = TryGenerateColonistOrPrisonerDemand(caravan);
			if (!list7.NullOrEmpty())
			{
				return list7;
			}
		}
		return null;
	}

	private List<ThingCount> TryGenerateColonistOrPrisonerDemand(Caravan caravan)
	{
		List<Pawn> list = new List<Pawn>();
		int num = 0;
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			if (caravan.IsOwner(caravan.pawns[i]))
			{
				num++;
			}
		}
		if (num >= 2)
		{
			for (int j = 0; j < caravan.pawns.Count; j++)
			{
				if (caravan.IsOwner(caravan.pawns[j]))
				{
					list.Add(caravan.pawns[j]);
				}
			}
		}
		for (int k = 0; k < caravan.pawns.Count; k++)
		{
			if (caravan.pawns[k].IsPrisoner)
			{
				list.Add(caravan.pawns[k]);
			}
		}
		if (list.Any())
		{
			return new List<ThingCount>
			{
				new ThingCount(list.RandomElement(), 1)
			};
		}
		return null;
	}

	private List<ThingCount> TryGenerateAnimalsDemand(Caravan caravan)
	{
		int num = 0;
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			if (caravan.pawns[i].IsAnimal)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return null;
		}
		int count = Rand.RangeInclusive(1, (int)Mathf.Max((float)num * 0.6f, 1f));
		return (from x in (from x in caravan.pawns.InnerListForReading
				where x.IsAnimal
				orderby x.MarketValue descending
				select x).Take(count)
			select new ThingCount(x, 1)).ToList();
	}

	private List<ThingCount> TryGenerateItemsDemand(Caravan caravan)
	{
		List<ThingCount> list = new List<ThingCount>();
		List<Thing> list2 = new List<Thing>();
		list2.AddRange(caravan.PawnsListForReading.SelectMany((Pawn x) => ThingOwnerUtility.GetAllThingsRecursively(x, allowUnreal: false)));
		list2.RemoveAll((Thing x) => x.MarketValue * (float)x.stackCount < 50f);
		list2.RemoveAll((Thing x) => x.ParentHolder is Pawn_ApparelTracker && x.MarketValue < 500f);
		list2.RemoveAll((Thing x) => x.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker && pawn_EquipmentTracker.pawn.kindDef.destroyGearOnDrop);
		float num = list2.Sum((Thing x) => x.MarketValue * (float)x.stackCount);
		float requestedCaravanValue = Mathf.Clamp(DemandAsPercentageOfCaravan.RandomInRange * num, 300f, 3500f);
		while (requestedCaravanValue > 50f)
		{
			if (!list2.Where((Thing x) => x.MarketValue * (float)x.stackCount <= requestedCaravanValue * 2f).TryRandomElementByWeight((Thing x) => Mathf.Pow(x.MarketValue / x.GetStatValue(StatDefOf.Mass), 2f), out var result))
			{
				return null;
			}
			int num2 = Mathf.Clamp((int)(requestedCaravanValue / result.MarketValue), 1, result.stackCount);
			requestedCaravanValue -= result.MarketValue * (float)num2;
			list.Add(new ThingCount(result, num2));
			list2.Remove(result);
		}
		return list;
	}

	private string GenerateMessageText(Faction enemyFaction, int attackerCount, List<ThingCount> demands, Caravan caravan)
	{
		return "CaravanDemand".Translate(caravan.Name, enemyFaction.NameColored, attackerCount, GenLabel.ThingsLabel(demands), enemyFaction.def.pawnsPlural).Resolve().CapitalizeFirst();
	}

	private void TakeFromCaravan(Caravan caravan, List<ThingCount> demands, Faction enemyFaction)
	{
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < demands.Count; i++)
		{
			ThingCount thingCount = demands[i];
			if (thingCount.Thing is Pawn)
			{
				Pawn pawn = (Pawn)thingCount.Thing;
				caravan.RemovePawn(pawn);
				foreach (Thing item in ThingOwnerUtility.GetAllThingsRecursively(pawn, allowUnreal: false))
				{
					list.Add(item);
					item.holdingOwner.Take(item);
				}
				if (pawn.RaceProps.Humanlike)
				{
					enemyFaction.kidnapped.Kidnap(pawn, null);
				}
				else if (!Find.WorldPawns.Contains(pawn))
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
			else
			{
				thingCount.Thing.SplitOff(thingCount.Count).Destroy();
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (!list[j].Destroyed)
			{
				CaravanInventoryUtility.GiveThing(caravan, list[j]);
			}
		}
	}

	private void ActionGive(Caravan caravan, List<ThingCount> demands, List<Pawn> attackers)
	{
		TakeFromCaravan(caravan, demands, attackers[0].Faction);
		for (int i = 0; i < attackers.Count; i++)
		{
			Find.WorldPawns.PassToWorld(attackers[i]);
		}
	}

	private void ActionFight(Caravan caravan, List<Pawn> attackers)
	{
		Faction enemyFaction = attackers[0].Faction;
		TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, caravan.RandomOwner());
		LongEventHandler.QueueLongEvent(delegate
		{
			Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, attackers, sendLetterIfRelatedPawns: true);
			LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, canKidnap: true, canTimeoutOrFlee: false);
			if (lordJob_AssaultColony != null)
			{
				LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
			}
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
			CameraJumper.TryJump(attackers[0]);
		}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
	}
}
