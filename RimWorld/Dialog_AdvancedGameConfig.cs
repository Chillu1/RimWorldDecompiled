using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AdvancedGameConfig : Window
	{
		private int selTile = -1;

		private const float ColumnWidth = 200f;

		private static readonly int[] MapSizes = new int[6]
		{
			200,
			225,
			250,
			275,
			300,
			325
		};

		private static readonly int[] TestMapSizes = new int[2]
		{
			350,
			400
		};

		public override Vector2 InitialSize => new Vector2(700f, 500f);

		public Dialog_AdvancedGameConfig(int selTile)
		{
			doCloseButton = true;
			forcePause = true;
			absorbInputAroundWindow = true;
			this.selTile = selTile;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = 200f;
			listing_Standard.Begin(inRect.AtZero());
			Text.Font = GameFont.Medium;
			listing_Standard.Label("MapSize".Translate());
			Text.Font = GameFont.Small;
			IEnumerable<int> enumerable = MapSizes.AsEnumerable();
			if (Prefs.TestMapSizes)
			{
				enumerable = enumerable.Concat(TestMapSizes);
			}
			foreach (int item in enumerable)
			{
				switch (item)
				{
				case 200:
					listing_Standard.Label("MapSizeSmall".Translate());
					break;
				case 250:
					listing_Standard.Gap(10f);
					listing_Standard.Label("MapSizeMedium".Translate());
					break;
				case 300:
					listing_Standard.Gap(10f);
					listing_Standard.Label("MapSizeLarge".Translate());
					break;
				case 350:
					listing_Standard.Gap(10f);
					listing_Standard.Label("MapSizeExtreme".Translate());
					break;
				}
				string label = "MapSizeDesc".Translate(item, item * item);
				if (listing_Standard.RadioButton(label, Find.GameInitData.mapSize == item))
				{
					Find.GameInitData.mapSize = item;
				}
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("MapStartSeason".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Label("");
			if (listing_Standard.RadioButton("MapStartSeasonDefault".Translate(), Find.GameInitData.startingSeason == Season.Undefined))
			{
				Find.GameInitData.startingSeason = Season.Undefined;
			}
			if (listing_Standard.RadioButton(Season.Spring.LabelCap(), Find.GameInitData.startingSeason == Season.Spring))
			{
				Find.GameInitData.startingSeason = Season.Spring;
			}
			if (listing_Standard.RadioButton(Season.Summer.LabelCap(), Find.GameInitData.startingSeason == Season.Summer))
			{
				Find.GameInitData.startingSeason = Season.Summer;
			}
			if (listing_Standard.RadioButton(Season.Fall.LabelCap(), Find.GameInitData.startingSeason == Season.Fall))
			{
				Find.GameInitData.startingSeason = Season.Fall;
			}
			if (listing_Standard.RadioButton(Season.Winter.LabelCap(), Find.GameInitData.startingSeason == Season.Winter))
			{
				Find.GameInitData.startingSeason = Season.Winter;
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("Notice".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Label("");
			bool flag = false;
			if (selTile >= 0 && Find.GameInitData.startingSeason != 0)
			{
				float y = Find.WorldGrid.LongLatOf(selTile).y;
				if (GenTemperature.AverageTemperatureAtTileForTwelfth(selTile, Find.GameInitData.startingSeason.GetFirstTwelfth(y)) < 3f)
				{
					listing_Standard.Label("MapTemperatureDangerWarning".Translate());
					flag = true;
				}
			}
			if (Find.GameInitData.mapSize > 280)
			{
				listing_Standard.Label("MapSizePerformanceWarning".Translate());
				flag = true;
			}
			if (!flag)
			{
				listing_Standard.None();
			}
			listing_Standard.End();
		}
	}
}
