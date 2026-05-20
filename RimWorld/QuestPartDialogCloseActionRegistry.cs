using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class QuestPartDialogCloseActionRegistry
	{
		private static readonly Dictionary<QuestPartDialogCloseAction.CloseActionKey, QuestPartDialogCloseAction> CloseActions = new Dictionary<QuestPartDialogCloseAction.CloseActionKey, QuestPartDialogCloseAction>
		{
			{
				QuestPartDialogCloseAction.CloseActionKey.ArchonexusVictorySound2nd,
				new QuestPartDialogCloseAction
				{
					dialogCloseSound = SoundDefOf.GameStartSting_FirstArchonexusCycle,
					dialogCloseAction = delegate
					{
						Find.MusicManagerPlay.ForceSilenceFor(8f);
					}
				}
			},
			{
				QuestPartDialogCloseAction.CloseActionKey.ArchonexusVictorySound3rd,
				new QuestPartDialogCloseAction
				{
					dialogCloseSound = SoundDefOf.GameStartSting_SecondArchonexusCycle,
					dialogCloseAction = delegate
					{
						Find.MusicManagerPlay.ForceSilenceFor(8f);
					}
				}
			}
		};

		public static QuestPartDialogCloseAction CloseActionOf(QuestPartDialogCloseAction.CloseActionKey key)
		{
			return CloseActions.TryGetValue(key);
		}
	}
}
