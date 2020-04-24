using Verse;

namespace RimWorld
{
	public struct Signal
	{
		public string tag;

		public SignalArgs args;

		public Signal(string tag)
		{
			this.tag = tag;
			args = default(SignalArgs);
		}

		public Signal(string tag, SignalArgs args)
		{
			this.tag = tag;
			this.args = args;
		}

		public Signal(string tag, NamedArgument arg1)
		{
			this.tag = tag;
			args = new SignalArgs(arg1);
		}

		public Signal(string tag, NamedArgument arg1, NamedArgument arg2)
		{
			this.tag = tag;
			args = new SignalArgs(arg1, arg2);
		}

		public Signal(string tag, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			this.tag = tag;
			args = new SignalArgs(arg1, arg2, arg3);
		}

		public Signal(string tag, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			this.tag = tag;
			args = new SignalArgs(arg1, arg2, arg3, arg4);
		}

		public Signal(string tag, params NamedArgument[] args)
		{
			this.tag = tag;
			this.args = new SignalArgs(args);
		}
	}
}
