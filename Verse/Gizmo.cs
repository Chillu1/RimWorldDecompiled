using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class Gizmo
	{
		public bool disabled;

		public string disabledReason;

		public bool alsoClickIfOtherInGroupClicked = true;

		public float order;

		public const float Height = 75f;

		public virtual bool Visible => true;

		public virtual IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				yield break;
			}
		}

		public abstract GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth);

		public virtual void GizmoUpdateOnMouseover()
		{
		}

		public abstract float GetWidth(float maxWidth);

		public virtual void ProcessInput(Event ev)
		{
		}

		public virtual bool GroupsWith(Gizmo other)
		{
			return false;
		}

		public virtual void MergeWith(Gizmo other)
		{
		}

		public virtual bool InheritInteractionsFrom(Gizmo other)
		{
			return alsoClickIfOtherInGroupClicked;
		}

		public virtual bool InheritFloatMenuInteractionsFrom(Gizmo other)
		{
			return InheritInteractionsFrom(other);
		}

		public void Disable(string reason = null)
		{
			disabled = true;
			disabledReason = reason;
		}
	}
}
