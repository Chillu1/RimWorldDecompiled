using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class LordJob_SleepThenAssaultColony : LordJob
	{
		private Faction faction;

		public override bool GuiltyOnDowned => true;

		public LordJob_SleepThenAssaultColony()
		{
		}

		public LordJob_SleepThenAssaultColony(Faction faction)
		{
			this.faction = faction;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Sleep firstSource = (LordToil_Sleep)(stateGraph.StartingToil = new LordToil_Sleep());
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
			Transition transition = new Transition(firstSource, startingToil);
			transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.DormancyWakeup));
			transition.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig));
			transition.AddPostAction(new TransitionAction_WakeAll());
			transition.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					zero += lord.ownedPawns[i].Position.ToVector3();
				}
				zero /= (float)lord.ownedPawns.Count;
				SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(zero.ToIntVec3(), base.Map));
			}));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
