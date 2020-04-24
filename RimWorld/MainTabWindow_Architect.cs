using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabWindow_Architect : MainTabWindow
	{
		private List<ArchitectCategoryTab> desPanelsCached;

		public ArchitectCategoryTab selectedDesPanel;

		public const float WinWidth = 200f;

		private const float ButHeight = 32f;

		public float WinHeight
		{
			get
			{
				if (desPanelsCached == null)
				{
					CacheDesPanels();
				}
				return (float)Mathf.CeilToInt((float)desPanelsCached.Count / 2f) * 32f;
			}
		}

		public override Vector2 RequestedTabSize => new Vector2(200f, WinHeight);

		protected override float Margin => 0f;

		public MainTabWindow_Architect()
		{
			CacheDesPanels();
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (selectedDesPanel != null && selectedDesPanel.def.showPowerGrid)
			{
				OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
			}
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			if (selectedDesPanel != null)
			{
				selectedDesPanel.DesignationTabOnGUI();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			Text.Font = GameFont.Small;
			float num = inRect.width / 2f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < desPanelsCached.Count; i++)
			{
				Rect rect = new Rect(num2 * num, num3 * 32f, num, 32f);
				float height = rect.height;
				rect.height = height + 1f;
				if (num2 == 0f)
				{
					rect.width += 1f;
				}
				if (Widgets.ButtonTextSubtle(rect, desPanelsCached[i].def.LabelCap, 0f, 8f, SoundDefOf.Mouseover_Category, new Vector2(-1f, -1f)))
				{
					ClickedCategory(desPanelsCached[i]);
				}
				if (selectedDesPanel != desPanelsCached[i])
				{
					UIHighlighter.HighlightOpportunity(rect, desPanelsCached[i].def.cachedHighlightClosedTag);
				}
				num2 += 1f;
				if (num2 > 1f)
				{
					num2 = 0f;
					num3 += 1f;
				}
			}
		}

		private void CacheDesPanels()
		{
			desPanelsCached = new List<ArchitectCategoryTab>();
			foreach (DesignationCategoryDef item in DefDatabase<DesignationCategoryDef>.AllDefs.OrderByDescending((DesignationCategoryDef dc) => dc.order))
			{
				desPanelsCached.Add(new ArchitectCategoryTab(item));
			}
		}

		protected void ClickedCategory(ArchitectCategoryTab Pan)
		{
			if (selectedDesPanel == Pan)
			{
				selectedDesPanel = null;
			}
			else
			{
				selectedDesPanel = Pan;
			}
			SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
		}
	}
}
