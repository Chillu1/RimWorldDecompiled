using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Orbit : WITab
	{
		private Vector2 scrollPosition;

		private float lastDrawnHeight;

		private static readonly Vector2 WinSize = new Vector2(440f, 540f);

		public override bool IsVisible
		{
			get
			{
				if (ModsConfig.OdysseyActive && base.SelPlanetTile.Valid)
				{
					return base.SelPlanetTile.LayerDef == PlanetLayerDefOf.Orbit;
				}
				return false;
			}
		}

		public WITab_Orbit()
		{
			size = WinSize;
			labelKey = "TabOrbit";
			tutorTag = "Orbit";
		}

		protected override void FillTab()
		{
			Rect outRect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Rect rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, base.SelTile.PrimaryBiome.LabelCap);
			Rect rect2 = rect;
			rect2.yMin += 35f;
			rect2.height = 99999f;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.verticalSpacing = 0f;
			listing_Standard.Begin(rect2);
			DrawScrollContents(listing_Standard, rect2);
			listing_Standard.End();
			Widgets.EndScrollView();
		}

		private void DrawScrollContents(Listing_Standard listing, Rect infoRect)
		{
			Tile selTile = base.SelTile;
			listing.Label(selTile.PrimaryBiome.description);
			listing.Gap(8f);
			listing.GapLine();
			if (!selTile.PrimaryBiome.implemented)
			{
				listing.Label(string.Format("{0} {1}", selTile.PrimaryBiome.LabelCap, "BiomeNotImplemented".Translate()));
			}
			ListOrbitalDetails(listing, selTile, base.SelPlanetTile);
			listing.GapLine();
			ListMiscDetails(listing, selTile, base.SelPlanetTile);
			lastDrawnHeight = infoRect.y + listing.CurHeight;
		}

		private static void ListOrbitalDetails(Listing_Standard listing, Tile ws, PlanetTile tile)
		{
			listing.LabelDouble("Elevation".Translate(), ws.Layer.Def.elevationString.Formatted(ws.elevation.ToString("F0")));
			listing.LabelDouble("AvgTemp".Translate(), GenTemperature.GetAverageTemperatureLabel(tile));
		}

		private static void ListMiscDetails(Listing_Standard listing, Tile ws, PlanetTile tile)
		{
			listing.LabelDouble("TimeZone".Translate(), GenDate.TimeZoneAt(Find.WorldGrid.LongLatOf(tile).x).ToStringWithSign());
			if (Prefs.DevMode)
			{
				listing.LabelDouble("Debug world tile ID", tile.ToString());
			}
		}
	}
}
