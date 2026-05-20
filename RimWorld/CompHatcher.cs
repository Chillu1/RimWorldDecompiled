using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompHatcher : ThingComp
{
	private float gestateProgress;

	public Pawn hatcheeParent;

	public Pawn otherParent;

	public Faction hatcheeFaction;

	public CompProperties_Hatcher Props => (CompProperties_Hatcher)props;

	private CompTemperatureRuinable FreezerComp => parent.GetComp<CompTemperatureRuinable>();

	public bool TemperatureDamaged
	{
		get
		{
			if (FreezerComp != null)
			{
				return FreezerComp.Ruined;
			}
			return false;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref gestateProgress, "gestateProgress", 0f);
		Scribe_References.Look(ref hatcheeParent, "hatcheeParent");
		Scribe_References.Look(ref otherParent, "otherParent");
		Scribe_References.Look(ref hatcheeFaction, "hatcheeFaction");
	}

	public override void CompTick()
	{
		if (!TemperatureDamaged)
		{
			float num = 1f / (Props.hatcherDaystoHatch * 60000f);
			gestateProgress += num;
			if (gestateProgress > 1f)
			{
				Hatch();
			}
			return;
		}
		CompProperties_EggLayer compProperties_EggLayer = Props.hatcherPawn?.race?.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties_EggLayer != null)
		{
			if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
			{
				pawn_CarryTracker.TryDropCarriedThing(parent.Position, ThingPlaceMode.Near, out var _);
			}
			if (parent.ParentHolder is CompEggContainer compEggContainer)
			{
				int stackCount = parent.stackCount;
				compEggContainer.innerContainer.Remove(parent);
				parent.Destroy();
				Thing thing = ThingMaker.MakeThing(compProperties_EggLayer.eggUnfertilizedDef);
				thing.stackCount = stackCount;
				compEggContainer.innerContainer.TryAdd(thing);
			}
			else
			{
				Map mapHeld = parent.MapHeld;
				IntVec3 positionHeld = parent.PositionHeld;
				int stackCount2 = parent.stackCount;
				parent.Destroy();
				GenSpawn.Spawn(compProperties_EggLayer.eggUnfertilizedDef, positionHeld, mapHeld).stackCount = stackCount2;
			}
		}
	}

	public void Hatch()
	{
		try
		{
			PawnGenerationRequest request = new PawnGenerationRequest(Props.hatcherPawn, hatcheeFaction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
			if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
			{
				pawn_CarryTracker.TryDropCarriedThing(parent.PositionHeld, ThingPlaceMode.Near, out var _);
			}
			for (int i = 0; i < parent.stackCount; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
				{
					if (pawn != null)
					{
						if (hatcheeParent != null)
						{
							if (pawn.playerSettings != null && hatcheeParent.playerSettings != null && hatcheeParent.Faction == hatcheeFaction)
							{
								pawn.playerSettings.AreaRestrictionInPawnCurrentMap = hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
							}
							if (pawn.RaceProps.IsFlesh)
							{
								pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, hatcheeParent);
							}
						}
						if (otherParent != null && (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) && pawn.RaceProps.IsFlesh)
						{
							pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, otherParent);
						}
					}
					if (parent.Spawned)
					{
						FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_AmnioticFluid);
					}
				}
				else
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
			}
		}
		finally
		{
			parent.Destroy();
		}
	}

	public override bool AllowStackWith(Thing other)
	{
		CompHatcher comp = ((ThingWithComps)other).GetComp<CompHatcher>();
		if (TemperatureDamaged != comp.TemperatureDamaged)
		{
			return false;
		}
		return base.AllowStackWith(other);
	}

	public override void PreAbsorbStack(Thing otherStack, int count)
	{
		float t = (float)count / (float)(parent.stackCount + count);
		float b = ((ThingWithComps)otherStack).GetComp<CompHatcher>().gestateProgress;
		gestateProgress = Mathf.Lerp(gestateProgress, b, t);
	}

	public override void PostSplitOff(Thing piece)
	{
		CompHatcher comp = ((ThingWithComps)piece).GetComp<CompHatcher>();
		comp.gestateProgress = gestateProgress;
		comp.hatcheeParent = hatcheeParent;
		comp.otherParent = otherParent;
		comp.hatcheeFaction = hatcheeFaction;
	}

	public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
	{
		base.PrePreTraded(action, playerNegotiator, trader);
		switch (action)
		{
		case TradeAction.PlayerBuys:
			hatcheeFaction = Faction.OfPlayer;
			break;
		case TradeAction.PlayerSells:
			hatcheeFaction = trader.Faction;
			break;
		}
	}

	public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
	{
		base.PostPostGeneratedForTrader(trader, forTile, forFaction);
		hatcheeFaction = forFaction;
	}

	public override string CompInspectStringExtra()
	{
		if (!TemperatureDamaged)
		{
			return "EggProgress".Translate() + ": " + gestateProgress.ToStringPercent() + "\n" + "HatchesIn".Translate() + ": " + "PeriodDays".Translate((Props.hatcherDaystoHatch * (1f - gestateProgress)).ToString("F1"));
		}
		return null;
	}
}
