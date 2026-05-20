using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_Hide_ZoneFishing : Command_Hide
{
	private const string IconTexPath = "UI/Designators/ZoneCreate_Fishing";

	private static Texture2D cachedIcon;

	public Command_Hide_ZoneFishing(IHideable hideable)
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
		if ((object)cachedIcon == null)
		{
			cachedIcon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Fishing");
		}
		foreach (FloatMenuOption item in Command_Hide.ZoneTypeOptions<Zone_Fishing>("FishingGroup".Translate(), cachedIcon))
		{
			yield return item;
		}
	}
}
