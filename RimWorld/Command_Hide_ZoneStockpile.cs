using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_Hide_ZoneStockpile : Command_Hide
{
	private static readonly Texture2D IconTex = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile");

	public Command_Hide_ZoneStockpile(IHideable hideable)
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
		foreach (FloatMenuOption item in Command_Hide.ZoneTypeOptions<Zone_Stockpile>("StockpileGroup".Translate(), IconTex))
		{
			yield return item;
		}
	}
}
