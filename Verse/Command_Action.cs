using System;
using UnityEngine;

namespace Verse
{
	public class Command_Action : Command
	{
		public Action action;

		public Action onHover;

		private Color? iconDrawColorOverride;

		public override Color IconDrawColor => iconDrawColorOverride ?? base.IconDrawColor;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			action();
		}

		public override void GizmoUpdateOnMouseover()
		{
			if (onHover != null)
			{
				onHover();
			}
		}

		public void SetColorOverride(Color color)
		{
			iconDrawColorOverride = color;
		}
	}
}
