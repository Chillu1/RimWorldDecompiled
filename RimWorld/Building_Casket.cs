using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_Casket : Building, IThingHolder, IOpenable, ISearchableContents
{
	protected ThingOwner innerContainer;

	protected bool contentsKnown;

	public string openedSignal;

	public virtual int OpenTicks => 300;

	public bool HasAnyContents => innerContainer.Count > 0;

	public Thing ContainedThing
	{
		get
		{
			if (innerContainer.Count != 0)
			{
				return innerContainer[0];
			}
			return null;
		}
	}

	public virtual bool CanOpen => HasAnyContents;

	public ThingOwner SearchableContents => innerContainer;

	public Building_Casket()
	{
		innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public virtual void Open()
	{
		if (HasAnyContents)
		{
			EjectContents();
			if (!openedSignal.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(openedSignal, this.Named("SUBJECT")));
			}
			DirtyMapMesh(base.Map);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo2 in base.GetGizmos())
		{
			yield return gizmo2;
		}
		Gizmo gizmo = Building.SelectContainedItemGizmo(this, ContainedThing);
		if (gizmo != null)
		{
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos && CanOpen)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Open",
				action = Open
			};
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref contentsKnown, "contentsKnown", defaultValue: false);
		Scribe_Values.Look(ref openedSignal, "openedSignal");
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (base.Faction != null && base.Faction.IsPlayer)
		{
			contentsKnown = true;
		}
	}

	public override AcceptanceReport ClaimableBy(Faction fac)
	{
		if (innerContainer.Any && !contentsKnown)
		{
			return false;
		}
		return base.ClaimableBy(fac);
	}

	public virtual bool Accepts(Thing thing)
	{
		return innerContainer.CanAcceptAnyOf(thing);
	}

	public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
	{
		if (!Accepts(thing))
		{
			return false;
		}
		bool flag;
		if (thing.holdingOwner != null)
		{
			thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
			flag = true;
		}
		else
		{
			flag = innerContainer.TryAdd(thing);
		}
		if (flag)
		{
			if (thing.Faction != null && thing.Faction.IsPlayer)
			{
				contentsKnown = true;
			}
			return true;
		}
		return false;
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.Destroy(mode);
		if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
		{
			if (mode != DestroyMode.Deconstruct)
			{
				List<Pawn> list = new List<Pawn>();
				foreach (Thing item2 in (IEnumerable<Thing>)innerContainer)
				{
					if (item2 is Pawn item)
					{
						list.Add(item);
					}
				}
				foreach (Pawn item3 in list)
				{
					HealthUtility.DamageUntilDowned(item3);
				}
			}
			innerContainer.TryDropAll(base.Position, map, ThingPlaceMode.Near);
		}
		innerContainer.ClearAndDestroyContents();
	}

	public virtual void EjectContents()
	{
		innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
		contentsKnown = true;
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		string str = (contentsKnown ? innerContainer.ContentsString : ((string)"UnknownLower".Translate()));
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + ("CasketContains".Translate() + ": " + str.CapitalizeFirst());
	}
}
