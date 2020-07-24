using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TrainingCardUtility
	{
		public const float RowHeight = 28f;

		private const float InfoHeaderHeight = 50f;

		[TweakValue("Interface", -100f, 300f)]
		private static float TrainabilityLeft = 220f;

		[TweakValue("Interface", -100f, 300f)]
		private static float TrainabilityTop = 0f;

		private static readonly Texture2D LearnedTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheck");

		private static readonly Texture2D LearnedNotTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheckOff");

		public static void DrawTrainingCard(Rect rect, Pawn pawn)
		{
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(TrainabilityLeft, TrainabilityTop, 30f, 30f);
			TooltipHandler.TipRegionByKey(rect2, "RenameAnimal");
			if (Widgets.ButtonImage(rect2, TexButton.Rename))
			{
				Find.WindowStack.Add(new Dialog_NamePawn(pawn));
			}
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect);
			listing_Standard.Label("CreatureTrainability".Translate(pawn.def.label).CapitalizeFirst() + ": " + pawn.RaceProps.trainability.LabelCap, 22f);
			listing_Standard.Label("CreatureWildness".Translate(pawn.def.label).CapitalizeFirst() + ": " + pawn.RaceProps.wildness.ToStringPercent(), 22f, TrainableUtility.GetWildnessExplanation(pawn.def));
			if (pawn.training.HasLearned(TrainableDefOf.Obedience))
			{
				Rect rect3 = listing_Standard.GetRect(25f);
				Widgets.Label(rect3, "Master".Translate() + ": ");
				rect3.xMin = rect3.center.x;
				TrainableUtility.MasterSelectButton(rect3, pawn, paintable: false);
				listing_Standard.Gap();
				Rect rect4 = listing_Standard.GetRect(25f);
				bool checkOn = pawn.playerSettings.followDrafted;
				Widgets.CheckboxLabeled(rect4, "CreatureFollowDrafted".Translate(), ref checkOn);
				if (checkOn != pawn.playerSettings.followDrafted)
				{
					pawn.playerSettings.followDrafted = checkOn;
				}
				Rect rect5 = listing_Standard.GetRect(25f);
				bool checkOn2 = pawn.playerSettings.followFieldwork;
				Widgets.CheckboxLabeled(rect5, "CreatureFollowFieldwork".Translate(), ref checkOn2);
				if (checkOn2 != pawn.playerSettings.followFieldwork)
				{
					pawn.playerSettings.followFieldwork = checkOn2;
				}
			}
			listing_Standard.Gap();
			float num = 50f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (TryDrawTrainableRow(listing_Standard.GetRect(28f), pawn, trainableDefsInListOrder[i]))
				{
					num += 28f;
				}
			}
			listing_Standard.End();
		}

		public static float TotalHeightForPawn(Pawn p)
		{
			if (p == null)
			{
				return 0f;
			}
			int num = 0;
			for (int i = 0; i < DefDatabase<TrainableDef>.AllDefsListForReading.Count; i++)
			{
				p.training.CanAssignToTrain(DefDatabase<TrainableDef>.AllDefsListForReading[i], out bool visible);
				if (visible)
				{
					num++;
				}
			}
			float num2 = 112f + 28f * (float)num;
			if (p.training.HasLearned(TrainableDefOf.Obedience))
			{
				num2 += 75f;
				num2 += 12f;
			}
			return num2;
		}

		private static bool TryDrawTrainableRow(Rect rect, Pawn pawn, TrainableDef td)
		{
			bool flag = pawn.training.HasLearned(td);
			bool visible;
			AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out visible);
			if (!visible)
			{
				return false;
			}
			Widgets.DrawHighlightIfMouseover(rect);
			Rect rect2 = rect;
			rect2.width -= 50f;
			rect2.xMin += (float)td.indent * 10f;
			Rect rect3 = rect;
			rect3.xMin = rect3.xMax - 50f + 17f;
			DoTrainableCheckbox(rect2, pawn, td, canTrain, drawLabel: true, doTooltip: false);
			if (flag)
			{
				GUI.color = Color.green;
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect3, pawn.training.GetSteps(td) + " / " + td.steps);
			Text.Anchor = TextAnchor.UpperLeft;
			if (DebugSettings.godMode && !pawn.training.HasLearned(td))
			{
				Rect rect4 = rect3;
				rect4.yMin = rect4.yMax - 10f;
				rect4.xMin = rect4.xMax - 10f;
				if (Widgets.ButtonText(rect4, "+"))
				{
					pawn.training.Train(td, pawn.Map.mapPawns.FreeColonistsSpawned.RandomElement());
				}
			}
			DoTrainableTooltip(rect, pawn, td, canTrain);
			GUI.color = Color.white;
			return true;
		}

		public static void DoTrainableCheckbox(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain, bool drawLabel, bool doTooltip)
		{
			bool num = pawn.training.HasLearned(td);
			bool checkOn = pawn.training.GetWanted(td);
			bool flag = checkOn;
			Texture2D texChecked = num ? LearnedTrainingTex : null;
			Texture2D texUnchecked = num ? LearnedNotTrainingTex : null;
			if (drawLabel)
			{
				Widgets.CheckboxLabeled(rect, td.LabelCap, ref checkOn, !canTrain.Accepted, texChecked, texUnchecked);
			}
			else
			{
				Widgets.Checkbox(rect.position, ref checkOn, rect.width, !canTrain.Accepted, paintable: true, texChecked, texUnchecked);
			}
			if (checkOn != flag)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTraining, KnowledgeAmount.Total);
				pawn.training.SetWantedRecursive(td, checkOn);
			}
			if (doTooltip)
			{
				DoTrainableTooltip(rect, pawn, td, canTrain);
			}
		}

		private static void DoTrainableTooltip(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain)
		{
			if (!Mouse.IsOver(rect))
			{
				return;
			}
			TooltipHandler.TipRegion(rect, delegate
			{
				string text = td.LabelCap + "\n\n" + td.description;
				if (!canTrain.Accepted)
				{
					text = text + "\n\n" + canTrain.Reason;
				}
				else if (!td.prerequisites.NullOrEmpty())
				{
					text += "\n";
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						if (!pawn.training.HasLearned(td.prerequisites[i]))
						{
							text += "\n" + "TrainingNeedsPrerequisite".Translate(td.prerequisites[i].LabelCap);
						}
					}
				}
				return text;
			}, (int)(rect.y * 612f + rect.x));
		}
	}
}
