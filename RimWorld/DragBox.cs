using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class DragBox
	{
		public bool active;

		public Vector3 start;

		private const float DragBoxMinDiagonal = 0.5f;

		public float LeftX => Math.Min(start.x, UI.MouseMapPosition().x);

		public float RightX => Math.Max(start.x, UI.MouseMapPosition().x);

		public float BotZ => Math.Min(start.z, UI.MouseMapPosition().z);

		public float TopZ => Math.Max(start.z, UI.MouseMapPosition().z);

		public Rect ScreenRect
		{
			get
			{
				Vector2 vector = start.MapToUIPosition();
				Vector2 mousePosition = Event.current.mousePosition;
				if (mousePosition.x < vector.x)
				{
					float x = mousePosition.x;
					mousePosition.x = vector.x;
					vector.x = x;
				}
				if (mousePosition.y < vector.y)
				{
					float y = mousePosition.y;
					mousePosition.y = vector.y;
					vector.y = y;
				}
				Rect result = default(Rect);
				result.xMin = vector.x;
				result.xMax = mousePosition.x;
				result.yMin = vector.y;
				result.yMax = mousePosition.y;
				return result;
			}
		}

		public bool IsValid => (start - UI.MouseMapPosition()).magnitude > 0.5f;

		public bool IsValidAndActive
		{
			get
			{
				if (active)
				{
					return IsValid;
				}
				return false;
			}
		}

		public void DragBoxOnGUI()
		{
			if (IsValidAndActive)
			{
				Widgets.DrawBox(ScreenRect, 2);
			}
		}

		public bool Contains(Thing t)
		{
			if (t is Pawn)
			{
				return Contains((t as Pawn).Drawer.DrawPos);
			}
			foreach (IntVec3 item in t.OccupiedRect())
			{
				if (Contains(item.ToVector3Shifted()))
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(Vector3 v)
		{
			if (v.x + 0.5f > LeftX && v.x - 0.5f < RightX && v.z + 0.5f > BotZ && v.z - 0.5f < TopZ)
			{
				return true;
			}
			return false;
		}
	}
}
