using System;

namespace Verse
{
	public struct DebugMenuOption
	{
		public DebugMenuOptionMode mode;

		public string label;

		public Action method;

		public DebugMenuOption(string label, DebugMenuOptionMode mode, Action method)
		{
			this.label = label;
			this.method = method;
			this.mode = mode;
		}
	}
}
