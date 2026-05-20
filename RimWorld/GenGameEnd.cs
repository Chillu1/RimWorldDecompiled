using UnityEngine;
using Verse;

namespace RimWorld;

public static class GenGameEnd
{
	public static void EndGameDialogMessage(string msg, bool allowKeepPlaying = true)
	{
		EndGameDialogMessage(msg, allowKeepPlaying, Color.clear);
	}

	public static void EndGameDialogMessage(string msg, bool allowKeepPlaying, Color screenFillColor)
	{
		DiaNode diaNode = new DiaNode(msg);
		if (allowKeepPlaying)
		{
			DiaOption diaOption = new DiaOption("GameOverKeepWatching".Translate());
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			DiaOption diaOption2 = new DiaOption("GameOverCreateNewWanderers".Translate());
			diaOption2.resolveTree = true;
			diaOption2.action = delegate
			{
				Find.WindowStack.Add(new Dialog_ChooseNewWanderers());
			};
			diaNode.options.Add(diaOption2);
		}
		DiaOption diaOption3 = new DiaOption("GameOverMainMenu".Translate());
		diaOption3.action = delegate
		{
			GenScene.GoToMainMenu();
		};
		diaOption3.resolveTree = true;
		diaNode.options.Add(diaOption3);
		Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, delayInteractivity: true);
		dialog_NodeTree.screenFillColor = screenFillColor;
		dialog_NodeTree.silenceAmbientSound = !allowKeepPlaying;
		dialog_NodeTree.closeOnAccept = allowKeepPlaying;
		dialog_NodeTree.closeOnCancel = allowKeepPlaying;
		Find.WindowStack.Add(dialog_NodeTree);
		Find.Archive.Add(new ArchivedDialog(diaNode.text));
	}
}
