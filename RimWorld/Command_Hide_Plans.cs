using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_Hide_Plans : Command_Hide
{
	private static readonly Texture2D PlanTex = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn");

	public Command_Hide_Plans(IHideable hideable)
		: base(hideable)
	{
	}

	protected override IEnumerable<FloatMenuOption> GetOptions()
	{
		return GetHideOptions();
	}

	public static IEnumerable<FloatMenuOption> GetHideOptions()
	{
		yield return new FloatMenuOption("ShowAllZones".Translate(), delegate
		{
			Command_Hide.ToggleAll(hidden: false);
		});
		yield return new FloatMenuOption("HideAllZones".Translate(), delegate
		{
			Command_Hide.ToggleAll(hidden: true);
		});
		yield return new FloatMenuOption("ShowAllZoneTypes".Translate("Planning".Translate()), delegate
		{
			Command_Hide.TogglePlans(hidden: false);
		}, PlanTex, Color.white);
		yield return new FloatMenuOption("HideAllZoneTypes".Translate("Planning".Translate()), delegate
		{
			Command_Hide.TogglePlans(hidden: true);
		}, PlanTex, Color.gray);
	}
}
