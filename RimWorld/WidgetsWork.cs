using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class WidgetsWork
	{
		public const float WorkBoxSize = 25f;

		public static readonly Texture2D WorkBoxBGTex_Awful = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Awful");

		public static readonly Texture2D WorkBoxBGTex_Bad = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Bad");

		private const int AwfulBGMax = 4;

		public static readonly Texture2D WorkBoxBGTex_Mid = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Mid");

		private const int BadBGMax = 14;

		public static readonly Texture2D WorkBoxBGTex_Excellent = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Excellent");

		public static readonly Texture2D WorkBoxCheckTex = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxCheck");

		public static readonly Texture2D PassionWorkboxMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinorGray");

		public static readonly Texture2D PassionWorkboxMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajorGray");

		public static readonly Texture2D WorkBoxOverlay_Warning = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxOverlay_Warning");

		private const int WarnIfSelectedMax = 2;

		private const float PassionOpacity = 0.4f;

		public static Color ColorOfPriority(int prio)
		{
			return prio switch
			{
				1 => new Color(0f, 1f, 0f), 
				2 => new Color(1f, 0.9f, 0.5f), 
				3 => new Color(0.8f, 0.7f, 0.5f), 
				4 => new Color(0.74f, 0.74f, 0.74f), 
				_ => Color.grey, 
			};
		}

		public static void DrawWorkBoxFor(float x, float y, Pawn p, WorkTypeDef wType, bool incapableBecauseOfCapacities)
		{
			if (p.WorkTypeIsDisabled(wType))
			{
				return;
			}
			Rect rect = new Rect(x, y, 25f, 25f);
			if (incapableBecauseOfCapacities)
			{
				GUI.color = new Color(1f, 0.3f, 0.3f);
			}
			DrawWorkBoxBackground(rect, p, wType);
			GUI.color = Color.white;
			if (Find.PlaySettings.useWorkPriorities)
			{
				int priority = p.workSettings.GetPriority(wType);
				if (priority > 0)
				{
					Text.Anchor = TextAnchor.MiddleCenter;
					GUI.color = ColorOfPriority(priority);
					Widgets.Label(rect.ContractedBy(-3f), priority.ToStringCached());
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
				}
				if (Event.current.type != 0 || !Mouse.IsOver(rect))
				{
					return;
				}
				bool num = p.workSettings.WorkIsActive(wType);
				if (Event.current.button == 0)
				{
					int num2 = p.workSettings.GetPriority(wType) - 1;
					if (num2 < 0)
					{
						num2 = 4;
					}
					p.workSettings.SetPriority(wType, num2);
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (Event.current.button == 1)
				{
					int num3 = p.workSettings.GetPriority(wType) + 1;
					if (num3 > 4)
					{
						num3 = 0;
					}
					p.workSettings.SetPriority(wType, num3);
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (!num && p.workSettings.WorkIsActive(wType) && wType.relevantSkills.Any() && p.skills.AverageOfRelevantSkillsFor(wType) <= 2f)
				{
					SoundDefOf.Crunch.PlayOneShotOnCamera();
				}
				Event.current.Use();
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorkTab, KnowledgeAmount.SpecificInteraction);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ManualWorkPriorities, KnowledgeAmount.SmallInteraction);
				return;
			}
			if (p.workSettings.GetPriority(wType) > 0)
			{
				GUI.DrawTexture(rect, WorkBoxCheckTex);
			}
			if (!Widgets.ButtonInvisible(rect))
			{
				return;
			}
			if (p.workSettings.GetPriority(wType) > 0)
			{
				p.workSettings.SetPriority(wType, 0);
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			else
			{
				p.workSettings.SetPriority(wType, 3);
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				if (wType.relevantSkills.Any() && p.skills.AverageOfRelevantSkillsFor(wType) <= 2f)
				{
					SoundDefOf.Crunch.PlayOneShotOnCamera();
				}
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.WorkTab, KnowledgeAmount.SpecificInteraction);
		}

		public static string TipForPawnWorker(Pawn p, WorkTypeDef wDef, bool incapableBecauseOfCapacities)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string str = wDef.gerundLabel.CapitalizeFirst();
			int priority = p.workSettings.GetPriority(wDef);
			str = str + ": " + ColoredText.Colorize(("Priority" + priority).Translate(), ColorOfPriority(priority));
			stringBuilder.AppendLine(str);
			if (p.WorkTypeIsDisabled(wDef))
			{
				stringBuilder.Append("CannotDoThisWork".Translate(p.LabelShort, p));
			}
			else
			{
				float num = p.skills.AverageOfRelevantSkillsFor(wDef);
				if (wDef.relevantSkills.Any())
				{
					string text = "";
					foreach (SkillDef relevantSkill in wDef.relevantSkills)
					{
						text = text + relevantSkill.skillLabel.CapitalizeFirst() + ", ";
					}
					text = text.Substring(0, text.Length - 2);
					stringBuilder.AppendLine("RelevantSkills".Translate(text, num.ToString("0.#"), 20));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append(wDef.description);
				if (incapableBecauseOfCapacities)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("IncapableOfWorkTypeBecauseOfCapacities".Translate());
				}
				if (wDef.relevantSkills.Any() && num <= 2f && p.workSettings.WorkIsActive(wDef))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("SelectedWorkTypeWithVeryBadSkill".Translate());
				}
			}
			return stringBuilder.ToString();
		}

		private static void DrawWorkBoxBackground(Rect rect, Pawn p, WorkTypeDef workDef)
		{
			float num = p.skills.AverageOfRelevantSkillsFor(workDef);
			Texture2D image;
			Texture2D image2;
			float a;
			if (num < 4f)
			{
				image = WorkBoxBGTex_Awful;
				image2 = WorkBoxBGTex_Bad;
				a = num / 4f;
			}
			else if (num <= 14f)
			{
				image = WorkBoxBGTex_Bad;
				image2 = WorkBoxBGTex_Mid;
				a = (num - 4f) / 10f;
			}
			else
			{
				image = WorkBoxBGTex_Mid;
				image2 = WorkBoxBGTex_Excellent;
				a = (num - 14f) / 6f;
			}
			GUI.DrawTexture(rect, image);
			GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, a);
			GUI.DrawTexture(rect, image2);
			if (workDef.relevantSkills.Any() && num <= 2f && p.workSettings.WorkIsActive(workDef))
			{
				GUI.color = Color.white;
				GUI.DrawTexture(rect.ContractedBy(-2f), WorkBoxOverlay_Warning);
			}
			Passion passion = p.skills.MaxPassionOfRelevantSkillsFor(workDef);
			if ((int)passion > 0)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.4f);
				Rect position = rect;
				position.xMin = rect.center.x;
				position.yMin = rect.center.y;
				switch (passion)
				{
				case Passion.Minor:
					GUI.DrawTexture(position, PassionWorkboxMinorIcon);
					break;
				case Passion.Major:
					GUI.DrawTexture(position, PassionWorkboxMajorIcon);
					break;
				}
			}
			GUI.color = Color.white;
		}
	}
}
