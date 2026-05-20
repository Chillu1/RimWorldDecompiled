using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class DesignatorManager
{
	private Designator selectedDesignator;

	private DrawStyleDef selectedStyle;

	private Dictionary<DrawStyleCategoryDef, DrawStyleDef> previouslySelected = new Dictionary<DrawStyleCategoryDef, DrawStyleDef>();

	private readonly DesignationDragger dragger = new DesignationDragger();

	public Designator SelectedDesignator => selectedDesignator;

	public DrawStyleDef SelectedStyle
	{
		get
		{
			return selectedStyle;
		}
		set
		{
			selectedStyle = value;
			dragger.UpdateDragCellsIfNeeded();
			DrawStyleCategoryDef drawStyleCategory = selectedDesignator.DrawStyleCategory;
			if (drawStyleCategory != null && drawStyleCategory.styles.Contains(value))
			{
				previouslySelected[drawStyleCategory] = value;
			}
		}
	}

	public DesignationDragger Dragger => dragger;

	public void Select(Designator des)
	{
		Deselect();
		selectedDesignator = des;
		DrawStyleCategoryDef drawStyleCategory = des.DrawStyleCategory;
		if (drawStyleCategory != null && !drawStyleCategory.styles.NullOrEmpty())
		{
			if (Prefs.RememberDrawStlyes && previouslySelected.TryGetValue(drawStyleCategory, out var value))
			{
				selectedStyle = value;
			}
			else
			{
				selectedStyle = drawStyleCategory.styles[0];
			}
		}
		else
		{
			selectedStyle = null;
		}
		selectedDesignator.Selected();
	}

	public void Deselect()
	{
		if (selectedDesignator != null)
		{
			selectedDesignator.Deselected();
			selectedDesignator = null;
			dragger.EndDrag();
		}
	}

	private bool CheckSelectedDesignatorValid()
	{
		if (selectedDesignator == null)
		{
			return false;
		}
		if (!selectedDesignator.CanRemainSelected())
		{
			Deselect();
			return false;
		}
		return true;
	}

	public void ProcessInputEvents()
	{
		if (!CheckSelectedDesignatorValid())
		{
			return;
		}
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			if (SelectedStyle == null)
			{
				Designator designator = selectedDesignator;
				AcceptanceReport acceptanceReport = selectedDesignator.CanDesignateCell(UI.MouseCell());
				if (acceptanceReport.Accepted)
				{
					designator.DesignateSingleCell(UI.MouseCell());
					designator.Finalize(somethingSucceeded: true);
				}
				else
				{
					Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.SilentInput, historical: false);
					selectedDesignator.Finalize(somethingSucceeded: false);
				}
			}
			else
			{
				dragger.StartDrag();
			}
			Event.current.Use();
		}
		if ((Event.current.type == EventType.MouseDown && Event.current.button == 1) || KeyBindingDefOf.Cancel.KeyDownEvent)
		{
			SoundDefOf.CancelMode.PlayOneShotOnCamera();
			Deselect();
			dragger.EndDrag();
			Event.current.Use();
			TutorSystem.Notify_Event("ClearDesignatorSelection");
		}
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && (dragger.Dragging || (selectedStyle != null && selectedStyle.DrawStyleWorker.SingleCell)))
		{
			selectedDesignator.DesignateMultiCell(dragger.DragCells);
			dragger.EndDrag();
			Event.current.Use();
		}
	}

	public void DesignationManagerOnGUI()
	{
		dragger.DraggerOnGUI();
		if (CheckSelectedDesignatorValid())
		{
			selectedDesignator.DrawMouseAttachments();
			selectedStyle?.DrawStyleWorker.Draw();
		}
	}

	public void DesignatorManagerUpdate()
	{
		dragger.DraggerUpdate();
		if (CheckSelectedDesignatorValid())
		{
			selectedDesignator.SelectedUpdate();
		}
	}

	public void AdvanceDrawStyle()
	{
		ChangeDrawStyle(1);
	}

	public void PreviousDrawStyle()
	{
		ChangeDrawStyle(-1);
	}

	private void ChangeDrawStyle(int delta)
	{
		if (SelectedStyle != null && selectedDesignator.DrawStyleCategory != null && !selectedDesignator.DrawStyleCategory.styles.NullOrEmpty())
		{
			List<DrawStyleDef> styles = selectedDesignator.DrawStyleCategory.styles;
			int index = GenMath.PositiveMod(styles.IndexOf(SelectedStyle) + delta, styles.Count);
			SelectedStyle = styles[index];
		}
	}
}
