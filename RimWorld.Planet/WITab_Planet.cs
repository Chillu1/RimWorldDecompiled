using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Planet : WITab
	{
		private static readonly Vector2 WinSize = new Vector2(400f, 150f);

		public override bool IsVisible => base.SelTileID >= 0;

		private string Desc
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("PlanetSeed".Translate());
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(Find.World.info.seedString);
				stringBuilder.Append("PlanetCoverageShort".Translate());
				stringBuilder.Append(": ");
				stringBuilder.AppendLine(Find.World.info.planetCoverage.ToStringPercent());
				return stringBuilder.ToString();
			}
		}

		public WITab_Planet()
		{
			size = WinSize;
			labelKey = "TabPlanet";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, Find.World.info.name);
			Rect rect2 = rect;
			rect2.yMin += 35f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, Desc);
		}
	}
}
