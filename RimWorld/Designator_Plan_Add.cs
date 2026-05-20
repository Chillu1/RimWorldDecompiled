using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Plan_Add : Designator_Plan
	{
		protected ColorDef colorDef;

		public const float UIAlpha = 0.8f;

		private readonly List<IntVec3> unsetCells = new List<IntVec3>();

		private readonly HashSet<Plan> modified = new HashSet<Plan>();

		public override bool DragDrawMeasurements => true;

		public virtual bool CanRightClickToggleVisibility => true;

		public static IEnumerable<ColorDef> Colors => from x in DefDatabase<ColorDef>.AllDefs
			where x.colorType == ColorType.Planning
			orderby x.displayOrder
			select x;

		public override Color IconDrawColor
		{
			get
			{
				Color color = colorDef.color;
				color.a = 0.8f;
				return color;
			}
		}

		protected virtual bool CanSelectColor => true;

		protected Plan SelectedPlan
		{
			get
			{
				Plan selectedPlan = Find.Selector.SelectedPlan;
				if (selectedPlan != null)
				{
					colorDef = Find.Selector.SelectedPlan.Color;
				}
				return selectedPlan;
			}
			set
			{
				if (Find.Selector.SelectedPlan == value)
				{
					return;
				}
				Find.Selector.ClearSelection();
				if (value != null)
				{
					Find.Selector.Select(value, playSound: false, forceDesignatorDeselect: false);
					if (Find.Selector.SelectedPlan != null)
					{
						colorDef = Find.Selector.SelectedPlan.Color;
					}
				}
			}
		}

		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				if (!CanRightClickToggleVisibility)
				{
					yield break;
				}
				foreach (FloatMenuOption hideOption in Command_Hide_Plans.GetHideOptions())
				{
					yield return hideOption;
				}
			}
		}

		public Designator_Plan_Add()
		{
			colorDef = Colors.FirstOrDefault();
			defaultLabel = "DesignatorPlan".Translate();
			defaultDesc = "DesignatorPlanDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn");
			soundSucceeded = SoundDefOf.Designate_PlanAdd;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			hotKey = KeyBindingDefOf.Misc9;
			useMouseIcon = true;
		}

		protected virtual Plan CreateNewPlan()
		{
			return new Plan(colorDef, Find.CurrentMap.planManager);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.InNoBuildEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			return true;
		}

		public override void DrawMouseAttachments()
		{
			if (useMouseIcon)
			{
				string text = mouseText;
				if (!Input.GetKey(KeyCode.Mouse0) && text.NullOrEmpty())
				{
					Plan selectedPlan = Find.Selector.SelectedPlan;
					text = ((selectedPlan != null) ? "ExpandOrCreatePlan".Translate(selectedPlan.label) : "CreateNewPlan".Translate());
				}
				GenUI.DrawMouseAttachment(icon, text);
			}
		}

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			GenUI.RenderMouseoverBracket();
			if (Find.Selector.SelectedPlan != null && Find.Selector.SelectedPlan.Color != colorDef)
			{
				Find.Selector.Deselect(Find.Selector.SelectedPlan);
			}
			if (!base.Map.IsPocketMap)
			{
				if (!base.Map.Tile.LayerDef.ignoreNoBuildArea)
				{
					GenDraw.DrawNoBuildEdgeLines();
				}
				else
				{
					GenDraw.DrawMapBoundaryLines();
				}
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!CanSelectColor)
			{
				base.ProcessInput(ev);
			}
			else
			{
				if (!CheckCanInteract())
				{
					return;
				}
				if (ev.button == 0)
				{
					List<FloatMenuGridOption> list = new List<FloatMenuGridOption>();
					foreach (ColorDef color2 in Colors)
					{
						ColorDef newCol = color2;
						Color color = newCol.color;
						color.a = 0.8f;
						list.Add(new FloatMenuGridOption(BaseContent.WhiteTex, delegate
						{
							base.ProcessInput(ev);
							Find.DesignatorManager.Select(this);
							colorDef = newCol;
						}, color, newCol.LabelCap));
					}
					Find.WindowStack.Add(new FloatMenuGrid(list));
					Find.DesignatorManager.Select(this);
				}
				else if (ev.button == 1 && CanRightClickToggleVisibility)
				{
					List<FloatMenuOption> options = new List<FloatMenuOption>(Command_Hide_Plans.GetHideOptions());
					Find.WindowStack.Add(new FloatMenu(options));
				}
			}
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Plan plan = base.Map.planManager.PlanAt(c);
			if (plan != null && plan.Color == colorDef)
			{
				SelectedPlan = plan;
				Finalize(somethingSucceeded: false);
			}
			else
			{
				PlanCells(new List<IntVec3> { c });
			}
		}

		public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			if (cells.Count() == 1)
			{
				Plan plan = base.Map.planManager.PlanAt(cells.First());
				if (plan != null && plan.Color == colorDef)
				{
					SelectedPlan = plan;
					Finalize(somethingSucceeded: false);
					return;
				}
			}
			PlanCells(cells);
		}

		protected void PlanCells(IEnumerable<IntVec3> cells)
		{
			modified.Clear();
			unsetCells.Clear();
			unsetCells.AddRange(cells);
			if (SelectedPlan == null)
			{
				Plan plan = null;
				foreach (IntVec3 cell in cells)
				{
					Plan plan2 = base.Map.planManager.PlanAt(cell);
					if (plan2 != null && plan2.Color == colorDef)
					{
						if (plan == null)
						{
							plan = plan2;
						}
						else if (plan2 != plan)
						{
							plan = null;
							break;
						}
					}
				}
				SelectedPlan = plan;
			}
			if (unsetCells.Count == 0)
			{
				return;
			}
			if (SelectedPlan == null)
			{
				Plan plan3 = base.Map.planManager.PlanAt(unsetCells[0]);
				if (plan3 != null)
				{
					plan3.RemoveCell(unsetCells[0]);
					modified.Add(plan3);
				}
				SelectedPlan = CreateNewPlan();
				SelectedPlan.AddCell(unsetCells[0]);
				unsetCells.RemoveAt(0);
			}
			modified.Add(SelectedPlan);
			bool somethingSucceeded;
			while (true)
			{
				somethingSucceeded = true;
				int count = unsetCells.Count;
				for (int num = unsetCells.Count - 1; num >= 0; num--)
				{
					IntVec3 intVec = unsetCells[num];
					for (int i = 0; i < 4; i++)
					{
						IntVec3 c = intVec + GenAdj.CardinalDirections[i];
						if (c.InBounds(base.Map))
						{
							Plan plan4 = base.Map.planManager.PlanAt(c);
							if (plan4 != null && plan4.Color == colorDef && plan4 != SelectedPlan)
							{
								SelectedPlan.MergeIn(plan4);
							}
						}
					}
					Plan plan5 = base.Map.planManager.PlanAt(intVec);
					if (plan5 != null && plan5 != SelectedPlan)
					{
						plan5.RemoveCell(intVec);
						modified.Add(plan5);
					}
					if (!SelectedPlan.ContainsCell(intVec))
					{
						SelectedPlan.AddCell(intVec);
					}
					unsetCells.RemoveAt(num);
				}
				if (unsetCells.Count == 0)
				{
					break;
				}
				if (unsetCells.Count == count)
				{
					SelectedPlan = CreateNewPlan();
					SelectedPlan.AddCell(unsetCells[0]);
					unsetCells.RemoveAt(0);
				}
			}
			foreach (Plan item in modified)
			{
				item.CheckContiguous();
			}
			Finalize(somethingSucceeded);
			modified.Clear();
			unsetCells.Clear();
		}
	}
}
