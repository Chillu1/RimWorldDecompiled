using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_AssaultColony : LordJob
	{
		private Faction assaulterFaction;

		private bool canKidnap = true;

		private bool canTimeoutOrFlee = true;

		private bool sappers;

		private bool useAvoidGridSmart;

		private bool canSteal = true;

		private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);

		private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);

		public override bool GuiltyOnDowned => true;

		public LordJob_AssaultColony()
		{
		}

		public LordJob_AssaultColony(SpawnedPawnParams parms)
		{
			assaulterFaction = parms.spawnerThing.Faction;
			canKidnap = false;
			canTimeoutOrFlee = false;
			canSteal = false;
		}

		public LordJob_AssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true)
		{
			this.assaulterFaction = assaulterFaction;
			this.canKidnap = canKidnap;
			this.canTimeoutOrFlee = canTimeoutOrFlee;
			this.sappers = sappers;
			this.useAvoidGridSmart = useAvoidGridSmart;
			this.canSteal = canSteal;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil lordToil = null;
			if (sappers)
			{
				lordToil = new LordToil_AssaultColonySappers();
				if (useAvoidGridSmart)
				{
					lordToil.useAvoidGrid = true;
				}
				stateGraph.AddToil(lordToil);
				Transition transition = new Transition(lordToil, lordToil, canMoveToSameState: true);
				transition.AddTrigger(new Trigger_PawnLost());
				stateGraph.AddTransition(transition);
			}
			LordToil lordToil2 = new LordToil_AssaultColony();
			if (useAvoidGridSmart)
			{
				lordToil2.useAvoidGrid = true;
			}
			stateGraph.AddToil(lordToil2);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true);
			lordToil_ExitMap.useAvoidGrid = true;
			stateGraph.AddToil(lordToil_ExitMap);
			if (sappers)
			{
				Transition transition2 = new Transition(lordToil, lordToil2);
				transition2.AddTrigger(new Trigger_NoFightingSappers());
				stateGraph.AddTransition(transition2);
			}
			if (assaulterFaction.def.humanlikeFaction)
			{
				if (canTimeoutOrFlee)
				{
					Transition transition3 = new Transition(lordToil2, lordToil_ExitMap);
					if (lordToil != null)
					{
						transition3.AddSource(lordToil);
					}
					transition3.AddTrigger(new Trigger_TicksPassed(sappers ? SapTimeBeforeGiveUp.RandomInRange : AssaultTimeBeforeGiveUp.RandomInRange));
					transition3.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
					stateGraph.AddTransition(transition3);
					Transition transition4 = new Transition(lordToil2, lordToil_ExitMap);
					if (lordToil != null)
					{
						transition4.AddSource(lordToil);
					}
					float randomInRange = new FloatRange(0.25f, 0.35f).RandomInRange;
					transition4.AddTrigger(new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
					transition4.AddPreAction(new TransitionAction_Message("MessageRaidersSatisfiedLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
					stateGraph.AddTransition(transition4);
				}
				if (canKidnap)
				{
					LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Kidnap().CreateGraph()).StartingToil;
					Transition transition5 = new Transition(lordToil2, startingToil);
					if (lordToil != null)
					{
						transition5.AddSource(lordToil);
					}
					transition5.AddPreAction(new TransitionAction_Message("MessageRaidersKidnapping".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
					transition5.AddTrigger(new Trigger_KidnapVictimPresent());
					stateGraph.AddTransition(transition5);
				}
				if (canSteal)
				{
					LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_Steal().CreateGraph()).StartingToil;
					Transition transition6 = new Transition(lordToil2, startingToil2);
					if (lordToil != null)
					{
						transition6.AddSource(lordToil);
					}
					transition6.AddPreAction(new TransitionAction_Message("MessageRaidersStealing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
					transition6.AddTrigger(new Trigger_HighValueThingsAround());
					stateGraph.AddTransition(transition6);
				}
			}
			Transition transition7 = new Transition(lordToil2, lordToil_ExitMap);
			if (lordToil != null)
			{
				transition7.AddSource(lordToil);
			}
			transition7.AddTrigger(new Trigger_BecameNonHostileToPlayer());
			transition7.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
			stateGraph.AddTransition(transition7);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref assaulterFaction, "assaulterFaction");
			Scribe_Values.Look(ref canKidnap, "canKidnap", defaultValue: true);
			Scribe_Values.Look(ref canTimeoutOrFlee, "canTimeoutOrFlee", defaultValue: true);
			Scribe_Values.Look(ref sappers, "sappers", defaultValue: false);
			Scribe_Values.Look(ref useAvoidGridSmart, "useAvoidGridSmart", defaultValue: false);
			Scribe_Values.Look(ref canSteal, "canSteal", defaultValue: true);
		}
	}
}
