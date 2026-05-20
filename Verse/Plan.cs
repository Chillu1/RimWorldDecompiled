using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class Plan : IExposable, ISelectable, ILoadReferenceable, IRenameable, IHideable
{
	public PlanManager planManager;

	private ColorDef color;

	private int ID = -1;

	public string label;

	private string baseLabel;

	private List<IntVec3> cells = new List<IntVec3>();

	private bool hidden;

	private Material materialInt;

	private bool cellsShuffled;

	private static BoolGrid extantGrid;

	private static BoolGrid foundGrid;

	private static readonly HashSet<IntVec3> toSplit = new HashSet<IntVec3>();

	public string RenamableLabel
	{
		get
		{
			return label ?? baseLabel;
		}
		set
		{
			label = value;
		}
	}

	public string BaseLabel => baseLabel;

	public string InspectLabel => RenamableLabel;

	public Map Map => planManager.map;

	public bool Hidden
	{
		get
		{
			return hidden;
		}
		set
		{
			hidden = value;
			foreach (IntVec3 cell in Cells)
			{
				Map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Plan);
			}
		}
	}

	public ColorDef Color
	{
		get
		{
			return color;
		}
		private set
		{
			if (color != value)
			{
				color = value;
				materialInt = CreateMaterial();
				Dirty();
			}
		}
	}

	public Material Material
	{
		get
		{
			if ((object)materialInt == null)
			{
				materialInt = CreateMaterial();
			}
			return materialInt;
		}
	}

	public List<IntVec3> Cells
	{
		get
		{
			if (!cellsShuffled)
			{
				cells.Shuffle();
				cellsShuffled = true;
			}
			return cells;
		}
	}

	public int CellCount => cells.Count;

	public void MergeIn(Plan plan)
	{
		for (int num = plan.Cells.Count - 1; num >= 0; num--)
		{
			IntVec3 c = plan.Cells[num];
			plan.RemoveCell(c);
			AddCell(c);
		}
	}

	protected virtual Material CreateMaterial()
	{
		return MaterialPool.MatFrom(new MaterialRequest
		{
			BaseTexPath = "Designations/Plan",
			shader = ShaderDatabase.MetaOverlay,
			color = color.color
		});
	}

	public IEnumerator<IntVec3> GetEnumerator()
	{
		return cells.GetEnumerator();
	}

	public Plan()
	{
	}

	public Plan(ColorDef color, PlanManager planManager)
	{
		baseLabel = "NewPlan".Translate();
		label = planManager.NewPlanName("NewPlan".Translate());
		this.planManager = planManager;
		ID = Find.UniqueIDsManager.GetNextPlanID();
		Map.planManager.RegisterPlan(this);
		this.color = color;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref ID, "ID", -1);
		Scribe_Values.Look(ref label, "label");
		Scribe_Values.Look(ref baseLabel, "baseLabel");
		Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
		Scribe_Defs.Look(ref color, "color");
		Scribe_Collections.Look(ref cells, "cells", LookMode.Undefined);
		BackCompatibility.PostExposeData(this);
	}

	public virtual void AddCell(IntVec3 c)
	{
		if (cells.Contains(c))
		{
			Log.Error($"Adding cell to zone which already has it. c={c}, zone={this}");
			return;
		}
		cells.Add(c);
		planManager.AddPlanGridCell(this, c);
		Map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Plan);
		cellsShuffled = false;
	}

	public virtual void RemoveCell(IntVec3 c)
	{
		if (!cells.Contains(c))
		{
			Log.Error($"Cannot remove cell from zone which doesn't have it. c={c}, zone={this}");
			return;
		}
		cells.Remove(c);
		planManager.ClearPlanGridCell(c);
		Map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Plan);
		cellsShuffled = false;
		if (cells.Count == 0)
		{
			Deregister();
		}
	}

	public void Dirty()
	{
		foreach (IntVec3 cell in cells)
		{
			Map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Plan);
		}
	}

	public void Delete()
	{
		Delete(playSound: true);
	}

	public virtual void Delete(bool playSound)
	{
		if (playSound)
		{
			SoundDefOf.Designate_ZoneDelete.PlayOneShotOnCamera(Map);
		}
		if (cells.Count == 0)
		{
			Deregister();
		}
		else
		{
			while (cells.Count > 0)
			{
				RemoveCell(cells.First());
			}
		}
		Find.Selector.Deselect(this);
	}

	public void Deregister()
	{
		planManager.DeregisterPlan(this);
	}

	public bool ContainsCell(IntVec3 c)
	{
		return cells.Contains(c);
	}

	public virtual string GetInspectString()
	{
		int num = 0;
		for (int i = 0; i < planManager.AllPlans.Count; i++)
		{
			Plan plan = planManager.AllPlans[i];
			num += plan.CellCount;
		}
		string text = string.Format("{0}: {1}", "Size".Translate().CapitalizeFirst(), CellCount);
		if (planManager.AllPlans.Count > 1)
		{
			text += string.Format("\n{0}: {1}", "TotalPlanningArea".Translate().CapitalizeFirst(), num);
		}
		return text;
	}

	public virtual IEnumerable<InspectTabBase> GetInspectTabs()
	{
		return null;
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		Designator_Plan_Expand expand = DesignatorUtility.FindAllowedDesignator<Designator_Plan_Expand>();
		expand.Initialize(color);
		yield return new Command_Hide_Plans(this)
		{
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanHide"),
			defaultDesc = "CommandHidePlanDesc".Translate(),
			hotKey = KeyBindingDefOf.Misc2
		};
		yield return expand;
		yield return DesignatorUtility.FindAllowedDesignator<Designator_Plan_Shrink>();
		yield return new Command_Action
		{
			icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanChangeColor"),
			defaultLabel = "CommandChangeColorPlanLabel".Translate(),
			defaultDesc = "CommandChangeColorPlanDesc".Translate(),
			action = delegate
			{
				List<FloatMenuGridOption> list = new List<FloatMenuGridOption>();
				foreach (ColorDef color in Designator_Plan_Add.Colors)
				{
					ColorDef newCol = color;
					Color value = newCol.color;
					value.a = 0.8f;
					list.Add(new FloatMenuGridOption(BaseContent.WhiteTex, delegate
					{
						Color = newCol;
						CheckContiguous();
					}, value, newCol.LabelCap));
				}
				Find.WindowStack.Add(new FloatMenuGrid(list));
			}
		};
		yield return new Command_Action
		{
			icon = TexButton.Delete,
			defaultLabel = "CommandDeletePlanLabel".Translate(),
			defaultDesc = "CommandDeletePlanDesc".Translate(),
			action = Delete,
			hotKey = KeyBindingDefOf.Designator_Deconstruct
		};
		yield return DesignatorUtility.FindAllowedDesignator<Designator_Plan_Copy>();
		yield return DesignatorUtility.FindAllowedDesignator<Designator_Plan_CopySelection>();
	}

	public void CheckContiguous()
	{
		if (cells.Count == 0)
		{
			return;
		}
		if (extantGrid == null)
		{
			extantGrid = new BoolGrid(Map);
		}
		else
		{
			extantGrid.ClearAndResizeTo(Map);
		}
		if (foundGrid == null)
		{
			foundGrid = new BoolGrid(Map);
		}
		else
		{
			foundGrid.ClearAndResizeTo(Map);
		}
		foreach (IntVec3 allCell in Map.AllCells)
		{
			Plan plan = allCell.GetPlan(Map);
			if (plan != null && plan == this)
			{
				extantGrid.Set(allCell, value: true);
			}
		}
		int numFound = 0;
		Map.floodFiller.FloodFill(cells.First(), (Predicate<IntVec3>)PassCheck, (Action<IntVec3>)Processor, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
		if (numFound >= cells.Count)
		{
			return;
		}
		foreach (IntVec3 allCell2 in Map.AllCells)
		{
			if (!extantGrid[allCell2] || foundGrid[allCell2])
			{
				continue;
			}
			Map.floodFiller.FloodFill(allCell2, (IntVec3 cell) => extantGrid[cell] && !foundGrid[cell], delegate(IntVec3 cell)
			{
				if (Map.planManager.PlanAt(cell) == this)
				{
					toSplit.Add(cell);
				}
				extantGrid[cell] = false;
			});
			Plan plan2 = null;
			foreach (IntVec3 item in toSplit)
			{
				if (plan2 == null)
				{
					plan2 = new Plan(color, planManager);
				}
				plan2.RenamableLabel = RenamableLabel;
				RemoveCell(item);
				plan2.AddCell(item);
			}
			toSplit.Clear();
		}
		static bool PassCheck(IntVec3 c)
		{
			if (!extantGrid[c])
			{
				return false;
			}
			if (foundGrid[c])
			{
				return false;
			}
			return true;
		}
		void Processor(IntVec3 c)
		{
			foundGrid.Set(c, value: true);
			numFound++;
		}
	}

	public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		yield break;
	}

	public override string ToString()
	{
		return label;
	}

	public string GetUniqueLoadID()
	{
		return $"Plan_{ID}";
	}
}
