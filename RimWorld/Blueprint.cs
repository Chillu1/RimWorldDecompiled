using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Blueprint : ThingWithComps, IConstructible
	{
		public override string Label => def.entityDefToBuild.label + "BlueprintLabelExtra".Translate();

		protected abstract float WorkTotal
		{
			get;
		}

		public override void Draw()
		{
			if (def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			else
			{
				Comps_PostDraw();
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
			if (selectMonumentMarkerGizmo != null)
			{
				yield return selectMonumentMarkerGizmo;
			}
		}

		public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			jobEnded = false;
			if (GenConstruct.FirstBlockingThing(this, workerPawn) != null)
			{
				workerPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				jobEnded = true;
				createdThing = null;
				return false;
			}
			createdThing = MakeSolidThing();
			Map map = base.Map;
			GenSpawn.WipeExistingThings(base.Position, base.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
			if (!base.Destroyed)
			{
				Destroy();
			}
			if (createdThing.def.CanHaveFaction)
			{
				createdThing.SetFactionDirect(workerPawn.Faction);
			}
			GenSpawn.Spawn(createdThing, base.Position, map, base.Rotation);
			return true;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			map.blueprintGrid.Register(this);
			base.SpawnSetup(map, respawningAfterLoad);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			map.blueprintGrid.DeRegister(this);
		}

		protected abstract Thing MakeSolidThing();

		public abstract List<ThingDefCountClass> MaterialsNeeded();

		public abstract ThingDef EntityToBuildStuff();

		public Thing BlockingHaulableOnTop()
		{
			if (def.entityDefToBuild.passability == Traversability.Standable)
			{
				return null;
			}
			foreach (IntVec3 item in this.OccupiedRect())
			{
				List<Thing> thingList = item.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.EverHaulable)
					{
						return thing;
					}
				}
			}
			return null;
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (base.Faction == null)
			{
				return 0;
			}
			if (def.entityDefToBuild is TerrainDef)
			{
				return 0;
			}
			if ((p.Faction == base.Faction || p.HostFaction == base.Faction) && (base.Map.reservationManager.IsReservedByAnyoneOf(this, p.Faction) || (p.HostFaction != null && base.Map.reservationManager.IsReservedByAnyoneOf(this, p.HostFaction))))
			{
				return Frame.AvoidUnderConstructionPathFindCost;
			}
			return 0;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("WorkLeft".Translate() + ": " + WorkTotal.ToStringWorkAmount());
			return stringBuilder.ToString();
		}
	}
}
