using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class KeyBindingDef : Def
	{
		public KeyBindingCategoryDef category;

		public KeyCode defaultKeyCodeA;

		public KeyCode defaultKeyCodeB;

		public bool devModeOnly;

		[NoTranslate]
		public List<string> extraConflictTags;

		public KeyCode MainKey
		{
			get
			{
				if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out KeyBindingData value))
				{
					if (value.keyBindingA != 0)
					{
						return value.keyBindingA;
					}
					if (value.keyBindingB != 0)
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
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode != 0 && KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out KeyBindingData value))
				{
					if (value.keyBindingA != KeyCode.LeftCommand && value.keyBindingA != KeyCode.RightCommand && value.keyBindingB != KeyCode.LeftCommand && value.keyBindingB != KeyCode.RightCommand && Event.current.command)
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
				if (!KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out KeyBindingData value))
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
				if (Event.current.command && (value.keyBindingA == KeyCode.LeftCommand || value.keyBindingA == KeyCode.RightCommand || value.keyBindingB == KeyCode.LeftCommand || value.keyBindingB == KeyCode.RightCommand))
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
				if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out KeyBindingData value))
				{
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
				if (KeyPrefs.KeyPrefsData.keyPrefs.TryGetValue(this, out KeyBindingData value))
				{
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
			switch (slot)
			{
			case KeyPrefs.BindingSlot.A:
				return defaultKeyCodeA;
			case KeyPrefs.BindingSlot.B:
				return defaultKeyCodeB;
			default:
				throw new InvalidOperationException();
			}
		}

		public static KeyBindingDef Named(string name)
		{
			return DefDatabase<KeyBindingDef>.GetNamedSilentFail(name);
		}
	}
}
