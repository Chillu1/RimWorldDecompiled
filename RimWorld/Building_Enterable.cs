using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class Building_Enterable : Building, IThingHolder, ISuspendableThingHolder, ISearchableContents
{
	public ThingOwner innerContainer;

	protected Pawn selectedPawn;

	protected int startTick = -1;

	public bool Working => startTick >= 0;

	public virtual bool IsContentsSuspended => true;

	public bool AnyAcceptablePawns
	{
		get
		{
			if (!base.Spawned)
			{
				return false;
			}
			return base.Map.mapPawns.AllPawnsSpawned.Any((Pawn x) => CanAcceptPawn(x));
		}
	}

	public ThingOwner SearchableContents => innerContainer;

	public Pawn SelectedPawn
	{
		get
		{
			return selectedPawn;
		}
		set
		{
			selectedPawn = value;
		}
	}

	public abstract Vector3 PawnDrawOffset { get; }

	public Building_Enterable()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public abstract AcceptanceReport CanAcceptPawn(Pawn p);

	public abstract void TryAcceptPawn(Pawn p);

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
		}
		base.DeSpawn(mode);
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	protected virtual void SelectPawn(Pawn pawn)
	{
		selectedPawn = pawn;
		if (!pawn.IsPrisonerOfColony && !pawn.Downed)
		{
			pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
		}
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (!Working && selectedPawn != null && selectedPawn.Map == base.Map)
		{
			GenDraw.DrawLineBetween(this.TrueCenter(), selectedPawn.TrueCenter());
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo2 in base.GetGizmos())
		{
			yield return gizmo2;
		}
		foreach (Thing item in (IEnumerable<Thing>)innerContainer)
		{
			Gizmo gizmo = Building.SelectContainedItemGizmo(this, item);
			if (gizmo != null)
			{
				yield return gizmo;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref startTick, "startTick", -1);
		Scribe_References.Look(ref selectedPawn, "selectedPawn");
	}
}
