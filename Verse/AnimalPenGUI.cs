using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class AnimalPenGUI
	{
		public class PenPainter : AnimalPenEnclosureCalculator
		{
			private readonly List<IntVec3> directEdgeCells = new List<IntVec3>();

			private readonly List<IntVec3> indirectEdgeCells = new List<IntVec3>();

			private readonly List<IntVec3> openDoorEdgeCells = new List<IntVec3>();

			protected override void VisitDirectlyConnectedRegion(Region r)
			{
				directEdgeCells.AddRange(r.Cells);
			}

			protected override void VisitIndirectlyDirectlyConnectedRegion(Region r)
			{
				indirectEdgeCells.AddRange(r.Cells);
			}

			protected override void VisitPassableDoorway(Region r)
			{
				openDoorEdgeCells.AddRange(r.Cells);
			}

			public void Paint(IntVec3 position, Map map)
			{
				directEdgeCells.Clear();
				indirectEdgeCells.Clear();
				openDoorEdgeCells.Clear();
				if (VisitPen(position, map))
				{
					GenDraw.DrawFieldEdges(directEdgeCells, Color.green);
					GenDraw.DrawFieldEdges(indirectEdgeCells, Color.white);
					GenDraw.DrawFieldEdges(openDoorEdgeCells, Color.white);
				}
				else
				{
					GenDraw.DrawFieldEdges(openDoorEdgeCells, Color.red);
				}
			}
		}

		public class PenBlueprintPainter
		{
			private AnimalPenBlueprintEnclosureCalculator filler = new AnimalPenBlueprintEnclosureCalculator();

			public void Paint(IntVec3 position, Map map)
			{
				filler.VisitPen(position, map);
				if (filler.isEnclosed)
				{
					GenDraw.DrawFieldEdges(filler.cellsFound, Color.white);
				}
			}
		}

		public static void DoAllowedAreaMessage(Rect rect, Pawn pawn)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			CompAnimalPenMarker currentPenOf = AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false);
			TaggedString taggedString;
			TaggedString taggedString2;
			if (currentPenOf != null)
			{
				taggedString = "InPen".Translate() + ": " + currentPenOf.label;
				taggedString2 = taggedString;
			}
			else
			{
				GUI.color = Color.gray;
				taggedString = "(" + "Unpenned".Translate() + ")";
				taggedString2 = "UnpennedTooltip".Translate();
			}
			Widgets.Label(rect, taggedString);
			TooltipHandler.TipRegion(rect, taggedString2);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
		}

		public static void DrawPlacingMouseAttachments(IntVec3 mouseCell, Map map, PenFoodCalculator calc)
		{
			StringBuilder sb = new StringBuilder();
			Vector2 location = Find.WorldGrid.LongLatOf(map.Tile);
			Quadrum summerQuadrum = calc.GetSummerOrBestQuadrum();
			Season summerLabel = summerQuadrum.GetSeason(location);
			sb.AppendLine(calc.PenSizeDescription());
			sb.AppendLine("PenExampleAnimals".Translate() + ":");
			AppendCapacityOf(ThingDefOf.Cow);
			AppendCapacityOf(ThingDefOf.Goat);
			AppendCapacityOf(ThingDefOf.Chicken);
			Widgets.MouseAttachedLabel(sb.ToString().TrimEnd(), 8f, 35f);
			void AppendCapacityOf(ThingDef animalDef)
			{
				sb.Append("PenCapacityDesc".Translate(NamedArgumentUtility.Named(animalDef, "ANIMALDEF")).CapitalizeFirst());
				sb.Append(" (").Append(summerLabel.Label()).Append("): ");
				if (calc.Unenclosed)
				{
					sb.Append("?");
				}
				else
				{
					sb.Append(calc.CapacityOf(summerQuadrum, animalDef).ToString("F1"));
				}
				sb.AppendLine();
			}
		}
	}
}
