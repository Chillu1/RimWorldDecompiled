using System;
using Verse;

namespace RimWorld
{
	public class QuestPartDialogCloseAction
	{
		public enum CloseActionKey
		{
			None,
			ArchonexusVictorySound2nd,
			ArchonexusVictorySound3rd
		}

		public SoundDef dialogCloseSound;

		public Action dialogCloseAction;
	}
}
