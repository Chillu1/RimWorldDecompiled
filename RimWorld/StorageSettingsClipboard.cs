using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StorageSettingsClipboard
	{
		private static StorageSettings clipboard = new StorageSettings();

		private static bool copied = false;

		public static bool HasCopiedSettings => copied;

		public static void Copy(StorageSettings s)
		{
			clipboard.CopyFrom(s);
			copied = true;
		}

		public static void PasteInto(StorageSettings s)
		{
			s.CopyFrom(clipboard);
		}

		public static IEnumerable<Gizmo> CopyPasteGizmosFor(StorageSettings s)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings");
			command_Action.defaultLabel = "CommandCopyZoneSettingsLabel".Translate();
			command_Action.defaultDesc = "CommandCopyZoneSettingsDesc".Translate();
			command_Action.action = delegate
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				Copy(s);
			};
			command_Action.hotKey = KeyBindingDefOf.Misc4;
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings");
			command_Action2.defaultLabel = "CommandPasteZoneSettingsLabel".Translate();
			command_Action2.defaultDesc = "CommandPasteZoneSettingsDesc".Translate();
			command_Action2.action = delegate
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				PasteInto(s);
			};
			command_Action2.hotKey = KeyBindingDefOf.Misc5;
			if (!HasCopiedSettings)
			{
				command_Action2.Disable();
			}
			yield return command_Action2;
		}
	}
}
