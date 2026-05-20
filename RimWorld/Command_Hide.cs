using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class Command_Hide : Command_Toggle
{
	protected readonly IHideable hideable;

	public IHideable Hideable => hideable;

	public override string Label => Hideable.Hidden ? "CommandUnhideLabel".Translate() : "CommandHideLabel".Translate();

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			foreach (FloatMenuOption option in GetOptions())
			{
				yield return option;
			}
		}
	}

	public Command_Hide(IHideable hideable)
	{
		this.hideable = hideable;
		icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone");
		defaultDesc = "CommandHideZoneDesc".Translate();
		hotKey = KeyBindingDefOf.Misc2;
		isActive = () => hideable.Hidden;
		toggleAction = delegate
		{
			hideable.Hidden = !hideable.Hidden;
		};
	}

	protected abstract IEnumerable<FloatMenuOption> GetOptions();

	protected static IEnumerable<FloatMenuOption> ZoneTypeOptions<T>(string label, Texture2D zoneIcon) where T : Zone
	{
		yield return new FloatMenuOption("ShowAllZoneTypes".Translate(label), delegate
		{
			ToggleZones<T>(hidden: false);
		}, zoneIcon, Color.white);
		yield return new FloatMenuOption("HideAllZoneTypes".Translate(label), delegate
		{
			ToggleZones<T>(hidden: true);
		}, zoneIcon, Color.gray);
	}

	protected static void ToggleAll(bool hidden)
	{
		foreach (Zone allZone in Find.CurrentMap.zoneManager.AllZones)
		{
			allZone.Hidden = hidden;
		}
		TogglePlans(hidden);
	}

	protected static void TogglePlans(bool hidden)
	{
		foreach (Plan allPlan in Find.CurrentMap.planManager.AllPlans)
		{
			allPlan.Hidden = hidden;
		}
	}

	protected static void ToggleZones<T>(bool hidden) where T : Zone
	{
		foreach (Zone allZone in Find.CurrentMap.zoneManager.AllZones)
		{
			if (allZone is T)
			{
				allZone.Hidden = hidden;
			}
		}
	}
}
