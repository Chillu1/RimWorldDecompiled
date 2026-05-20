using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Building_Crate : Building_Casket, IThingGlower
{
	public const string CrateContentsChanged = "CrateContentsChanged";

	private CompHackable hackable;

	private static List<Pawn> tmpAllowedPawns = new List<Pawn>();

	public override int OpenTicks => 100;

	public override bool CanOpen
	{
		get
		{
			if (hackable == null)
			{
				hackable = GetComp<CompHackable>();
			}
			if (hackable != null && !hackable.IsHacked)
			{
				return false;
			}
			return base.CanOpen;
		}
	}

	public bool ShouldBeLitNow()
	{
		return base.HasAnyContents;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (def != ThingDefOf.AncientSecurityCrate || ModLister.CheckIdeology("Ancient security crate"))
		{
			base.SpawnSetup(map, respawningAfterLoad);
		}
	}

	public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
	{
		if (base.TryAcceptThing(thing, allowSpecialEffects))
		{
			BroadcastCompSignal("CrateContentsChanged");
			return true;
		}
		return false;
	}

	public override void EjectContents()
	{
		innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near, null, (IntVec3 c) => c.GetEdifice(base.Map) == null);
		contentsKnown = true;
		if (def.building.openingEffect != null)
		{
			Effecter effecter = def.building.openingEffect.Spawn();
			effecter.Trigger(new TargetInfo(base.Position, base.Map), null);
			effecter.Cleanup();
		}
		BroadcastCompSignal("CrateContentsChanged");
	}

	protected override void ReceiveCompSignal(string signal)
	{
		base.ReceiveCompSignal(signal);
		if (signal == "Hacked")
		{
			Open();
		}
	}

	public override void Open()
	{
		contentsKnown = true;
		if (CanOpen)
		{
			base.Open();
		}
	}

	public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
	{
		foreach (FloatMenuOption multiSelectFloatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
		{
			yield return multiSelectFloatMenuOption;
		}
		if (!CanOpen)
		{
			yield break;
		}
		tmpAllowedPawns.Clear();
		foreach (Pawn selPawn in selPawns)
		{
			if (selPawn.RaceProps.Humanlike && selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
			{
				tmpAllowedPawns.Add(selPawn);
			}
		}
		if (tmpAllowedPawns.Count <= 0)
		{
			yield return new FloatMenuOption("CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		tmpAllowedPawns.Clear();
		foreach (Pawn selPawn2 in selPawns)
		{
			if (selPawn2.RaceProps.Humanlike && IsCapableOfOpening(selPawn2))
			{
				tmpAllowedPawns.Add(selPawn2);
			}
		}
		if (tmpAllowedPawns.Count <= 0)
		{
			yield return new FloatMenuOption("CannotOpen".Translate(Label) + ": " + "Incapable".Translate(), null);
			yield break;
		}
		tmpAllowedPawns.Clear();
		foreach (Pawn selPawn3 in selPawns)
		{
			if (selPawn3.RaceProps.Humanlike && IsCapableOfOpening(selPawn3) && selPawn3.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
			{
				tmpAllowedPawns.Add(selPawn3);
			}
		}
		if (tmpAllowedPawns.Count <= 0)
		{
			yield break;
		}
		yield return new FloatMenuOption("Open".Translate(this), delegate
		{
			tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), JobTag.Misc);
			for (int i = 1; i < tmpAllowedPawns.Count; i++)
			{
				FloatMenuOptionProvider_DraftedMove.PawnGotoAction(base.Position, tmpAllowedPawns[i], RCellFinder.BestOrderedGotoDestNear(base.Position, tmpAllowedPawns[i]));
			}
		});
	}

	private bool IsCapableOfOpening(Pawn pawn)
	{
		if (!pawn.IsSubhuman)
		{
			return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
		}
		return false;
	}
}
