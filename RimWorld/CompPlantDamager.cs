using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompPlantDamager : ThingComp
	{
		private int ticksToPlantHarm;

		private int queuedCycles;

		private static readonly SimpleCurve DamageMultiplierOverDistance = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 0.2f)
		};

		public CompProperties_PlantDamager Props => (CompProperties_PlantDamager)props;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			ticksToPlantHarm = Props.ticksBetweenDamage;
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref queuedCycles, "queuedCycles", 0);
			Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 0);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				queuedCycles = Props.cycleCountOnSpawn;
			}
		}

		public override void CompTick()
		{
			if (parent.Spawned)
			{
				while (queuedCycles > 0)
				{
					DamageCycle();
					queuedCycles--;
				}
				ticksToPlantHarm--;
				if (ticksToPlantHarm <= 0)
				{
					ticksToPlantHarm = Props.ticksBetweenDamage;
					DamageCycle();
				}
			}
		}

		public void DamageCycle()
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, Props.radius, useCenter: true))
			{
				if (item.InBounds(parent.Map))
				{
					float distance = item.DistanceTo(parent.Position);
					float amount = ComputeDamage(distance);
					Plant plant = item.GetPlant(parent.Map);
					if (plant != null && (!Props.ignoreAnima || (plant.def != ThingDefOf.Plant_GrassAnima && plant.def != ThingDefOf.Plant_TreeAnima)))
					{
						plant.TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount));
					}
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Plant kill cycle";
				command_Action.action = DamageCycle;
				yield return command_Action;
			}
		}

		private float ComputeDamage(float distance)
		{
			return Props.damagePerCycle * DamageMultiplierOverDistance.Evaluate(distance / Props.radius);
		}
	}
}
