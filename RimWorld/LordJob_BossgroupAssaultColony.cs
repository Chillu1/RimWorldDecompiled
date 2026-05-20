using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_BossgroupAssaultColony : LordJob
	{
		private static readonly IntRange PrepareTicksRange = new IntRange(5000, 10000);

		private Faction faction;

		private IntVec3 stageLoc;

		private List<Pawn> bosses = new List<Pawn>();

		public LordJob_BossgroupAssaultColony()
		{
		}

		public LordJob_BossgroupAssaultColony(Faction faction, IntVec3 stageLoc, IEnumerable<Pawn> bosses)
		{
			this.faction = faction;
			this.stageLoc = stageLoc;
			this.bosses.AddRange(bosses);
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Stage firstSource = (LordToil_Stage)(stateGraph.StartingToil = new LordToil_Stage(stageLoc));
			LordToil_MoveInBossgroup lordToil_MoveInBossgroup = new LordToil_MoveInBossgroup(bosses);
			stateGraph.AddToil(lordToil_MoveInBossgroup);
			LordToil_AssaultColonyBossgroup lordToil_AssaultColonyBossgroup = new LordToil_AssaultColonyBossgroup();
			stateGraph.AddToil(lordToil_AssaultColonyBossgroup);
			Transition transition = new Transition(firstSource, lordToil_MoveInBossgroup);
			transition.AddTrigger(new Trigger_TicksPassed(PrepareTicksRange.RandomInRange));
			if (faction != null)
			{
				transition.AddPreAction(new TransitionAction_Message("MessageRaidersBeginningAssault".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name), MessageTypeDefOf.ThreatBig));
			}
			transition.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(firstSource, lordToil_AssaultColonyBossgroup);
			transition2.AddTrigger(new Trigger_PawnHarmed());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_MoveInBossgroup, lordToil_AssaultColonyBossgroup);
			transition3.AddTrigger(new Trigger_PawnHarmed());
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref stageLoc, "stageLoc");
			Scribe_Collections.Look(ref bosses, "bosses", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				bosses.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
