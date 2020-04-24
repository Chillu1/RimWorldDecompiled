using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class DesignatorManager
	{
		private Designator selectedDesignator;

		private DesignationDragger dragger = new DesignationDragger();

		public Designator SelectedDesignator => selectedDesignator;

		public DesignationDragger Dragger => dragger;

		public void Select(Designator des)
		{
			Deselect();
			selectedDesignator = des;
			selectedDesignator.Selected();
		}

		public void Deselect()
		{
			if (selectedDesignator != null)
			{
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
				if (selectedDesignator.DraggableDimensions == 0)
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
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && dragger.Dragging)
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
	}
}
