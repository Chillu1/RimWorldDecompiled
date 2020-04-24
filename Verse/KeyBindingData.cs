using UnityEngine;

namespace Verse
{
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
			string str = "[";
			if (keyBindingA != 0)
			{
				str += keyBindingA.ToString();
			}
			if (keyBindingB != 0)
			{
				str = str + ", " + keyBindingB.ToString();
			}
			return str + "]";
		}
	}
}
