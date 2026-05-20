using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompEggContainer : CompThingContainer
{
	public bool CanEmpty
	{
		get
		{
			if (base.Empty)
			{
				return false;
			}
			if (EggsRuined)
			{
				return true;
			}
			return base.ContainedThing.stackCount >= base.Props.minCountToEmpty;
		}
	}

	private bool EggsRuined
	{
		get
		{
			if (base.Empty)
			{
				return false;
			}
			CompTemperatureRuinable compTemperatureRuinable = base.ContainedThing.TryGetComp<CompTemperatureRuinable>();
			if (compTemperatureRuinable != null && compTemperatureRuinable.Ruined)
			{
				return true;
			}
			CompHatcher compHatcher = base.ContainedThing.TryGetComp<CompHatcher>();
			if (compHatcher != null && compHatcher.TemperatureDamaged)
			{
				return true;
			}
			return false;
		}
	}

	public override bool Accepts(ThingDef thingDef)
	{
		if (base.ContainedThing != null && EggsRuined)
		{
			return false;
		}
		if (thingDef.thingCategories != null && (thingDef.thingCategories.Contains(ThingCategoryDefOf.EggsFertilized) || thingDef.thingCategories.Contains(ThingCategoryDefOf.EggsUnfertilized)))
		{
			return base.Accepts(thingDef);
		}
		return false;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!base.Empty && !EggsRuined)
		{
			CompHatcher compHatcher = base.ContainedThing.TryGetComp<CompHatcher>();
			if (compHatcher != null)
			{
				string text2 = compHatcher.CompInspectStringExtra();
				if (!text2.NullOrEmpty())
				{
					text = text + "\n" + text2;
				}
			}
		}
		return text;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos && base.Empty)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fill with eggs",
				action = delegate
				{
					Thing thing = ThingMaker.MakeThing(ThingDef.Named("EggChickenUnfertilized"));
					thing.stackCount = base.Props.stackLimit;
					innerContainer.TryAdd(thing);
				}
			};
		}
	}
}
