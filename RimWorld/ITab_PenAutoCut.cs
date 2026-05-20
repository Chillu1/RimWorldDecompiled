using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ITab_PenAutoCut : ITab_PenBase
	{
		private static readonly Vector2 WinSize = new Vector2(300f, 480f);

		private const float AutoCutRowHeight = 24f;

		private const int CutNowButtonWidth = 150;

		private const int CutNowButtonHeight = 27;

		private ThingFilterUI.UIState plantFilterState = new ThingFilterUI.UIState();

		public ITab_PenAutoCut()
		{
			size = WinSize;
			labelKey = "TabPenAutoCut";
		}

		public override void OnOpen()
		{
			base.OnOpen();
			plantFilterState.quickSearch.Reset();
		}

		protected override void FillTab()
		{
			CompAnimalPenMarker selectedCompAnimalPenMarker = base.SelectedCompAnimalPenMarker;
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Widgets.BeginGroup(rect);
			float curY = 0f;
			DrawAutoCutOptions(ref curY, rect.width, selectedCompAnimalPenMarker);
			curY += 4f;
			DrawPlantFilter(ref curY, rect.width, rect.height - curY, selectedCompAnimalPenMarker);
			Widgets.EndGroup();
		}

		private void DrawPlantFilter(ref float curY, float width, float height, CompAnimalPenMarker marker)
		{
			ThingFilterUI.DoThingFilterConfigWindow(new Rect(0f, curY, width, height), plantFilterState, marker.AutoCutFilter, marker.parent.Map.animalPenManager.GetFixedAutoCutFilter(), 1, null, map: marker.parent.Map, forceHiddenFilters: HiddenSpecialThingFilters(), forceHideHitPointsConfig: true);
		}

		private void DrawAutoCutOptions(ref float curY, float width, CompAnimalPenMarker marker)
		{
			Text.Font = GameFont.Small;
			bool enclosed = marker.PenState.Enclosed;
			Designator_PlantsCut designator_PlantsCut = Find.ReverseDesignatorDatabase.Get<Designator_PlantsCut>();
			Rect position = new Rect(0f, curY, 24f, 24f);
			Rect rect = new Rect(position.xMax + 4f, curY, width, 24f);
			Rect rect2 = new Rect(0f, rect.yMax + 4f, 150f, 27f);
			Rect rect3 = new Rect(0f, curY, width - 18f, 55f);
			Widgets.DrawHighlightIfMouseover(rect3);
			if (!enclosed)
			{
				GUI.color = Widgets.InactiveColor;
			}
			GUI.DrawTexture(position, designator_PlantsCut.icon);
			Widgets.CheckboxLabeled(rect, "PenAutoCut_EnabledCheckbox".Translate(), ref marker.autoCut, disabled: false, null, null, placeCheckboxNearText: true);
			GUI.color = Color.white;
			if (enclosed)
			{
				if (Widgets.ButtonText(rect2, "AutoCutNow".Translate()))
				{
					marker.DesignatePlantsToCut();
					designator_PlantsCut.soundSucceeded?.PlayOneShotOnCamera();
				}
			}
			else
			{
				GUI.color = ColorLibrary.RedReadable;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, "AutocutUnenclosedPen".Translate());
				Text.Font = GameFont.Small;
				GUI.color = Color.white;
			}
			if (Mouse.IsOver(rect3))
			{
				TaggedString tooltip = "PenAutoCut_EnabledCheckboxTip".Translate();
				if (!enclosed)
				{
					tooltip += "\n\n" + "AutocutUnenclosedPenTip".Translate().Colorize(ColorLibrary.RedReadable);
				}
				TooltipHandler.TipRegion(rect, () => tooltip.Resolve(), 19727181);
			}
			curY = rect2.yMax;
		}

		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			yield return SpecialThingFilterDefOf.AllowFresh;
			if (ModsConfig.IdeologyActive)
			{
				yield return SpecialThingFilterDefOf.AllowVegetarian;
				yield return SpecialThingFilterDefOf.AllowCarnivore;
				yield return SpecialThingFilterDefOf.AllowCannibal;
				yield return SpecialThingFilterDefOf.AllowInsectMeat;
			}
		}
	}
}
