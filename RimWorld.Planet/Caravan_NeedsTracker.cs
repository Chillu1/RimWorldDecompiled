using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_NeedsTracker : IExposable
	{
		public Caravan caravan;

		private static List<JoyKindDef> tmpAvailableJoyKinds = new List<JoyKindDef>();

		private static List<Thing> tmpInvFood = new List<Thing>();

		public Caravan_NeedsTracker()
		{
		}

		public Caravan_NeedsTracker(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
		}

		public void NeedsTrackerTick()
		{
			TrySatisfyPawnsNeeds();
		}

		public void TrySatisfyPawnsNeeds()
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int num = pawnsListForReading.Count - 1; num >= 0; num--)
			{
				TrySatisfyPawnNeeds(pawnsListForReading[num]);
			}
		}

		private void TrySatisfyPawnNeeds(Pawn pawn)
		{
			if (pawn.Dead)
			{
				return;
			}
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need need = allNeeds[i];
				Need_Rest need_Rest = need as Need_Rest;
				Need_Food need_Food = need as Need_Food;
				Need_Chemical need_Chemical = need as Need_Chemical;
				Need_Joy need_Joy = need as Need_Joy;
				if (need_Rest != null)
				{
					TrySatisfyRestNeed(pawn, need_Rest);
				}
				else if (need_Food != null)
				{
					TrySatisfyFoodNeed(pawn, need_Food);
				}
				else if (need_Chemical != null)
				{
					TrySatisfyChemicalNeed(pawn, need_Chemical);
				}
				else if (need_Joy != null)
				{
					TrySatisfyJoyNeed(pawn, need_Joy);
				}
			}
			Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
			if (psychicEntropy.Psylink != null)
			{
				TryGainPsyfocus(psychicEntropy);
			}
		}

		private void TrySatisfyRestNeed(Pawn pawn, Need_Rest rest)
		{
			if (!caravan.pather.MovingNow || pawn.InCaravanBed() || pawn.CarriedByCaravan())
			{
				float restEffectiveness = pawn.CurrentCaravanBed()?.GetStatValue(StatDefOf.BedRestEffectiveness) ?? StatDefOf.BedRestEffectiveness.valueIfMissing;
				rest.TickResting(restEffectiveness);
			}
		}

		private void TrySatisfyFoodNeed(Pawn pawn, Need_Food food)
		{
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
						caravan.RecacheImmobilizedNow();
						caravan.RecacheDaysWorthOfFood();
					}
					if (!caravan.notifiedOutOfFood && !CaravanInventoryUtility.TryGetBestFood(caravan, pawn, out food2, out owner))
					{
						Messages.Message("MessageCaravanRanOutOfFood".Translate(caravan.LabelCap, pawn.Label, pawn.Named("PAWN")), caravan, MessageTypeDefOf.ThreatBig);
						caravan.notifiedOutOfFood = true;
					}
				}
			}
		}

		private void TrySatisfyChemicalNeed(Pawn pawn, Need_Chemical chemical)
		{
			if ((int)chemical.CurCategory < 2 && CaravanInventoryUtility.TryGetDrugToSatisfyChemicalNeed(caravan, pawn, chemical, out var drug, out var owner))
			{
				IngestDrug(pawn, drug, owner);
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
				caravan.RecacheImmobilizedNow();
				caravan.RecacheDaysWorthOfFood();
			}
		}

		private void TrySatisfyJoyNeed(Pawn pawn, Need_Joy joy)
		{
			if (!pawn.IsHashIntervalTick(1250))
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

		public void TryGainPsyfocus(Pawn_PsychicEntropyTracker tracker)
		{
			if (!caravan.pather.MovingNow && !caravan.NightResting)
			{
				tracker.GainPsyfocus();
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
				if (!pawn.RaceProps.EatsFood || VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
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
}
