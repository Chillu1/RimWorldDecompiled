using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_GameStartDialog : ScenPart
	{
		private string text;

		private string textKey;

		private SoundDef closeSound;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 5f);
			text = Widgets.TextArea(scenPartRect, text);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref textKey, "textKey");
			Scribe_Defs.Look(ref closeSound, "closeSound");
		}

		public override void PostGameStart()
		{
			if (Find.GameInitData.startedFromEntry)
			{
				Find.MusicManagerPlay.disabled = true;
				Find.WindowStack.Notify_GameStartDialogOpened();
				DiaNode diaNode = new DiaNode(text.NullOrEmpty() ? textKey.TranslateSimple() : text);
				DiaOption diaOption = new DiaOption();
				diaOption.resolveTree = true;
				diaOption.clickSound = null;
				diaNode.options.Add(diaOption);
				Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode);
				dialog_NodeTree.soundClose = ((closeSound != null) ? closeSound : SoundDefOf.GameStartSting);
				dialog_NodeTree.closeAction = delegate
				{
					Find.MusicManagerPlay.ForceSilenceFor(7f);
					Find.MusicManagerPlay.disabled = false;
					Find.WindowStack.Notify_GameStartDialogClosed();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
					TutorSystem.Notify_Event("GameStartDialogClosed");
				};
				Find.WindowStack.Add(dialog_NodeTree);
				Find.Archive.Add(new ArchivedDialog(diaNode.text));
			}
		}
	}
}
