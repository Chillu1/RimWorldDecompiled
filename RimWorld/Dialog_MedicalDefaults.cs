using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_MedicalDefaults : Window
{
	private const float MedicalCareLabelWidth = 230f;

	private const float VerticalGap = 6f;

	public override Vector2 InitialSize => new Vector2(406f, 640f);

	public override string CloseButtonText => "OK".Translate();

	public Dialog_MedicalDefaults()
	{
		forcePause = true;
		doCloseX = true;
		doCloseButton = true;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		float y = 0f;
		using (new TextBlock(GameFont.Medium))
		{
			Widgets.Label(inRect, ref y, "DefaultMedicineSettings".Translate());
		}
		Text.Font = GameFont.Small;
		Widgets.Label(inRect, ref y, "DefaultMedicineSettingsDesc".Translate());
		y += 10f;
		Text.Anchor = TextAnchor.MiddleLeft;
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForColonist, "MedGroupColonists", "MedGroupColonistsDesc");
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForPrisoner, "MedGroupPrisoners", "MedGroupPrisonersDesc");
		if (ModsConfig.IdeologyActive)
		{
			DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForSlave, "MedGroupSlaves", "MedGroupSlavesDesc");
		}
		if (ModsConfig.AnomalyActive)
		{
			DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForGhouls, "MedGroupGhouls", "MedGroupGhoulsDesc");
		}
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForTamedAnimal, "MedGroupTamedAnimals", "MedGroupTamedAnimalsDesc");
		y += 17f;
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForFriendlyFaction, "MedGroupFriendlyFaction", "MedGroupFriendlyFactionDesc");
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForNeutralFaction, "MedGroupNeutralFaction", "MedGroupNeutralFactionDesc");
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForHostileFaction, "MedGroupHostileFaction", "MedGroupHostileFactionDesc");
		y += 17f;
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForNoFaction, "MedGroupNoFaction", "MedGroupNoFactionDesc");
		DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForWildlife, "MedGroupWildlife", "MedGroupWildlifeDesc");
		if (ModsConfig.AnomalyActive)
		{
			DoRow(inRect, ref y, ref Find.PlaySettings.defaultCareForEntities, "MedGroupEntities", "MedGroupEntitiesDesc");
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private void DoRow(Rect rect, ref float y, ref MedicalCareCategory category, string labelKey, string tipKey)
	{
		Rect rect2 = new Rect(rect.x, y, rect.width, 28f);
		Rect rect3 = new Rect(rect.x, y, 230f, 28f);
		Rect rect4 = new Rect(230f, y, 140f, 28f);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawLightHighlight(rect2);
		}
		TooltipHandler.TipRegionByKey(rect2, tipKey);
		Widgets.LabelFit(rect3, labelKey.Translate());
		MedicalCareUtility.MedicalCareSetter(rect4, ref category);
		y += 34f;
	}
}
