using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SkillUI
	{
		public enum SkillDrawMode : byte
		{
			Gameplay,
			Menu
		}

		private static float levelLabelWidth = -1f;

		private static List<SkillDef> skillDefsInListOrderCached;

		private const float SkillWidth = 230f;

		public const float SkillHeight = 24f;

		public const float SkillYSpacing = 3f;

		private const float LeftEdgeMargin = 6f;

		private const float IncButX = 205f;

		private const float IncButSpacing = 10f;

		private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);

		private static Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");

		private static Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor");

		private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

		public static void Reset()
		{
			skillDefsInListOrderCached = DefDatabase<SkillDef>.AllDefs.OrderByDescending((SkillDef sd) => sd.listOrder).ToList();
		}

		public static void DrawSkillsOf(Pawn p, Vector2 offset, SkillDrawMode mode)
		{
			Text.Font = GameFont.Small;
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				float x = Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst()).x;
				if (x > levelLabelWidth)
				{
					levelLabelWidth = x;
				}
			}
			for (int j = 0; j < skillDefsInListOrderCached.Count; j++)
			{
				SkillDef skillDef = skillDefsInListOrderCached[j];
				float y = (float)j * 27f + offset.y;
				DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode);
			}
		}

		public static void DrawSkill(SkillRecord skill, Vector2 topLeft, SkillDrawMode mode, string tooltipPrefix = "")
		{
			DrawSkill(skill, new Rect(topLeft.x, topLeft.y, 230f, 24f), mode);
		}

		public static void DrawSkill(SkillRecord skill, Rect holdingRect, SkillDrawMode mode, string tooltipPrefix = "")
		{
			if (Mouse.IsOver(holdingRect))
			{
				GUI.DrawTexture(holdingRect, TexUI.HighlightTex);
			}
			GUI.BeginGroup(holdingRect);
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect = new Rect(6f, 0f, levelLabelWidth + 6f, holdingRect.height);
			Widgets.Label(rect, skill.def.skillLabel.CapitalizeFirst());
			Rect position = new Rect(rect.xMax, 0f, 24f, 24f);
			if (!skill.TotallyDisabled)
			{
				if ((int)skill.passion > 0)
				{
					Texture2D image = ((skill.passion == Passion.Major) ? PassionMajorIcon : PassionMinorIcon);
					GUI.DrawTexture(position, image);
				}
				Rect rect2 = new Rect(position.xMax, 0f, holdingRect.width - position.xMax, holdingRect.height);
				float fillPercent = Mathf.Max(0.01f, (float)skill.Level / 20f);
				Widgets.FillableBar(rect2, fillPercent, SkillBarFillTex, null, doBorder: false);
			}
			Rect rect3 = new Rect(position.xMax + 4f, 0f, 999f, holdingRect.height);
			rect3.yMin += 3f;
			string label;
			if (skill.TotallyDisabled)
			{
				GUI.color = DisabledSkillColor;
				label = "-";
			}
			else
			{
				label = skill.Level.ToStringCached();
			}
			GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
			Widgets.Label(rect3, label);
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
			GUI.EndGroup();
			if (Mouse.IsOver(holdingRect))
			{
				string text = GetSkillDescription(skill);
				if (tooltipPrefix != "")
				{
					text = tooltipPrefix + "\n\n" + text;
				}
				TooltipHandler.TipRegion(holdingRect, new TipSignal(text, skill.def.GetHashCode() * 397945));
			}
		}

		private static string GetSkillDescription(SkillRecord sk)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (sk.TotallyDisabled)
			{
				stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
			}
			else
			{
				stringBuilder.AppendLine((string)("Level".Translate() + " ") + sk.Level + ": " + sk.LevelDescriptor);
				if (Current.ProgramState == ProgramState.Playing)
				{
					string text = ((sk.Level == 20) ? "Experience".Translate() : "ProgressToNextLevel".Translate());
					stringBuilder.AppendLine(text + ": " + sk.xpSinceLastLevel.ToString("F0") + " / " + sk.XpRequiredForLevelUp);
				}
				stringBuilder.Append("Passion".Translate() + ": ");
				switch (sk.passion)
				{
				case Passion.None:
					stringBuilder.Append("PassionNone".Translate(0.35f.ToStringPercent("F0")));
					break;
				case Passion.Minor:
					stringBuilder.Append("PassionMinor".Translate(1f.ToStringPercent("F0")));
					break;
				case Passion.Major:
					stringBuilder.Append("PassionMajor".Translate(1.5f.ToStringPercent("F0")));
					break;
				}
				if (sk.LearningSaturatedToday)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("LearnedMaxToday".Translate(sk.xpSinceMidnight.ToString("F0"), 4000, 0.2f.ToStringPercent("F0")));
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(sk.def.description);
			return stringBuilder.ToString();
		}
	}
}
