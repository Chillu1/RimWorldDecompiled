using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class EditWindow : Window
	{
		private const float SuperimposeAvoidThreshold = 8f;

		private const float SuperimposeAvoidOffset = 16f;

		private const float SuperimposeAvoidOffsetMinEdge = 200f;

		public override Vector2 InitialSize => new Vector2(500f, 500f);

		protected override float Margin => 8f;

		public EditWindow()
		{
			resizeable = true;
			draggable = true;
			preventCameraMotion = false;
			doCloseX = true;
			windowRect.x = 5f;
			windowRect.y = 5f;
		}

		public override void PostOpen()
		{
			while (!(windowRect.x > (float)UI.screenWidth - 200f) && !(windowRect.y > (float)UI.screenHeight - 200f))
			{
				bool flag = false;
				foreach (EditWindow item in Find.WindowStack.Windows.Where((Window di) => di is EditWindow).Cast<EditWindow>())
				{
					if (item != this && Mathf.Abs(item.windowRect.x - windowRect.x) < 8f && Mathf.Abs(item.windowRect.y - windowRect.y) < 8f)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					windowRect.x += 16f;
					windowRect.y += 16f;
					continue;
				}
				break;
			}
		}
	}
}
