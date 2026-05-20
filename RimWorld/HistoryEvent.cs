using Verse;

namespace RimWorld
{
	public struct HistoryEvent
	{
		public HistoryEventDef def;

		public SignalArgs args;

		public HistoryEvent(HistoryEventDef def)
		{
			this.def = def;
			args = default(SignalArgs);
		}

		public HistoryEvent(HistoryEventDef def, SignalArgs args)
		{
			this.def = def;
			this.args = args;
		}

		public HistoryEvent(HistoryEventDef def, NamedArgument arg1)
		{
			this.def = def;
			args = new SignalArgs(arg1);
		}

		public HistoryEvent(HistoryEventDef def, NamedArgument arg1, NamedArgument arg2)
		{
			this.def = def;
			args = new SignalArgs(arg1, arg2);
		}

		public HistoryEvent(HistoryEventDef def, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			this.def = def;
			args = new SignalArgs(arg1, arg2, arg3);
		}

		public HistoryEvent(HistoryEventDef def, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			this.def = def;
			args = new SignalArgs(arg1, arg2, arg3, arg4);
		}

		public HistoryEvent(HistoryEventDef def, params NamedArgument[] args)
		{
			this.def = def;
			this.args = new SignalArgs(args);
		}
	}
}
