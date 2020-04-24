using Steamworks;
using UnityEngine;

namespace Verse.Steam
{
	public class Dialog_WorkshopOperationInProgress : Window
	{
		public override Vector2 InitialSize => new Vector2(600f, 400f);

		public Dialog_WorkshopOperationInProgress()
		{
			forcePause = true;
			closeOnAccept = false;
			closeOnCancel = false;
			absorbInputAroundWindow = true;
			preventDrawTutor = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Workshop.GetUpdateStatus(out EItemUpdateStatus updateStatus, out float progPercent);
			WorkshopInteractStage curStage = Workshop.CurStage;
			if (curStage == WorkshopInteractStage.None && updateStatus == EItemUpdateStatus.k_EItemUpdateStatusInvalid)
			{
				Close();
				return;
			}
			string text = "";
			if (curStage != 0)
			{
				text += curStage.GetLabel();
				text += "\n\n";
			}
			if (updateStatus != 0)
			{
				text += updateStatus.GetLabel();
				if (progPercent > 0f)
				{
					text = text + " (" + progPercent.ToStringPercent() + ")";
				}
				text += GenText.MarchingEllipsis();
			}
			Widgets.Label(inRect, text);
		}

		public static void CloseAll()
		{
			Find.WindowStack.WindowOfType<Dialog_WorkshopOperationInProgress>()?.Close();
		}
	}
}
