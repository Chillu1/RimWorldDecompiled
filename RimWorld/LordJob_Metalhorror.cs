using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_Metalhorror : LordJob
{
	public int biosignature;

	private const int TicksToDormant = 15000;

	private const int TicksToDormantHibernation = 7500;

	private const int TransitionCheckRateTicks = 120;

	public override bool CanAutoAddPawns => false;

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_MetalhorrorActive lordToil_MetalhorrorActive = new LordToil_MetalhorrorActive();
		stateGraph.AddToil(lordToil_MetalhorrorActive);
		stateGraph.StartingToil = lordToil_MetalhorrorActive;
		LordToil_Sleep lordToil_Sleep = new LordToil_Sleep();
		stateGraph.AddToil(lordToil_Sleep);
		Transition transition = new Transition(lordToil_MetalhorrorActive, lordToil_Sleep);
		transition.AddTrigger(new Trigger_Custom((TriggerSignal _) => lord.ownedPawns.Count != 0 && GenTicks.IsTickInterval(120) && ShouldBeHibernating()));
		transition.AddPostAction(new TransitionAction_Message("MessageMetalHorrorDormant".Translate(), MessageTypeDefOf.NeutralEvent));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_Sleep, lordToil_MetalhorrorActive);
		transition2.AddTrigger(new Trigger_DormancyWakeup());
		transition2.AddTrigger(new Trigger_Custom((TriggerSignal _) => lord.ownedPawns.Count != 0 && GenTicks.IsTickInterval(120) && !ShouldBeHibernating()));
		transition2.AddPostAction(new TransitionAction_WakeAll());
		transition2.AddPostAction(new TransitionAction_Letter("LetterMetalhorrorReawakeningLabel".Translate(), "LetterMetalhorrorReawakening".Translate(), LetterDefOf.ThreatBig));
		stateGraph.AddTransition(transition2);
		return stateGraph;
	}

	private bool ShouldBeHibernating()
	{
		bool result = false;
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			if (ownedPawn.SpawnedOrAnyParentSpawned)
			{
				result = true;
				if (GenTicks.TicksGame < ownedPawn.TickSpawned + 15000 || ownedPawn.mindState.WasRecentlyCombatantTicks(15000) || GenTicks.TicksGame < ownedPawn.mindState.hibernationEndedTick + 7500 || ownedPawn.IsOnHoldingPlatform)
				{
					return false;
				}
			}
		}
		return result;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref biosignature, "biosignature", 0);
	}
}
