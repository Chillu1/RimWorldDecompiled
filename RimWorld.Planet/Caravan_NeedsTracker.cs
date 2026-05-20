using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class Caravan_NeedsTracker : IExposable
{
	public Caravan caravan;

	public Dictionary<Pawn, Pawn> breastfeedingBabyToFeeder;

	private readonly List<Pawn> tmpPawns = new List<Pawn>();

	private static List<Hediff> tmpHediffs = new List<Hediff>();

	private static List<JoyKindDef> tmpAvailableJoyKinds = new List<JoyKindDef>();

	private static List<Thing> tmpInvFood = new List<Thing>();

	public bool AnyPawnsNeedRest
	{
		get
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				if (pawnsListForReading[i].needs?.rest != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Caravan_NeedsTracker()
	{
	}

	public Caravan_NeedsTracker(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref breastfeedingBabyToFeeder, "breastfeedingBabyToFeeder", LookMode.Reference, LookMode.Reference);
	}

	public void NeedsTrackerTickInterval(int delta)
	{
		TrySatisfyPawnsNeeds(delta);
	}

	public void TrySatisfyPawnsNeeds(int delta)
	{
		tmpPawns.Clear();
		tmpPawns.AddRange(caravan.PawnsListForReading.ToList());
		tmpPawns.Sort((Pawn p1, Pawn p2) => p1.RaceProps.Humanlike.CompareTo(p2.RaceProps.Humanlike));
		for (int num = tmpPawns.Count - 1; num >= 0; num--)
		{
			TrySatisfyPawnNeeds(tmpPawns[num], delta);
		}
	}

	private void TrySatisfyPawnNeeds(Pawn pawn, int delta)
	{
		if (pawn.Dead)
		{
			return;
		}
		List<Need> allNeeds = pawn.needs.AllNeeds;
		for (int i = 0; i < allNeeds.Count; i++)
		{
			Need need = allNeeds[i];
			if (need is Need_Rest rest)
			{
				TrySatisfyRestNeed(pawn, rest, delta);
			}
			else if (need is Need_Food food)
			{
				TrySatisfyFoodNeed(pawn, food, delta);
			}
			else if (need is Need_Chemical chemical)
			{
				TrySatisfyChemicalNeed(pawn, chemical, delta);
			}
			else if (need is Need_Joy joy)
			{
				TrySatisfyJoyNeed(pawn, joy, delta);
			}
		}
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			Gene_Hemogen firstGeneOfType = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
			if (firstGeneOfType != null)
			{
				TrySatisfyHemogenNeed(pawn, firstGeneOfType, delta);
			}
		}
		Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
		if (psychicEntropy?.Psylink != null)
		{
			TryGainPsyfocus(psychicEntropy, delta);
		}
		TrySatisfyChemicalDependencies(pawn, delta);
	}

	private void TrySatisfyRestNeed(Pawn pawn, Need_Rest rest, int delta)
	{
		if (!caravan.pather.MovingNow || pawn.InCaravanBed() || pawn.CarriedByCaravan())
		{
			float restEffectiveness = pawn.CurrentCaravanBed()?.GetStatValue(StatDefOf.BedRestEffectiveness) ?? StatDefOf.BedRestEffectiveness.valueIfMissing;
			rest.TickResting(restEffectiveness);
		}
	}

	private void TrySatisfyFoodNeed(Pawn pawn, Need_Food food, int delta)
	{
		if (!ChildcareUtility.CanSuckle(pawn, out var reason))
		{
			breastfeedingBabyToFeeder?.Remove(pawn);
		}
		else
		{
			breastfeedingBabyToFeeder = breastfeedingBabyToFeeder ?? new Dictionary<Pawn, Pawn>(8);
			if (breastfeedingBabyToFeeder.TryGetValue(pawn, out var value))
			{
				if (!caravan.PawnsListForReading.Contains(value))
				{
					breastfeedingBabyToFeeder.Remove(pawn);
				}
				else if (!ChildcareUtility.CanBreastfeed(value, out reason))
				{
					breastfeedingBabyToFeeder.Remove(pawn);
				}
				else
				{
					if (ChildcareUtility.SuckleFromLactatingPawn(pawn, value, delta))
					{
						return;
					}
					breastfeedingBabyToFeeder.Remove(pawn);
				}
			}
			if (!ChildcareUtility.WantsSuckle(pawn, out reason))
			{
				return;
			}
			foreach (Pawn pawn2 in caravan.pawns)
			{
				if (!breastfeedingBabyToFeeder.ContainsValue(pawn2) && ChildcareUtility.CanMomBreastfeedBabyNow(pawn2, pawn, out reason) && ChildcareUtility.CanAutoBreastfeed(pawn2, pawn, forced: false, out reason))
				{
					breastfeedingBabyToFeeder[pawn] = pawn2;
					if (ChildcareUtility.SuckleFromLactatingPawn(pawn, pawn2, delta))
					{
						return;
					}
				}
			}
		}
		if (!food.Starving)
		{
			food.lastNonStarvingTick = Find.TickManager.TicksGame;
		}
		if ((int)food.CurCategory < 1)
		{
			return;
		}
		if (VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
		{
			VirtualPlantsUtility.EatVirtualPlants(pawn);
		}
		else
		{
			if (!CaravanInventoryUtility.TryGetBestFood(caravan, pawn, out var food2, out var owner))
			{
				return;
			}
			food.CurLevel += food2.Ingested(pawn, food.NutritionWanted);
			if (food2.Destroyed)
			{
				if (owner != null)
				{
					owner.inventory.innerContainer.Remove(food2);
					caravan.RecacheInventory();
				}
				if (!caravan.notifiedOutOfFood && !CaravanInventoryUtility.TryGetBestFood(caravan, pawn, out food2, out owner))
				{
					Messages.Message("MessageCaravanRanOutOfFood".Translate(caravan.LabelCap), caravan, MessageTypeDefOf.ThreatBig);
					caravan.notifiedOutOfFood = true;
				}
			}
		}
	}

	private void TrySatisfyHemogenNeed(Pawn pawn, Gene_Hemogen hemogenGene, int delta)
	{
		if (!hemogenGene.ShouldConsumeHemogenNow())
		{
			return;
		}
		Thing thing = CaravanInventoryUtility.AllInventoryItems(caravan).FirstOrFallback((Thing t) => t.def == ThingDefOf.HemogenPack);
		if (thing != null)
		{
			Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
			float num = thing.Ingested(pawn, thing.GetStatValue(StatDefOf.Nutrition));
			if (pawn.needs?.food != null)
			{
				pawn.needs.food.CurLevel += num;
			}
			if (thing.Destroyed && ownerOf != null)
			{
				ownerOf.inventory.innerContainer.Remove(thing);
				caravan.RecacheInventory();
			}
		}
	}

	private void TrySatisfyChemicalNeed(Pawn pawn, Need_Chemical chemical, int delta)
	{
		if ((int)chemical.CurCategory < 2 && CaravanInventoryUtility.TryGetDrugToSatisfyChemicalNeed(caravan, pawn, chemical.AddictionHediff, out var drug, out var owner))
		{
			IngestDrug(pawn, drug, owner);
		}
	}

	private void TrySatisfyChemicalDependencies(Pawn pawn, int delta)
	{
		tmpHediffs.Clear();
		tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
		foreach (Hediff tmpHediff in tmpHediffs)
		{
			if (tmpHediff is Hediff_ChemicalDependency { ShouldSatify: not false } hediff_ChemicalDependency && CaravanInventoryUtility.TryGetDrugToSatisfyChemicalNeed(caravan, pawn, hediff_ChemicalDependency, out var drug, out var owner))
			{
				IngestDrug(pawn, drug, owner);
			}
		}
	}

	public void IngestDrug(Pawn pawn, Thing drug, Pawn drugOwner)
	{
		float num = drug.Ingested(pawn, 0f);
		Need_Food food = pawn.needs.food;
		if (food != null)
		{
			food.CurLevel += num;
		}
		if (drug.Destroyed && drugOwner != null)
		{
			drugOwner.inventory.innerContainer.Remove(drug);
			caravan.RecacheInventory();
		}
	}

	private void TrySatisfyJoyNeed(Pawn pawn, Need_Joy joy, int delta)
	{
		if (!pawn.IsHashIntervalTick(1250, delta))
		{
			return;
		}
		float currentJoyGainPerTick = GetCurrentJoyGainPerTick(pawn);
		if (!(currentJoyGainPerTick <= 0f))
		{
			currentJoyGainPerTick *= 1250f;
			tmpAvailableJoyKinds.Clear();
			GetAvailableJoyKindsFor(pawn, tmpAvailableJoyKinds);
			if (tmpAvailableJoyKinds.TryRandomElementByWeight((JoyKindDef x) => 1f - Mathf.Clamp01(pawn.needs.joy.tolerances[x]), out var result))
			{
				joy.GainJoy(currentJoyGainPerTick, result);
				tmpAvailableJoyKinds.Clear();
			}
		}
	}

	public float GetCurrentJoyGainPerTick(Pawn pawn)
	{
		if (caravan.pather.MovingNow)
		{
			return 0f;
		}
		return 4E-05f;
	}

	public void TryGainPsyfocus(Pawn_PsychicEntropyTracker tracker, int delta)
	{
		if (!caravan.pather.MovingNow && !caravan.NightResting)
		{
			tracker.GainPsyfocus_NewTemp(delta);
		}
	}

	public bool AnyPawnOutOfFood(out string malnutritionHediff)
	{
		tmpInvFood.Clear();
		List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.IsNutritionGivingIngestible)
			{
				tmpInvFood.Add(list[i]);
			}
		}
		List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
		for (int j = 0; j < pawnsListForReading.Count; j++)
		{
			Pawn pawn = pawnsListForReading[j];
			if (!pawn.RaceProps.EatsFood || pawn.needs?.food == null || VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
			{
				continue;
			}
			bool flag = false;
			for (int k = 0; k < tmpInvFood.Count; k++)
			{
				if (CaravanPawnsNeedsUtility.CanEatForNutritionEver(tmpInvFood[k].def, pawn))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			int num = -1;
			string text = null;
			for (int l = 0; l < pawnsListForReading.Count; l++)
			{
				Hediff firstHediffOfDef = pawnsListForReading[l].health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
				if (firstHediffOfDef != null && (text == null || firstHediffOfDef.CurStageIndex > num))
				{
					num = firstHediffOfDef.CurStageIndex;
					text = firstHediffOfDef.LabelCap;
				}
			}
			malnutritionHediff = text;
			tmpInvFood.Clear();
			return true;
		}
		malnutritionHediff = null;
		tmpInvFood.Clear();
		return false;
	}

	private void GetAvailableJoyKindsFor(Pawn p, List<JoyKindDef> outJoyKinds)
	{
		outJoyKinds.Clear();
		if (!p.needs.joy.tolerances.BoredOf(JoyKindDefOf.Meditative))
		{
			outJoyKinds.Add(JoyKindDefOf.Meditative);
		}
		if (p.needs.joy.tolerances.BoredOf(JoyKindDefOf.Social))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < caravan.pawns.Count; i++)
		{
			if (caravan.pawns[i].RaceProps.Humanlike && !caravan.pawns[i].Downed && !caravan.pawns[i].InMentalState)
			{
				num++;
			}
		}
		if (num >= 2)
		{
			outJoyKinds.Add(JoyKindDefOf.Social);
		}
	}
}
