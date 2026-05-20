using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ITab_WindTurbineAutoCut : ITab
	{
		private static readonly Vector2 WinSize = new Vector2(300f, 480f);

		private const int CutNowButtonWidth = 150;

		private const int CutNowButtonHeight = 27;

		private const float AutoCutRowHeight = 24f;

		private ThingFilterUI.UIState plantFilterState = new ThingFilterUI.UIState();

		public CompAutoCut AutoCut => base.SelThing?.TryGetComp<CompAutoCut>();

		public override bool IsVisible => AutoCut != null;

		public ITab_WindTurbineAutoCut()
		{
			size = WinSize;
			labelKey = "TabWindTurbineAutoCut";
		}

		public override void OnOpen()
		{
			base.OnOpen();
			plantFilterState.quickSearch.Reset();
		}

		protected override void FillTab()
		{
			CompAutoCut autoCut = AutoCut;
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Widgets.BeginGroup(rect);
			float curY = 0f;
			DrawAutoCutOptions(ref curY, rect.width, autoCut);
			curY += 4f;
			DrawPlantFilter(ref curY, rect.width, rect.height - curY, autoCut);
			Widgets.EndGroup();
		}

		private void DrawPlantFilter(ref float curY, float width, float height, CompAutoCut autoCut)
		{
			ThingFilterUI.DoThingFilterConfigWindow(new Rect(0f, curY, width, height), plantFilterState, autoCut.AutoCutFilter, autoCut.GetFixedAutoCutFilter(), 1, null, map: autoCut.parent.Map, forceHiddenFilters: HiddenSpecialThingFilters(), forceHideHitPointsConfig: true);
		}

		private void DrawAutoCutOptions(ref float curY, float width, CompAutoCut autoCut)
		{
			Designator_PlantsCut designator_PlantsCut = Find.ReverseDesignatorDatabase.Get<Designator_PlantsCut>();
			Rect position = new Rect(0f, curY, 24f, 24f);
			Rect rect = new Rect(position.xMax + 4f, curY, width, 24f);
			Rect rect2 = new Rect(0f, rect.yMax + 4f, 150f, 27f);
			GUI.DrawTexture(position, designator_PlantsCut.icon);
			Text.Font = GameFont.Tiny;
			Widgets.CheckboxLabeled(rect, "WindTurbineAutoCut_EnabledCheckbox".Translate(), ref autoCut.autoCut, disabled: false, null, null, placeCheckboxNearText: true);
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(rect2, "AutoCutNow".Translate()))
			{
				autoCut.DesignatePlantsToCut();
				designator_PlantsCut.soundSucceeded?.PlayOneShotOnCamera();
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
