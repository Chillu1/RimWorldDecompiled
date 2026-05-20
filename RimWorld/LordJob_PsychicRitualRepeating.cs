using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_PsychicRitualRepeating : LordJob_PsychicRitual
{
	private int iterations = 1;

	private Pawn invoker;

	private IntVec3 ritualCell;

	public const string MemoPsychicRitualCompleted = "PsychicRitualCompleted";

	public const string MemoPsychicRitualCanceled = "PsychicRitualCanceled";

	public LordJob_PsychicRitualRepeating(PsychicRitualDef def, PsychicRitualRoleAssignments assignments, int iterations, IntVec3 ritualCell, float points = -1f)
		: base(def, assignments, points)
	{
		this.iterations = iterations;
		this.ritualCell = ritualCell;
		invoker = assignments.FirstAssignedPawn(PsychicRitualRoleDefOf.Invoker);
	}

	protected LordJob_PsychicRitualRepeating()
	{
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		List<LordToil_PsychicRitual> list = new List<LordToil_PsychicRitual>();
		List<LordToil> list2 = new List<LordToil>();
		LordToil_PsychicRitual lordToil_PsychicRitual = null;
		int i;
		for (i = 0; i < iterations; i++)
		{
			LordToil_PsychicRitual lordToil_PsychicRitual2 = new LordToil_PsychicRitual(def, assignments)
			{
				iteration = i,
				removeLordOnCancel = false
			};
			stateGraph.AddToil(lordToil_PsychicRitual2);
			list.Add(lordToil_PsychicRitual2);
			if (lordToil_PsychicRitual != null)
			{
				LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(ritualCell, 60f);
				stateGraph.AddToil(lordToil_DefendPoint);
				list2.Add(lordToil_DefendPoint);
				Transition transition = new Transition(lordToil_PsychicRitual, lordToil_DefendPoint);
				transition.AddTrigger(new Trigger_Memo("PsychicRitualCompleted" + (i - 1)));
				stateGraph.AddTransition(transition);
				Transition transition2 = new Transition(lordToil_DefendPoint, lordToil_PsychicRitual2);
				transition2.AddTrigger(new Trigger_TicksPassed((i + 1) * 2500));
				stateGraph.AddTransition(transition2);
			}
			else
			{
				stateGraph.StartingToil = lordToil_PsychicRitual2;
			}
			lordToil_PsychicRitual = lordToil_PsychicRitual2;
		}
		LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
		stateGraph.AddToil(lordToil_AssaultColony);
		Transition transition3 = new Transition(null, lordToil_AssaultColony);
		transition3.AddSources(list);
		transition3.AddSources(list2);
		transition3.AddTrigger(new Trigger_Memo("PsychicRitualCompleted" + (i - 1)));
		transition3.AddTrigger(new Trigger_Memo("PsychicRitualCanceled"));
		transition3.AddTrigger(new Trigger_PawnKilled(lord.ownedPawns.Except(invoker).ToList()));
		transition3.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			if (lord.ownedPawns.Any((Pawn x) => !x.DeadOrDowned))
			{
				Messages.Message("MessagePsychicRitualAssault".Translate(lord.faction), lord.ownedPawns, MessageTypeDefOf.ThreatSmall);
			}
		}));
		stateGraph.AddTransition(transition3);
		return stateGraph;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref iterations, "iterations", 0);
		Scribe_References.Look(ref invoker, "invoker");
		Scribe_Values.Look(ref ritualCell, "ritualCell");
	}
}
