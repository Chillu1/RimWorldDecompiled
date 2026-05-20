using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class KeyBindingDef : Def
{
	public KeyBindingCategoryDef category;

	public KeyCode defaultKeyCodeA;

	public KeyCode defaultKeyCodeB;

	public bool devModeOnly;

	public List<KeyBindingDef> ignoreConflictsWith;

	[NoTranslate]
	public List<string> extraConflictTags;

	public KeyCode MainKey
	{
		get
		{
			if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out var value))
			{
				if (value.keyBindingA != KeyCode.None)
				{
					return value.keyBindingA;
				}
				if (value.keyBindingB != KeyCode.None)
				{
					return value.keyBindingB;
				}
			}
			return KeyCode.None;
		}
	}

	public string MainKeyLabel => MainKey.ToStringReadable();

	public bool KeyDownEvent
	{
		get
		{
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None && KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out var value))
			{
				if (value.keyBindingA != KeyCode.LeftMeta && value.keyBindingA != KeyCode.RightMeta && value.keyBindingB != KeyCode.LeftMeta && value.keyBindingB != KeyCode.RightMeta && Event.current.command)
				{
					return false;
				}
				if (Find.WindowStack.AnySearchWidgetFocused)
				{
					return false;
				}
				if (Event.current.keyCode != value.keyBindingA)
				{
					return Event.current.keyCode == value.keyBindingB;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsDownEvent
	{
		get
		{
			if (Event.current == null)
			{
				return false;
			}
			if (!KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out var value))
			{
				return false;
			}
			if (Find.WindowStack.AnySearchWidgetFocused)
			{
				return false;
			}
			if (KeyDownEvent)
			{
				return true;
			}
			if (Event.current.shift && (value.keyBindingA == KeyCode.LeftShift || value.keyBindingA == KeyCode.RightShift || value.keyBindingB == KeyCode.LeftShift || value.keyBindingB == KeyCode.RightShift))
			{
				return true;
			}
			if (Event.current.control && (value.keyBindingA == KeyCode.LeftControl || value.keyBindingA == KeyCode.RightControl || value.keyBindingB == KeyCode.LeftControl || value.keyBindingB == KeyCode.RightControl))
			{
				return true;
			}
			if (Event.current.alt && (value.keyBindingA == KeyCode.LeftAlt || value.keyBindingA == KeyCode.RightAlt || value.keyBindingB == KeyCode.LeftAlt || value.keyBindingB == KeyCode.RightAlt))
			{
				return true;
			}
			if (Event.current.command && (value.keyBindingA == KeyCode.LeftMeta || value.keyBindingA == KeyCode.RightMeta || value.keyBindingB == KeyCode.LeftMeta || value.keyBindingB == KeyCode.RightMeta))
			{
				return true;
			}
			return IsDown;
		}
	}

	public bool JustPressed
	{
		get
		{
			if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out var value))
			{
				if (Find.WindowStack.AnySearchWidgetFocused)
				{
					return false;
				}
				if (!Input.GetKeyDown(value.keyBindingA))
				{
					return Input.GetKeyDown(value.keyBindingB);
				}
				return true;
			}
			return false;
		}
	}

	public bool IsDown
	{
		get
		{
			if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out var value))
			{
				if (Find.WindowStack.AnySearchWidgetFocused)
				{
					return false;
				}
				if (!Input.GetKey(value.keyBindingA))
				{
					return Input.GetKey(value.keyBindingB);
				}
				return true;
			}
			return false;
		}
	}

	public KeyCode GetDefaultKeyCode(KeyPrefs.BindingSlot slot)
	{
		return slot switch
		{
			KeyPrefs.BindingSlot.A => defaultKeyCodeA, 
			KeyPrefs.BindingSlot.B => defaultKeyCodeB, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static KeyBindingDef Named(string name)
	{
		return DefDatabase<KeyBindingDef>.GetNamedSilentFail(name);
	}
}
