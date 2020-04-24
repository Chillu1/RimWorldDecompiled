using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_MechanoidsDefend : LordJob_MechanoidDefendBase
	{
		private Thing shipPart;

		public static readonly string MemoDamaged = "ShipPartDamaged";

		public override bool CanBlockHostileVisitors => false;

		public override bool AddFleeToil => false;

		public LordJob_MechanoidsDefend()
		{
		}

		public LordJob_MechanoidsDefend(List<Thing> things, Faction faction, float defendRadius, IntVec3 defSpot, bool canAssaultColony, bool isMechCluster)
		{
			base.things.AddRange(things);
			base.faction = faction;
			base.defendRadius = defendRadius;
			base.defSpot = defSpot;
			base.canAssaultColony = canAssaultColony;
			base.isMechCluster = isMechCluster;
		}

		public LordJob_MechanoidsDefend(SpawnedPawnParams parms)
		{
			things.Add(parms.spawnerThing);
			faction = parms.spawnerThing.Faction;
			defendRadius = parms.defendRadius;
			defSpot = parms.defSpot;
			canAssaultColony = false;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!defSpot.IsValid)
			{
				Log.Warning("LordJob_MechanoidsDefendShip defSpot is invalid. Returning graph for LordJob_AssaultColony.");
				stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph());
				return stateGraph;
			}
			LordToil_DefendPoint lordToil_DefendPoint = (LordToil_DefendPoint)(stateGraph.StartingToil = new LordToil_DefendPoint(defSpot, defendRadius));
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
			stateGraph.AddToil(lordToil_AssaultColony);
			if (canAssaultColony)
			{
				LordToil_AssaultColony lordToil_AssaultColony2 = new LordToil_AssaultColony();
				stateGraph.AddToil(lordToil_AssaultColony2);
				Transition transition = new Transition(lordToil_DefendPoint, lordToil_AssaultColony);
				transition.AddSource(lordToil_AssaultColony2);
				transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
				stateGraph.AddTransition(transition);
				Transition transition2 = new Transition(lordToil_DefendPoint, lordToil_AssaultColony2);
				transition2.AddTrigger(new Trigger_PawnHarmed(0.5f, requireInstigatorWithFaction: true));
				transition2.AddTrigger(new Trigger_PawnLostViolently());
				transition2.AddTrigger(new Trigger_Memo(MemoDamaged));
				transition2.AddPostAction(new TransitionAction_EndAllJobs());
				stateGraph.AddTransition(transition2);
				Transition transition3 = new Transition(lordToil_AssaultColony2, lordToil_DefendPoint);
				transition3.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1380, MemoDamaged));
				transition3.AddPostAction(new TransitionAction_EndAttackBuildingJobs());
				stateGraph.AddTransition(transition3);
				Transition transition4 = new Transition(lordToil_DefendPoint, lordToil_AssaultColony);
				transition4.AddSource(lordToil_AssaultColony2);
				transition4.AddTrigger(new Trigger_AnyThingDamageTaken(things, 0.5f));
				transition4.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
				stateGraph.AddTransition(transition4);
			}
			Transition transition5 = new Transition(lordToil_DefendPoint, lordToil_AssaultColony);
			transition5.AddTrigger(new Trigger_ChanceOnSignal(TriggerSignalType.MechClusterDefeated, 1f));
			stateGraph.AddTransition(transition5);
			if (!isMechCluster)
			{
				Transition transition6 = new Transition(lordToil_DefendPoint, lordToil_AssaultColony);
				transition6.AddTrigger(new Trigger_AnyThingDamageTaken(things, 1f));
				stateGraph.AddTransition(transition6);
			}
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shipPart, "shipPart");
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			things.RemoveAll((Thing x) => x.DestroyedOrNull());
			if (shipPart != null)
			{
				if (things == null)
				{
					things = new List<Thing>();
				}
				things.Add(shipPart);
				shipPart = null;
			}
		}
	}
}
