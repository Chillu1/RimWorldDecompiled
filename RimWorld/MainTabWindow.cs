using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainTabWindow : Window
	{
		public MainButtonDef def;

		public virtual Vector2 RequestedTabSize => new Vector2(1010f, 684f);

		public virtual MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 requestedTabSize = RequestedTabSize;
				if (requestedTabSize.y > (float)(UI.screenHeight - 35))
				{
					requestedTabSize.y = UI.screenHeight - 35;
				}
				if (requestedTabSize.x > (float)UI.screenWidth)
				{
					requestedTabSize.x = UI.screenWidth;
				}
				return requestedTabSize;
			}
		}

		public MainTabWindow()
		{
			layer = WindowLayer.GameUI;
			soundAppear = null;
			soundClose = SoundDefOf.TabClose;
			doCloseButton = false;
			doCloseX = false;
			preventCameraMotion = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			SetInitialSizeAndPosition();
		}

		protected override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();
			if (Anchor == MainTabWindowAnchor.Left)
			{
				windowRect.x = 0f;
			}
			else
			{
				windowRect.x = (float)UI.screenWidth - windowRect.width;
			}
			windowRect.y = (float)(UI.screenHeight - 35) - windowRect.height;
		}
	}
}
