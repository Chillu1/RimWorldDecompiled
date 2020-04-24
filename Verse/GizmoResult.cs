using UnityEngine;

namespace Verse
{
	public struct GizmoResult
	{
		private GizmoState stateInt;

		private Event interactEventInt;

		public GizmoState State => stateInt;

		public Event InteractEvent => interactEventInt;

		public GizmoResult(GizmoState state)
		{
			stateInt = state;
			interactEventInt = null;
		}

		public GizmoResult(GizmoState state, Event interactEvent)
		{
			stateInt = state;
			interactEventInt = interactEvent;
		}
	}
}
