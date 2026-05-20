using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_IdeosDuringLanding : Window
	{
		private Vector2 scrollPosition_ideoList;

		private float scrollViewHeight_ideoList;

		private Vector2 scrollPosition_ideoDetails;

		private float scrollViewHeight_ideoDetails;

		public override Vector2 InitialSize => new Vector2(1010f, Mathf.Min(1000f, UI.screenHeight));

		public Dialog_IdeosDuringLanding()
		{
			doCloseButton = true;
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			IdeoUIUtility.DoIdeoListAndDetails(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - Window.CloseButSize.y), ref scrollPosition_ideoList, ref scrollViewHeight_ideoList, ref scrollPosition_ideoDetails, ref scrollViewHeight_ideoDetails, editMode: false, showCreateIdeoButton: false, null, null, null, forArchonexusRestart: false, null, null, showLoadExistingIdeoBtn: false, allowLoad: false);
		}
	}
}
