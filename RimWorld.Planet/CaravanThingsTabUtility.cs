using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class CaravanThingsTabUtility
{
	public const float MassColumnWidth = 60f;

	public const float SpaceAroundIcon = 4f;

	public const float SpecificTabButtonSize = 24f;

	public const float AbandonButtonSize = 24f;

	public const float AbandonSpecificCountButtonSize = 24f;

	public static readonly Texture2D AbandonButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon");

	public static readonly Texture2D AbandonSpecificCountButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/AbandonSpecificCount");

	public static readonly Texture2D SpecificTabButtonTex = ContentFinder<Texture2D>.Get("UI/Buttons/OpenSpecificTab");

	public static readonly Color OpenedSpecificTabButtonColor = new Color(0f, 0.8f, 0f);

	public static readonly Color OpenedSpecificTabButtonMouseoverColor = new Color(0f, 0.5f, 0f);

	public static void DoAbandonButton(Rect rowRect, Thing t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, AbandonButtonTex))
		{
			CaravanAbandonOrBanishUtility.TryAbandonOrBanishViaInterface(t, caravan);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => CaravanAbandonOrBanishUtility.GetAbandonOrBanishButtonTooltip(t, abandonSpecificCount: false), Gen.HashCombineInt(t.GetHashCode(), 1383004931));
		}
	}

	public static void DoAbandonButton(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, AbandonButtonTex))
		{
			CaravanAbandonOrBanishUtility.TryAbandonOrBanishViaInterface(t, caravan);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => CaravanAbandonOrBanishUtility.GetAbandonOrBanishButtonTooltip(t, abandonSpecificCount: false), Gen.HashCombineInt(t.GetHashCode(), 8476546));
		}
	}

	public static void DoAbandonSpecificCountButton(Rect rowRect, Thing t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, AbandonSpecificCountButtonTex))
		{
			CaravanAbandonOrBanishUtility.TryAbandonSpecificCountViaInterface(t, caravan);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => CaravanAbandonOrBanishUtility.GetAbandonOrBanishButtonTooltip(t, abandonSpecificCount: true), Gen.HashCombineInt(t.GetHashCode(), 1163428609));
		}
	}

	public static void DoAbandonSpecificCountButton(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, AbandonSpecificCountButtonTex))
		{
			CaravanAbandonOrBanishUtility.TryAbandonSpecificCountViaInterface(t, caravan);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, () => CaravanAbandonOrBanishUtility.GetAbandonOrBanishButtonTooltip(t, abandonSpecificCount: true), Gen.HashCombineInt(t.GetHashCode(), 1163428609));
		}
	}

	public static void DoOpenSpecificTabButton(Rect rowRect, Pawn p, ref Pawn specificTabForPawn)
	{
		Color baseColor = ((p == specificTabForPawn) ? OpenedSpecificTabButtonColor : Color.white);
		Color mouseoverColor = ((p == specificTabForPawn) ? OpenedSpecificTabButtonMouseoverColor : GenUI.MouseoverColor);
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, SpecificTabButtonTex, baseColor, mouseoverColor))
		{
			if (p == specificTabForPawn)
			{
				specificTabForPawn = null;
				SoundDefOf.TabClose.PlayOneShotOnCamera();
			}
			else
			{
				specificTabForPawn = p;
				SoundDefOf.TabOpen.PlayOneShotOnCamera();
			}
		}
		TooltipHandler.TipRegionByKey(rect, "OpenSpecificTabButtonTip");
		GUI.color = Color.white;
	}

	public static void DoOpenSpecificTabButtonInvisible(Rect rect, Pawn pawn, ref Pawn specificTabForPawn)
	{
		if (Widgets.ButtonInvisible(rect))
		{
			if (pawn == specificTabForPawn)
			{
				specificTabForPawn = null;
			}
			else
			{
				specificTabForPawn = pawn;
			}
			SoundDefOf.TabClose.PlayOneShotOnCamera();
		}
	}

	public static void DrawMass(TransferableImmutable transferable, Rect rect)
	{
		float num = 0f;
		for (int i = 0; i < transferable.things.Count; i++)
		{
			num += transferable.things[i].GetStatValue(StatDefOf.Mass) * (float)transferable.things[i].stackCount;
		}
		DrawMass(num, rect);
	}

	public static void DrawMass(Thing thing, Rect rect)
	{
		DrawMass(thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount, rect);
	}

	private static void DrawMass(float mass, Rect rect)
	{
		GUI.color = TransferableOneWayWidget.ItemMassColor;
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Widgets.Label(rect, mass.ToStringMass());
		Text.WordWrap = true;
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
	}
}
