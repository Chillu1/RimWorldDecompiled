using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompThingContainer : ThingComp, IThingHolder, ISearchableContents
{
	public ThingOwner innerContainer;

	public CompProperties_ThingContainer Props => (CompProperties_ThingContainer)props;

	public Thing ContainedThing
	{
		get
		{
			if (!innerContainer.Any)
			{
				return null;
			}
			return innerContainer[0];
		}
	}

	public bool Empty => ContainedThing == null;

	public int TotalStackCount
	{
		get
		{
			if (!Empty)
			{
				return innerContainer.TotalStackCountOfDef(innerContainer[0].def);
			}
			return 0;
		}
	}

	public bool Full
	{
		get
		{
			if (!Empty)
			{
				return TotalStackCount >= Props.stackLimit;
			}
			return false;
		}
	}

	public string LabelCapWithTotalCount
	{
		get
		{
			if (!Empty)
			{
				return ContainedThing.LabelCapNoCount + " x" + TotalStackCount.ToStringCached();
			}
			return null;
		}
	}

	public ThingOwner SearchableContents => innerContainer;

	public CompThingContainer()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public virtual bool Accepts(ThingDef thingDef)
	{
		if (!Empty)
		{
			if (TotalStackCount < Props.stackLimit)
			{
				return thingDef == ContainedThing.def;
			}
			return false;
		}
		return true;
	}

	public virtual bool Accepts(Thing thing)
	{
		return Accepts(thing.def);
	}

	public override void CompTick()
	{
		innerContainer.DoTick();
	}

	public override void PostDraw()
	{
		if (!Empty && Props.drawContainedThing)
		{
			ContainedThing.DrawNowAt((parent.Position + Props.containedThingOffset.RotatedBy(parent.Rotation)).ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop));
		}
	}

	public override void DrawGUIOverlay()
	{
		if (parent.Spawned && Props.drawStackLabel && !Empty && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
		{
			GenMapUI.DrawThingLabel(parent, TotalStackCount.ToStringCached());
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}
	}

	public override string CompInspectStringExtra()
	{
		return "Contents".Translate() + ": " + (Empty ? ((string)"Nothing".Translate()) : LabelCapWithTotalCount);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (!Empty)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "Contents".Translate(), LabelCapWithTotalCount, LabelCapWithTotalCount, 1200, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(ContainedThing)));
		}
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Thing item in (IEnumerable<Thing>)innerContainer)
		{
			Gizmo gizmo = Building.SelectContainedItemGizmo(parent, item);
			if (gizmo != null)
			{
				yield return gizmo;
			}
		}
	}
}
