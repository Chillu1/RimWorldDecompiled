using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class MainTabWindow_Mechs : MainTabWindow_PawnTable
{
	private List<Color> cachedColors = new List<Color>();

	private const int SetColorButtonHeight = 32;

	private const int SetColorButtonWidth = 240;

	protected override PawnTableDef PawnTableDef => PawnTableDefOf.Mechs;

	protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
		where p.RaceProps.IsMechanoid && p.OverseerSubject != null
		select p;

	public override void PostOpen()
	{
		base.PostOpen();
		cachedColors = DefDatabase<ColorDef>.AllDefsListForReading.Select((ColorDef c) => c.color).Concat(Find.FactionManager.AllFactionsVisible.Select((Faction f) => f.Color)).Distinct()
			.ToList();
		cachedColors.SortByColor((Color c) => c);
	}

	public override void DoWindowContents(Rect rect)
	{
		if (!ModLister.BiotechInstalled)
		{
			return;
		}
		base.DoWindowContents(rect);
		Rect rect2 = new Rect(rect.x, rect.y, 240f, 32f);
		Text.Font = GameFont.Small;
		if (!Widgets.ButtonText(rect2, "ChooseMechColor".Translate()))
		{
			return;
		}
		Find.WindowStack.Add(new Dialog_ChooseColor("ChooseMechAccentColor".Translate(), Find.FactionManager.OfPlayer.AllegianceColor, cachedColors, delegate(Color color)
		{
			Find.FactionManager.OfPlayer.AllegianceColor = color;
			foreach (Pawn item in MechanitorUtility.MechsInPlayerFaction())
			{
				PortraitsCache.SetDirty(item);
			}
		}));
	}
}
