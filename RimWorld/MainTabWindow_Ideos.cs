using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Ideos : MainTabWindow
	{
		private Vector2 scrollPosition_ideoList;

		private float scrollViewHeight_ideoList;

		private Vector2 scrollPosition_ideoDetails;

		private float scrollViewHeight_ideoDetails;

		public override Vector2 InitialSize => new Vector2(base.InitialSize.x, UI.screenHeight - 35);

		public override void PreOpen()
		{
			base.PreOpen();
			scrollPosition_ideoDetails = Vector2.zero;
		}

		public override void PostClose()
		{
			base.PostClose();
			IdeoUIUtility.UnselectCurrent();
		}

		public override void DoWindowContents(Rect rect)
		{
			IdeoUIUtility.DoIdeoListAndDetails(rect, ref scrollPosition_ideoList, ref scrollViewHeight_ideoList, ref scrollPosition_ideoDetails, ref scrollViewHeight_ideoDetails, editMode: false, showCreateIdeoButton: false, null, null, null, forArchonexusRestart: false, null, null, showLoadExistingIdeoBtn: false, allowLoad: false);
		}
	}
}
