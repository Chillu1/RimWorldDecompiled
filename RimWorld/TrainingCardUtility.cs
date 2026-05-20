using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class TrainingCardUtility
{
	public const float RowHeight = 28f;

	public const float RenameButtonSize = 30f;

	private const float InfoHeaderHeight = 50f;

	[TweakValue("Interface", -100f, 300f)]
	private static float TrainabilityLeft = 220f;

	[TweakValue("Interface", -100f, 300f)]
	private static float TrainabilityTop = 0f;

	public static readonly Texture2D LearnedTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheck");

	public static readonly Texture2D LearnedNotTrainingTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheckOff");

	public static void DrawTrainingCard(Rect rect, Pawn pawn)
	{
		Text.Font = GameFont.Small;
		RenameUIUtility.DrawRenameButton(new Rect(TrainabilityLeft, TrainabilityTop, 30f, 30f), pawn);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(rect);
		TrainabilityDef trainability = TrainableUtility.GetTrainability(pawn);
		if (trainability != null)
		{
			listing_Standard.Label("CreatureTrainability".Translate(pawn.def.label).CapitalizeFirst() + ": " + trainability.LabelCap, 22f);
		}
		float statValue = pawn.GetStatValue(StatDefOf.Wildness);
		listing_Standard.Label("CreatureWildness".Translate(pawn.def.label).CapitalizeFirst() + ": " + statValue.ToStringPercent(), 22f, StatDefOf.Wildness.Worker.GetExplanationFull(StatRequest.For(pawn), StatDefOf.Wildness.toStringNumberSense, statValue));
		if (pawn.training.HasLearned(TrainableDefOf.Obedience))
		{
			Rect rect2 = listing_Standard.GetRect(25f);
			Widgets.Label(rect2, "Master".Translate() + ": ");
			rect2.xMin = rect2.center.x;
			if (pawn.RaceProps.playerCanChangeMaster || !ModsConfig.IdeologyActive)
			{
				TrainableUtility.MasterSelectButton(rect2, pawn, paintable: false);
			}
			else if (pawn.playerSettings?.Master != null)
			{
				Widgets.Label(rect2, TrainableUtility.MasterString(pawn).Truncate(rect2.width));
				TooltipHandler.TipRegion(rect2, "DryadCannotChangeMaster".Translate(pawn.Named("ANIMAL"), pawn.playerSettings.Master.Named("MASTER")).CapitalizeFirst());
			}
			listing_Standard.Gap();
			Rect rect3 = listing_Standard.GetRect(25f);
			bool checkOn = pawn.playerSettings.followDrafted;
			Widgets.CheckboxLabeled(rect3, "CreatureFollowDrafted".Translate(), ref checkOn, disabled: false, null, null, placeCheckboxNearText: false, paintable: true);
			pawn.playerSettings.followDrafted = checkOn;
			Rect rect4 = listing_Standard.GetRect(25f);
			bool checkOn2 = pawn.playerSettings.followFieldwork;
			Widgets.CheckboxLabeled(rect4, "CreatureFollowFieldwork".Translate(), ref checkOn2, disabled: false, null, null, placeCheckboxNearText: false, paintable: true);
			pawn.playerSettings.followFieldwork = checkOn2;
		}
		if (ModsConfig.OdysseyActive)
		{
			if (pawn.training.HasLearned(TrainableDefOf.Forage))
			{
				Rect rect5 = listing_Standard.GetRect(25f);
				bool checkOn3 = pawn.playerSettings.animalForage;
				Widgets.CheckboxLabeled(rect5, "ForageEnabled".Translate(), ref checkOn3, disabled: false, null, null, placeCheckboxNearText: false, paintable: true);
				pawn.playerSettings.animalForage = checkOn3;
			}
			if (pawn.training.HasLearned(TrainableDefOf.Dig))
			{
				Rect rect6 = listing_Standard.GetRect(25f);
				bool checkOn4 = pawn.playerSettings.animalDig;
				Widgets.CheckboxLabeled(rect6, "DigEnabled".Translate(), ref checkOn4, disabled: false, null, null, placeCheckboxNearText: false, paintable: true);
				pawn.playerSettings.animalDig = checkOn4;
			}
		}
		if (pawn.RaceProps.showTrainables)
		{
			listing_Standard.Gap();
			foreach (TrainableDef item in TrainableUtility.TrainableDefsInListOrder)
			{
				TryDrawTrainableRow(listing_Standard, pawn, item);
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
		if (p.RaceProps.showTrainables)
		{
			for (int i = 0; i < DefDatabase<TrainableDef>.AllDefsListForReading.Count; i++)
			{
				p.training.CanAssignToTrain(DefDatabase<TrainableDef>.AllDefsListForReading[i], out var visible);
				if (visible)
				{
					num++;
				}
			}
		}
		float num2 = 112f + 28f * (float)num;
		if (p.training.HasLearned(TrainableDefOf.Obedience))
		{
			num2 += 75f;
			num2 += 12f;
		}
		if (ModsConfig.OdysseyActive)
		{
			if (p.training.HasLearned(TrainableDefOf.Forage))
			{
				num2 += 25f;
			}
			if (p.training.HasLearned(TrainableDefOf.Dig))
			{
				num2 += 25f;
			}
		}
		return num2;
	}

	private static void TryDrawTrainableRow(Listing listing, Pawn pawn, TrainableDef td)
	{
		bool flag = pawn.training.HasLearned(td);
		bool visible;
		AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out visible);
		if (!visible)
		{
			return;
		}
		Rect rect = listing.GetRect(28f);
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
	}

	public static void DoTrainableCheckbox(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain, bool drawLabel, bool doTooltip)
	{
		bool num = pawn.training.HasLearned(td);
		bool checkOn = pawn.training.GetWanted(td);
		bool flag = checkOn;
		Texture2D texChecked = (num ? LearnedTrainingTex : null);
		Texture2D texUnchecked = (num ? LearnedNotTrainingTex : null);
		if (drawLabel)
		{
			Widgets.CheckboxLabeled(rect, td.LabelCap, ref checkOn, !canTrain.Accepted, texChecked, texUnchecked, placeCheckboxNearText: false, paintable: true);
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
