using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public abstract class Gizmo
{
	protected bool disabled;

	public string disabledReason;

	public bool alsoClickIfOtherInGroupClicked = true;

	private float order;

	public const float Height = 75f;

	public virtual bool Disabled
	{
		get
		{
			return disabled;
		}
		set
		{
			disabled = value;
		}
	}

	public virtual bool Visible => true;

	public virtual IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => Enumerable.Empty<FloatMenuOption>();

	public virtual float Order
	{
		get
		{
			return order;
		}
		set
		{
			order = value;
		}
	}

	public abstract GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms);

	public virtual void GizmoUpdateOnMouseover()
	{
	}

	public abstract float GetWidth(float maxWidth);

	public virtual void ProcessInput(Event ev)
	{
	}

	public virtual void ProcessGroupInput(Event ev, List<Gizmo> group)
	{
	}

	public virtual bool GroupsWith(Gizmo other)
	{
		return false;
	}

	public virtual void MergeWith(Gizmo other)
	{
	}

	public virtual bool ShowPawnDetailsWith(Gizmo gizmo)
	{
		return false;
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
