using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_PlantGrower : Building, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow;

		private CompPowerTrader compPower;

		public IEnumerable<Plant> PlantsOnMe
		{
			get
			{
				if (base.Spawned)
				{
					foreach (IntVec3 item in this.OccupiedRect())
					{
						List<Thing> thingList = base.Map.thingGrid.ThingsListAt(item);
						for (int i = 0; i < thingList.Count; i++)
						{
							Plant plant = thingList[i] as Plant;
							if (plant != null)
							{
								yield return plant;
							}
						}
					}
				}
			}
		}

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells => this.OccupiedRect().Cells;

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
		}

		public override void PostMake()
		{
			base.PostMake();
			plantDefToGrow = def.building.defaultPlantToGrow;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			compPower = GetComp<CompPowerTrader>();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
		}

		public override void TickRare()
		{
			if (compPower != null && !compPower.PowerOn)
			{
				foreach (Plant item in PlantsOnMe)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 1f);
					item.TakeDamage(dinfo);
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			foreach (Plant item in PlantsOnMe.ToList())
			{
				item.Destroy();
			}
			base.DeSpawn(mode);
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Spawned)
			{
				text = ((!PlantUtility.GrowthSeasonNow(base.Position, base.Map, forSowing: true)) ? ((string)(text + ("\n" + "CannotGrowBadSeasonTemperature".Translate()))) : ((string)(text + ("\n" + "GrowSeasonHereNow".Translate()))));
			}
			return text;
		}

		public ThingDef GetPlantDefToGrow()
		{
			return plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			if (compPower != null && !compPower.PowerOn)
			{
				return false;
			}
			return true;
		}
	}
}
