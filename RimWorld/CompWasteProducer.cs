using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompWasteProducer : ThingComp
{
	private CompProperties_WasteProducer Props => (CompProperties_WasteProducer)props;

	public Thing Waste => parent.TryGetInnerInteractableThingOwner().FirstOrFallback((Thing t) => t.def == ThingDefOf.Wastepack);

	public bool CanEmptyNow
	{
		get
		{
			if (parent is Building_MechGestator { GestatingMech: not null })
			{
				return false;
			}
			return Waste != null;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotech("Waste producer"))
		{
			parent.Destroy();
		}
		else
		{
			base.PostSpawnSetup(respawningAfterLoad);
		}
	}

	public void ProduceWaste(int amountToMake)
	{
		int num = Mathf.CeilToInt((float)amountToMake / (float)ThingDefOf.Wastepack.stackLimit);
		for (int i = 0; i < num; i++)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Wastepack);
			thing.stackCount = Mathf.Min(amountToMake, ThingDefOf.Wastepack.stackLimit);
			if (parent.TryGetInnerInteractableThingOwner().TryAdd(thing))
			{
				amountToMake -= thing.stackCount;
				continue;
			}
			break;
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (Props.showContentsInInspectPane && Waste != null)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			int num = 0;
			foreach (Thing item in (IEnumerable<Thing>)parent.TryGetInnerInteractableThingOwner())
			{
				if (item.def == ThingDefOf.Wastepack)
				{
					num += item.stackCount;
				}
			}
			text = string.Concat(text, "Contents".Translate() + ": " + Waste.LabelCapNoCount + " x", num.ToString());
		}
		return text;
	}
}
