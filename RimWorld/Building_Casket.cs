using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_Casket : Building, IThingHolder, IOpenable
	{
		protected ThingOwner innerContainer;

		protected bool contentsKnown;

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

		public bool CanOpen => HasAnyContents;

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

		public override void TickRare()
		{
			base.TickRare();
			innerContainer.ThingOwnerTickRare();
		}

		public override void Tick()
		{
			base.Tick();
			innerContainer.ThingOwnerTick();
		}

		public virtual void Open()
		{
			if (HasAnyContents)
			{
				EjectContents();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref contentsKnown, "contentsKnown", defaultValue: false);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				contentsKnown = true;
			}
		}

		public override bool ClaimableBy(Faction fac)
		{
			if (innerContainer.Any)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i].Faction == fac)
					{
						return true;
					}
				}
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
			bool flag = false;
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
			if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing item in (IEnumerable<Thing>)innerContainer)
					{
						Pawn pawn = item as Pawn;
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn item2 in list)
					{
						HealthUtility.DamageUntilDowned(item2);
					}
				}
				EjectContents();
			}
			innerContainer.ClearAndDestroyContents();
			base.Destroy(mode);
		}

		public virtual void EjectContents()
		{
			innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
			contentsKnown = true;
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			string str = contentsKnown ? innerContainer.ContentsString : ((string)"UnknownLower".Translate());
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("CasketContains".Translate() + ": " + str.CapitalizeFirst());
		}
	}
}
