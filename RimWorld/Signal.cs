using Verse;

namespace RimWorld;

public struct Signal
{
	public string tag;

	public SignalArgs args;

	public bool global;

	public Signal(string tag, bool global = false)
	{
		this.tag = tag;
		args = default(SignalArgs);
		this.global = global;
	}

	public Signal(string tag, SignalArgs args, bool global = false)
	{
		this.tag = tag;
		this.args = args;
		this.global = global;
	}

	public Signal(string tag, NamedArgument arg1)
	{
		this.tag = tag;
		args = new SignalArgs(arg1);
		global = false;
	}

	public Signal(string tag, NamedArgument arg1, NamedArgument arg2)
	{
		this.tag = tag;
		args = new SignalArgs(arg1, arg2);
		global = false;
	}

	public Signal(string tag, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
	{
		this.tag = tag;
		args = new SignalArgs(arg1, arg2, arg3);
		global = false;
	}

	public Signal(string tag, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
	{
		this.tag = tag;
		args = new SignalArgs(arg1, arg2, arg3, arg4);
		global = false;
	}

	public Signal(string tag, params NamedArgument[] args)
	{
		this.tag = tag;
		this.args = new SignalArgs(args);
		global = false;
	}

	public override string ToString()
	{
		return tag;
	}
}
