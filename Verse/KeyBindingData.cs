using UnityEngine;

namespace Verse;

public class KeyBindingData
{
	public KeyCode keyBindingA;

	public KeyCode keyBindingB;

	public KeyBindingData()
	{
	}

	public KeyBindingData(KeyCode keyBindingA, KeyCode keyBindingB)
	{
		this.keyBindingA = keyBindingA;
		this.keyBindingB = keyBindingB;
	}

	public override string ToString()
	{
		string text = "[";
		if (keyBindingA != KeyCode.None)
		{
			text += keyBindingA;
		}
		if (keyBindingB != KeyCode.None)
		{
			text = text + ", " + keyBindingB;
		}
		return text + "]";
	}
}
