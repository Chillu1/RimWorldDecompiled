using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InspectGizmoGrid
	{
		private static List<object> objList = new List<object>();

		private static List<Gizmo> gizmoList = new List<Gizmo>();

		public static void DrawInspectGizmoGridFor(IEnumerable<object> selectedObjects, out Gizmo mouseoverGizmo)
		{
			mouseoverGizmo = null;
			try
			{
				objList.Clear();
				objList.AddRange(selectedObjects);
				gizmoList.Clear();
				for (int i = 0; i < objList.Count; i++)
				{
					ISelectable selectable = objList[i] as ISelectable;
					if (selectable != null)
					{
						gizmoList.AddRange(selectable.GetGizmos());
					}
				}
				for (int j = 0; j < objList.Count; j++)
				{
					Thing t = objList[j] as Thing;
					if (t == null)
					{
						continue;
					}
					List<Designator> allDesignators = Find.ReverseDesignatorDatabase.AllDesignators;
					for (int k = 0; k < allDesignators.Count; k++)
					{
						Designator des = allDesignators[k];
						if (!des.CanDesignateThing(t).Accepted)
						{
							continue;
						}
						Command_Action command_Action = new Command_Action();
						command_Action.defaultLabel = des.LabelCapReverseDesignating(t);
						command_Action.icon = des.IconReverseDesignating(t, out float angle, out Vector2 offset);
						command_Action.iconAngle = angle;
						command_Action.iconOffset = offset;
						command_Action.defaultDesc = des.DescReverseDesignating(t);
						command_Action.order = ((des is Designator_Uninstall) ? (-11f) : (-20f));
						command_Action.action = delegate
						{
							if (TutorSystem.AllowAction(des.TutorTagDesignate))
							{
								des.DesignateThing(t);
								des.Finalize(somethingSucceeded: true);
							}
						};
						command_Action.hotKey = des.hotKey;
						command_Action.groupKey = des.groupKey;
						gizmoList.Add(command_Action);
					}
				}
				objList.Clear();
				GizmoGridDrawer.DrawGizmoGrid(gizmoList, InspectPaneUtility.PaneWidthFor(Find.WindowStack.WindowOfType<IInspectPane>()) + GizmoGridDrawer.GizmoSpacing.y, out mouseoverGizmo);
				gizmoList.Clear();
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(ex.ToString(), 3427734);
			}
		}
	}
}
