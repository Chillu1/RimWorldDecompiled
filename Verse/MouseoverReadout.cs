using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class MouseoverReadout
	{
		private TerrainDef cachedTerrain;

		private string cachedTerrainString;

		private string[] glowStrings;

		private const float YInterval = 19f;

		private static readonly Vector2 BotLeft = new Vector2(15f, 65f);

		public MouseoverReadout()
		{
			MakePermaCache();
		}

		private void MakePermaCache()
		{
			glowStrings = new string[101];
			for (int i = 0; i <= 100; i++)
			{
				glowStrings[i] = GlowGrid.PsychGlowAtGlow((float)i / 100f).GetLabel() + " (" + ((float)i / 100f).ToStringPercent() + ")";
			}
		}

		public void MouseoverReadoutOnGUI()
		{
			if (Event.current.type != EventType.Repaint || Find.MainTabsRoot.OpenTab != null)
			{
				return;
			}
			GenUI.DrawTextWinterShadow(new Rect(256f, UI.screenHeight - 256, -256f, 256f));
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			IntVec3 c = UI.MouseCell();
			if (!c.InBounds(Find.CurrentMap))
			{
				return;
			}
			float num = 0f;
			if (c.Fogged(Find.CurrentMap))
			{
				Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), "Undiscovered".Translate());
				GUI.color = Color.white;
				return;
			}
			Rect rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
			int num2 = Mathf.RoundToInt(Find.CurrentMap.glowGrid.GameGlowAt(c) * 100f);
			Widgets.Label(rect, glowStrings[num2]);
			num += 19f;
			Rect rect2 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
			TerrainDef terrain = c.GetTerrain(Find.CurrentMap);
			if (terrain != cachedTerrain)
			{
				string t = ((double)terrain.fertility > 0.0001) ? (" " + "FertShort".TranslateSimple() + " " + terrain.fertility.ToStringPercent()) : "";
				cachedTerrainString = terrain.LabelCap + ((terrain.passability != Traversability.Impassable) ? (" (" + "WalkSpeed".Translate(SpeedPercentString(terrain.pathCost)) + t + ")") : ((TaggedString)null));
				cachedTerrain = terrain;
			}
			Widgets.Label(rect2, cachedTerrainString);
			num += 19f;
			Zone zone = c.GetZone(Find.CurrentMap);
			if (zone != null)
			{
				Rect rect3 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				string label = zone.label;
				Widgets.Label(rect3, label);
				num += 19f;
			}
			float depth = Find.CurrentMap.snowGrid.GetDepth(c);
			if (depth > 0.03f)
			{
				Rect rect4 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
				SnowCategory snowCategory = SnowUtility.GetSnowCategory(depth);
				string label2 = SnowUtility.GetDescription(snowCategory) + " (" + "WalkSpeed".Translate(SpeedPercentString(SnowUtility.MovementTicksAddOn(snowCategory))) + ")";
				Widgets.Label(rect4, label2);
				num += 19f;
			}
			List<Thing> thingList = c.GetThingList(Find.CurrentMap);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category != ThingCategory.Mote)
				{
					Rect rect5 = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
					string labelMouseover = thing.LabelMouseover;
					Widgets.Label(rect5, labelMouseover);
					num += 19f;
				}
			}
			RoofDef roof = c.GetRoof(Find.CurrentMap);
			if (roof != null)
			{
				Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f), roof.LabelCap);
				num += 19f;
			}
			GUI.color = Color.white;
		}

		private string SpeedPercentString(float extraPathTicks)
		{
			return (13f / (extraPathTicks + 13f)).ToStringPercent();
		}
	}
}
