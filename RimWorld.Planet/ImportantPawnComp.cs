using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public abstract class ImportantPawnComp : WorldObjectComp, ISuspendableThingHolder, IThingHolder, IThingHolderTickable
{
	public ThingOwner<Pawn> pawn;

	private const float AutoFoodLevel = 0.8f;

	public bool IsContentsSuspended => true;

	protected abstract string PawnSaveKey { get; }

	public bool ShouldTickContents => false;

	public ImportantPawnComp()
	{
		pawn = new ThingOwner<Pawn>(this, oneStackOnly: true);
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref pawn, PawnSaveKey, this);
		BackCompatibility.PostExposeData(this);
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return pawn;
	}

	public override void CompTick()
	{
		bool any = this.pawn.Any;
		this.pawn.DoTick();
		if (!any || base.ParentHasMap)
		{
			return;
		}
		if (!this.pawn.Any || this.pawn[0].Destroyed)
		{
			parent.Destroy();
			return;
		}
		Pawn pawn = this.pawn[0];
		if (pawn.needs.food != null)
		{
			pawn.needs.food.CurLevelPercentage = 0.8f;
		}
	}

	public override void PostDestroy()
	{
		base.PostDestroy();
		RemovePawnOnWorldObjectRemoved();
	}

	protected abstract void RemovePawnOnWorldObjectRemoved();
}
