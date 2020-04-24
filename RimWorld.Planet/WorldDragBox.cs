using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDragBox
	{
		public bool active;

		public Vector2 start;

		private const float DragBoxMinDiagonal = 7f;

		public float LeftX => Math.Min(start.x, UI.MousePositionOnUIInverted.x);

		public float RightX => Math.Max(start.x, UI.MousePositionOnUIInverted.x);

		public float BotZ => Math.Min(start.y, UI.MousePositionOnUIInverted.y);

		public float TopZ => Math.Max(start.y, UI.MousePositionOnUIInverted.y);

		public Rect ScreenRect => new Rect(LeftX, BotZ, RightX - LeftX, TopZ - BotZ);

		public float Diagonal => (start - new Vector2(UI.MousePositionOnUIInverted.x, UI.MousePositionOnUIInverted.y)).magnitude;

		public bool IsValid => Diagonal > 7f;

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

		public bool Contains(WorldObject o)
		{
			return Contains(o.ScreenPos());
		}

		public bool Contains(Vector2 screenPoint)
		{
			if (screenPoint.x + 0.5f > LeftX && screenPoint.x - 0.5f < RightX && screenPoint.y + 0.5f > BotZ && screenPoint.y - 0.5f < TopZ)
			{
				return true;
			}
			return false;
		}
	}
}
