using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class KeyPrefsData
{
	public Dictionary<KeyBindingDef, KeyBindingData> keyPrefs = new Dictionary<KeyBindingDef, KeyBindingData>();

	public void ResetToDefaults()
	{
		keyPrefs.Clear();
		AddMissingDefaultBindings();
	}

	public void AddMissingDefaultBindings()
	{
		foreach (KeyBindingDef allDef in DefDatabase<KeyBindingDef>.AllDefs)
		{
			if (!keyPrefs.ContainsKey(allDef))
			{
				keyPrefs.Add(allDef, new KeyBindingData(allDef.defaultKeyCodeA, allDef.defaultKeyCodeB));
			}
		}
	}

	public bool SetBinding(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot, KeyCode keyCode)
	{
		if (keyPrefs.TryGetValue(keyDef, out var value))
		{
			switch (slot)
			{
			case KeyPrefs.BindingSlot.A:
				value.keyBindingA = keyCode;
				break;
			case KeyPrefs.BindingSlot.B:
				value.keyBindingB = keyCode;
				break;
			default:
				Log.Error("Tried to set a key binding for \"" + keyDef.LabelCap + "\" on a nonexistent slot: " + slot.ToString());
				return false;
			}
			return true;
		}
		Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
		return false;
	}

	public KeyCode GetBoundKeyCode(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
	{
		if (!keyPrefs.TryGetValue(keyDef, out var value))
		{
			Log.Error("Key not found in keyprefs: \"" + keyDef.LabelCap + "\"");
			return KeyCode.None;
		}
		return slot switch
		{
			KeyPrefs.BindingSlot.A => value.keyBindingA, 
			KeyPrefs.BindingSlot.B => value.keyBindingB, 
			_ => throw new InvalidOperationException(), 
		};
	}

	private IEnumerable<KeyBindingDef> ConflictingBindings(KeyBindingDef keyDef, KeyCode code)
	{
		foreach (KeyBindingDef def in DefDatabase<KeyBindingDef>.AllDefs)
		{
			if (def != keyDef && (keyDef.ignoreConflictsWith == null || !keyDef.ignoreConflictsWith.Contains(def)) && ((def.category == keyDef.category && def.category.selfConflicting) || keyDef.category.checkForConflicts.Contains(def.category) || (keyDef.extraConflictTags != null && def.extraConflictTags != null && keyDef.extraConflictTags.Any((string tag) => def.extraConflictTags.Contains(tag)))) && keyPrefs.TryGetValue(def, out var value) && (value.keyBindingA == code || value.keyBindingB == code))
			{
				yield return def;
			}
		}
	}

	public void EraseConflictingBindingsForKeyCode(KeyBindingDef keyDef, KeyCode keyCode, Action<KeyBindingDef> callBackOnErase = null)
	{
		foreach (KeyBindingDef item in ConflictingBindings(keyDef, keyCode))
		{
			KeyBindingData keyBindingData = keyPrefs[item];
			if (keyBindingData.keyBindingA == keyCode)
			{
				keyBindingData.keyBindingA = KeyCode.None;
			}
			if (keyBindingData.keyBindingB == keyCode)
			{
				keyBindingData.keyBindingB = KeyCode.None;
			}
			callBackOnErase?.Invoke(item);
		}
	}

	public void CheckConflictsFor(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
	{
		KeyCode boundKeyCode = GetBoundKeyCode(keyDef, slot);
		if (boundKeyCode != KeyCode.None)
		{
			EraseConflictingBindingsForKeyCode(keyDef, boundKeyCode);
			SetBinding(keyDef, slot, boundKeyCode);
		}
	}

	public KeyPrefsData Clone()
	{
		KeyPrefsData keyPrefsData = new KeyPrefsData();
		foreach (KeyValuePair<KeyBindingDef, KeyBindingData> keyPref in keyPrefs)
		{
			keyPrefsData.keyPrefs[keyPref.Key] = new KeyBindingData(keyPref.Value.keyBindingA, keyPref.Value.keyBindingB);
		}
		return keyPrefsData;
	}

	public void ErrorCheck()
	{
		foreach (KeyBindingDef allDef in DefDatabase<KeyBindingDef>.AllDefs)
		{
			ErrorCheckOn(allDef, KeyPrefs.BindingSlot.A);
			ErrorCheckOn(allDef, KeyPrefs.BindingSlot.B);
		}
	}

	private void ErrorCheckOn(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
	{
		KeyCode boundKeyCode = GetBoundKeyCode(keyDef, slot);
		if (boundKeyCode == KeyCode.None)
		{
			return;
		}
		foreach (KeyBindingDef item in ConflictingBindings(keyDef, boundKeyCode))
		{
			bool flag = boundKeyCode != keyDef.GetDefaultKeyCode(slot);
			Log.Warning("Key binding conflict: " + item?.ToString() + " and " + keyDef?.ToString() + " are both bound to " + boundKeyCode.ToString() + "." + (flag ? " Fixed automatically." : ""));
			if (flag)
			{
				if (slot == KeyPrefs.BindingSlot.A)
				{
					keyPrefs[keyDef].keyBindingA = keyDef.defaultKeyCodeA;
				}
				else
				{
					keyPrefs[keyDef].keyBindingB = keyDef.defaultKeyCodeB;
				}
				KeyPrefs.Save();
			}
		}
	}
}
