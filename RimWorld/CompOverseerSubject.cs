using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class CompOverseerSubject : ThingComp
{
	private int delayUntilFeralCheck;

	private Effecter effect;

	public static bool debugDisableNeedsOverseerEffect;

	private List<Pawn> tmpFeralPawns = new List<Pawn>();

	public CompProperties_OverseerSubject Props => (CompProperties_OverseerSubject)props;

	public Pawn Parent => (Pawn)parent;

	public OverseerSubjectState State
	{
		get
		{
			if (Overseer?.mechanitor == null)
			{
				return OverseerSubjectState.RequiresOverseer;
			}
			if (Overseer.mechanitor.ControlledPawns.Contains(Parent))
			{
				return OverseerSubjectState.Overseen;
			}
			return OverseerSubjectState.RequiresBandwidth;
		}
	}

	private Pawn Overseer => ((Pawn)parent)?.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);

	public int DelayUntilFeralCheckTicks => delayUntilFeralCheck;

	public void Notify_DisconnectedFromOverseer()
	{
		if (Parent.Drafted)
		{
			Parent.drafter.Drafted = false;
		}
		if (Parent.carryTracker?.CarriedThing != null)
		{
			Parent.carryTracker.TryDropCarriedThing(Parent.Position, ThingPlaceMode.Near, out var _);
		}
		Parent.jobs?.EndCurrentJob(JobCondition.InterruptForced);
		Parent.jobs?.CheckForJobOverride();
		delayUntilFeralCheck = Props.delayUntilFeralCheck;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			delayUntilFeralCheck = Props.delayUntilFeralCheck;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (!ModLister.CheckBiotech("Overseer subject"))
		{
			parent.Destroy();
			return;
		}
		effect?.Cleanup();
		effect = null;
		base.PostDeSpawn(map, mode);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!debugDisableNeedsOverseerEffect && CanGoFeral(Parent))
		{
			if (Props.needsOverseerEffect != null && effect == null)
			{
				effect = Props.needsOverseerEffect.SpawnAttached(parent, parent.Map);
			}
		}
		else
		{
			effect?.Cleanup();
			effect = null;
		}
		effect?.EffectTick(parent, parent);
		if (CanGoFeral(Parent))
		{
			if (delayUntilFeralCheck > 0)
			{
				delayUntilFeralCheck--;
			}
			if (delayUntilFeralCheck <= 0 && Rand.MTBEventOccurs(Props.feralMtbDays, 60000f, 1f))
			{
				TryMakeFeral();
			}
		}
	}

	private static bool CanGoFeral(Pawn pawn)
	{
		if (!pawn.Spawned || !pawn.Awake())
		{
			return false;
		}
		return pawn.IsColonyMechRequiringMechanitor();
	}

	public bool TryMakeFeral()
	{
		tmpFeralPawns.Clear();
		tmpFeralPawns.Add(Parent);
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, Props.feralCascadeRadialDistance, useCenter: true))
		{
			if (item is Pawn pawn && !tmpFeralPawns.Contains(pawn) && CanGoFeral(pawn))
			{
				tmpFeralPawns.Add(pawn);
			}
		}
		for (int i = 0; i < tmpFeralPawns.Count; i++)
		{
			tmpFeralPawns[i].OverseerSubject.ForceFeral();
		}
		IEnumerable<IGrouping<PawnKindDef, Pawn>> source = from p in tmpFeralPawns
			group p by p.kindDef;
		Find.LetterStack.ReceiveLetter("LetterLabelMechsFeral".Translate(), "LetterMechsFeral".Translate(Faction.OfMechanoids, source.Select((IGrouping<PawnKindDef, Pawn> g) => string.Concat(g.Key.LabelCap + " x", g.Count().ToString())).ToLineList(" - ")), LetterDefOf.ThreatBig, tmpFeralPawns);
		for (int num = 0; num < tmpFeralPawns.Count; num++)
		{
			tmpFeralPawns[num].GetLord()?.Notify_PawnLost(tmpFeralPawns[num], PawnLostCondition.ForcedToJoinOtherLord);
		}
		LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_ExitMapBest(LocomotionUrgency.Jog, canDig: false, canDefendSelf: true), parent.MapHeld, tmpFeralPawns);
		for (int num2 = 0; num2 < tmpFeralPawns.Count; num2++)
		{
			if (tmpFeralPawns[num2].CurJob != null)
			{
				tmpFeralPawns[num2].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		tmpFeralPawns.Clear();
		return true;
	}

	private void ForceFeral()
	{
		Pawn overseer = Overseer;
		if (overseer != null)
		{
			Messages.Message("MessageMechanitorLostControlOfMech".Translate(overseer, Parent) + ": " + Parent.LabelShortCap, new LookTargets(new Pawn[2] { Parent, overseer }), MessageTypeDefOf.NeutralEvent);
			Parent.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, overseer);
		}
		parent.SetFaction(Faction.OfMechanoids);
	}

	public override string CompInspectStringExtra()
	{
		if (parent.Faction == Faction.OfPlayer)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Pawn overseer = Overseer;
			TaggedString taggedString = "Overseer".Translate();
			if (overseer?.mechanitor != null)
			{
				taggedString += ": " + Overseer.LabelShort;
				if (!overseer.mechanitor.ControlledPawns.Contains(parent))
				{
					taggedString += " (" + "InsufficientBandwidth".Translate() + ")";
				}
			}
			else
			{
				taggedString += ": " + "OverseerNone".Translate();
			}
			stringBuilder.Append(taggedString);
			if (State != OverseerSubjectState.Overseen)
			{
				TaggedString ts = ((delayUntilFeralCheck > 0) ? ("Uncontrolled".Translate() + " (" + (Props.delayUntilFeralCheck - delayUntilFeralCheck).ToStringTicksToPeriod(allowSeconds: true, shortForm: true) + ")") : ("Danger".Translate() + ": " + "MayGoFeral".Translate()));
				stringBuilder.AppendInNewLine(ts.Colorize(ColorLibrary.RedReadable));
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	public override void Notify_AbandonedAtTile(PlanetTile tile)
	{
		base.Notify_AbandonedAtTile(tile);
		Pawn overseer = Overseer;
		if (overseer != null)
		{
			Parent.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, overseer);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "DEV: Make feral";
		command_Action.action = ForceFeral;
		yield return command_Action;
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = "DEV: Make feral (event)";
		command_Action2.action = delegate
		{
			TryMakeFeral();
		};
		yield return command_Action2;
		Command_Action command_Action3 = new Command_Action();
		command_Action3.defaultLabel = "DEV: Assign to overseer";
		command_Action3.action = delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<Pawn> freeColonists = parent.Map.mapPawns.FreeColonists;
			for (int i = 0; i < freeColonists.Count; i++)
			{
				Pawn localPawn = freeColonists[i];
				if (MechanitorUtility.IsMechanitor(localPawn))
				{
					list.Add(new FloatMenuOption(localPawn.LabelShortCap, delegate
					{
						foreach (Pawn item2 in Find.Selector.SelectedPawns.Where((Pawn p) => p.RaceProps.IsMechanoid))
						{
							item2.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, item2);
							item2.SetFaction(Faction.OfPlayer);
							localPawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, item2);
						}
					}));
				}
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
		yield return command_Action3;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref delayUntilFeralCheck, "delayUntilFeralCheck", 0);
	}
}
