using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class CompMechGestatorTank : ThingComp, IThingGlower
{
	public enum TankState
	{
		Empty,
		Dormant,
		Proximity
	}

	private TankState state;

	private float triggerRadius;

	private const int StunTicks = 180;

	private static readonly IntRange GestationFluidFilthRange = new IntRange(2, 4);

	private CompProperties_MechGestatorTank Props => (CompProperties_MechGestatorTank)props;

	public TankState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
			if (parent.Spawned)
			{
				parent.DirtyMapMesh(parent.Map);
				parent.TryGetComp<CompGlower>()?.UpdateLit(parent.Map);
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		return string.Format("{0}: {1}", "Contains".Translate(), ((state == TankState.Empty) ? "Nothing" : "Unknown").Translate().CapitalizeFirst());
	}

	public bool ShouldBeLitNow()
	{
		return state != TankState.Empty;
	}

	public override bool DontDrawParent()
	{
		return true;
	}

	public override void PostPostMake()
	{
		triggerRadius = Props.triggerRadiusRange.RandomInRange;
	}

	public override void CompTick()
	{
		if (state == TankState.Proximity && parent.IsHashIntervalTick(250))
		{
			CheckTrigger();
		}
	}

	private void CheckTrigger()
	{
		if (!parent.Spawned)
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, triggerRadius, useCenter: false))
		{
			if (!item.InBounds(parent.Map) || !GenSight.LineOfSight(parent.Position, item, parent.Map))
			{
				continue;
			}
			foreach (Thing thing in item.GetThingList(parent.Map))
			{
				if (thing is Pawn { IsColonist: not false })
				{
					Trigger(parent.Map);
					return;
				}
			}
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if (mode != DestroyMode.WillReplace && state != TankState.Empty)
		{
			Trigger(previousMap);
		}
	}

	private void Trigger(Map map)
	{
		if (state == TankState.Empty)
		{
			return;
		}
		State = TankState.Empty;
		IntVec3 loc = parent.OccupiedRect().ExpandedBy(1).EdgeCells.Where(Standable).RandomElementWithFallback(IntVec3.Invalid);
		ScatterDebrisUtility.ScatterFilthAroundThing(parent, map, ThingDefOf.Filth_GestationFluid, GestationFluidFilthRange);
		if (loc.IsValid)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(Props.mechKindOptions.RandomElementByWeight((PawnKindDefWeight x) => x.weight).kindDef, Faction.OfMechanoids);
			GenSpawn.Spawn(pawn, loc, map);
			pawn.stances?.stunner?.StunFor(180, null, addBattleLog: false);
			if (!map.lordManager.TryGetLordByJob<LordJob_AssaultColony>(Faction.OfMechanoids, out var lord))
			{
				LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony(Faction.OfMechanoids, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), map, new List<Pawn> { pawn });
			}
			else
			{
				lord.lord.AddPawn(pawn);
			}
			Messages.Message(Props.triggeredMessage.Formatted(pawn), pawn, MessageTypeDefOf.NegativeEvent);
			Props.triggerSound.PlayOneShot(parent);
		}
		bool Standable(IntVec3 c)
		{
			return c.Standable(map);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		if (State == TankState.Empty)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Add mech";
			command_Action.action = delegate
			{
				State = TankState.Proximity;
			};
			yield return command_Action;
		}
		else
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Remove mech";
			command_Action2.action = delegate
			{
				State = TankState.Empty;
			};
			yield return command_Action2;
		}
	}

	public override void PostPrintOnto(SectionLayer layer)
	{
		((state == TankState.Empty) ? Props.emptyGraphic.Graphic : Props.dormantGraphic.Graphic).Print(layer, parent, 0f);
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref state, "state", TankState.Empty);
		Scribe_Values.Look(ref triggerRadius, "triggerRadius", 0f);
	}
}
