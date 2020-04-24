using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ShortcutKeys
	{
		public void ShortcutKeysOnGUI()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (KeyBindingDefOf.NextColonist.KeyDownEvent)
				{
					ThingSelectionUtility.SelectNextColonist();
					Event.current.Use();
				}
				if (KeyBindingDefOf.PreviousColonist.KeyDownEvent)
				{
					ThingSelectionUtility.SelectPreviousColonist();
					Event.current.Use();
				}
			}
		}
	}
}
