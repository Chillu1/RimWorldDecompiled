using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_ForageTracker : IExposable
	{
		private Caravan caravan;

		private float progress;

		private const int UpdateProgressIntervalTicks = 10;

		public Pair<ThingDef, float> ForagedFoodPerDay => ForagedFoodPerDayCalculator.ForagedFoodPerDay(caravan);

		public string ForagedFoodPerDayExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				ForagedFoodPerDayCalculator.ForagedFoodPerDay(caravan, stringBuilder);
				return stringBuilder.ToString();
			}
		}

		public Caravan_ForageTracker(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref progress, "progress", 0f);
		}

		public void ForageTrackerTick()
		{
			if (caravan.IsHashIntervalTick(10))
			{
				UpdateProgressInterval();
			}
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Dev: Forage";
				command_Action.action = Forage;
				yield return command_Action;
			}
		}

		private void UpdateProgressInterval()
		{
			float num = 10f * ForagedFoodPerDayCalculator.GetProgressPerTick(caravan);
			progress += num;
			if (progress >= 1f)
			{
				Forage();
				progress = 0f;
			}
		}

		private void Forage()
		{
			ThingDef foragedFood = caravan.Biome.foragedFood;
			if (foragedFood != null)
			{
				int a = GenMath.RoundRandom(ForagedFoodPerDayCalculator.GetForagedFoodCountPerInterval(caravan));
				int b = Mathf.FloorToInt((caravan.MassCapacity - caravan.MassUsage) / foragedFood.GetStatValueAbstract(StatDefOf.Mass));
				a = Mathf.Min(a, b);
				while (a > 0)
				{
					Thing thing = ThingMaker.MakeThing(foragedFood);
					thing.stackCount = Mathf.Min(a, foragedFood.stackLimit);
					a -= thing.stackCount;
					CaravanInventoryUtility.GiveThing(caravan, thing);
				}
			}
		}
	}
}
