using UnityEngine;
using Verse;
using Verse.Steam;

namespace RimWorld;

public class Dialog_DefineBinding : Window
{
	protected Vector2 windowSize = new Vector2(400f, 200f);

	protected KeyPrefsData keyPrefsData;

	protected KeyBindingDef keyDef;

	protected KeyPrefs.BindingSlot slot;

	public override Vector2 InitialSize => windowSize;

	protected override float Margin => 0f;

	public Dialog_DefineBinding(KeyPrefsData keyPrefsData, KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
	{
		this.keyDef = keyDef;
		this.slot = slot;
		this.keyPrefsData = keyPrefsData;
		closeOnAccept = false;
		closeOnCancel = false;
		forcePause = true;
		onlyOneOfTypeAllowed = true;
		absorbInputAroundWindow = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Anchor = TextAnchor.MiddleCenter;
		if (SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			Widgets.Label(inRect, "PressAnyKeyOrEscController".Translate().Resolve().AdjustedForKeys());
		}
		else
		{
			Widgets.Label(inRect, "PressAnyKeyOrEsc".Translate());
		}
		Text.Anchor = TextAnchor.UpperLeft;
		if (!Event.current.isKey || Event.current.type != EventType.KeyDown || Event.current.keyCode == KeyCode.None)
		{
			return;
		}
		if (Event.current.keyCode != KeyCode.Escape)
		{
			keyPrefsData.EraseConflictingBindingsForKeyCode(keyDef, Event.current.keyCode, delegate(KeyBindingDef oldDef)
			{
				Messages.Message("KeyBindingOverwritten".Translate(oldDef.LabelCap), MessageTypeDefOf.TaskCompletion, historical: false);
			});
			keyPrefsData.SetBinding(keyDef, slot, Event.current.keyCode);
		}
		Close();
		Event.current.Use();
	}
}
