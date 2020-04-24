using System;
using Verse;

namespace RimWorld
{
	public class CompPlantHarmRadius : ThingComp
	{
		private int plantHarmAge;

		private int ticksToPlantHarm;

		protected CompInitiatable initiatableComp;

		protected CompProperties_PlantHarmRadius PropsPlantHarmRadius => (CompProperties_PlantHarmRadius)props;

		public float AgeDays => (float)plantHarmAge / 60000f;

		public float CurrentRadius => PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(AgeDays);

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref plantHarmAge, "plantHarmAge", 0);
			Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 0);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostPostMake();
			initiatableComp = parent.GetComp<CompInitiatable>();
		}

		public override string CompInspectStringExtra()
		{
			return (string)("FoliageKillRadius".Translate() + ": " + CurrentRadius.ToString("0.0") + "\n" + "RadiusExpandRate".Translate() + ": ") + Math.Round(PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(AgeDays + 1f) - PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(AgeDays)) + "/" + "day".Translate();
		}

		public override void CompTick()
		{
			if (!parent.Spawned || (initiatableComp != null && !initiatableComp.Initiated))
			{
				return;
			}
			plantHarmAge++;
			ticksToPlantHarm--;
			if (ticksToPlantHarm <= 0)
			{
				float currentRadius = CurrentRadius;
				float num = (float)Math.PI * currentRadius * currentRadius * PropsPlantHarmRadius.harmFrequencyPerArea;
				float num2 = 60f / num;
				int num3;
				if (num2 >= 1f)
				{
					ticksToPlantHarm = GenMath.RoundRandom(num2);
					num3 = 1;
				}
				else
				{
					ticksToPlantHarm = 1;
					num3 = GenMath.RoundRandom(1f / num2);
				}
				for (int i = 0; i < num3; i++)
				{
					HarmRandomPlantInRadius(currentRadius);
				}
			}
		}

		private void HarmRandomPlantInRadius(float radius)
		{
			IntVec3 c = parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
			if (!c.InBounds(parent.Map))
			{
				return;
			}
			Plant plant = c.GetPlant(parent.Map);
			if (plant == null)
			{
				return;
			}
			if (plant.LeaflessNow)
			{
				if (Rand.Value < PropsPlantHarmRadius.leaflessPlantKillChance)
				{
					plant.Kill();
				}
			}
			else
			{
				plant.MakeLeafless(Plant.LeaflessCause.Poison);
			}
		}
	}
}
