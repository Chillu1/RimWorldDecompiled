using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public struct SignalArgs
{
	private int count;

	private NamedArgument arg1;

	private NamedArgument arg2;

	private NamedArgument arg3;

	private NamedArgument arg4;

	private NamedArgument[] args;

	public int Count => count;

	public IEnumerable<NamedArgument> Args
	{
		get
		{
			if (count == 0)
			{
				yield break;
			}
			if (args != null)
			{
				for (int i = 0; i < args.Length; i++)
				{
					yield return args[i];
				}
				yield break;
			}
			yield return arg1;
			if (count >= 2)
			{
				yield return arg2;
			}
			if (count >= 3)
			{
				yield return arg3;
			}
			if (count >= 4)
			{
				yield return arg4;
			}
		}
	}

	public SignalArgs(SignalArgs args)
	{
		count = args.count;
		arg1 = args.arg1;
		arg2 = args.arg2;
		arg3 = args.arg3;
		arg4 = args.arg4;
		this.args = args.args;
	}

	public SignalArgs(NamedArgument arg1)
	{
		count = 1;
		this.arg1 = arg1;
		arg2 = default(NamedArgument);
		arg3 = default(NamedArgument);
		arg4 = default(NamedArgument);
		args = null;
	}

	public SignalArgs(NamedArgument arg1, NamedArgument arg2)
	{
		count = 2;
		this.arg1 = arg1;
		this.arg2 = arg2;
		arg3 = default(NamedArgument);
		arg4 = default(NamedArgument);
		args = null;
	}

	public SignalArgs(NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
	{
		count = 3;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = arg3;
		arg4 = default(NamedArgument);
		args = null;
	}

	public SignalArgs(NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
	{
		count = 4;
		this.arg1 = arg1;
		this.arg2 = arg2;
		this.arg3 = arg3;
		this.arg4 = arg4;
		args = null;
	}

	public SignalArgs(params NamedArgument[] args)
	{
		count = args.Length;
		if (args.Length > 4)
		{
			arg1 = default(NamedArgument);
			arg2 = default(NamedArgument);
			arg3 = default(NamedArgument);
			arg4 = default(NamedArgument);
			this.args = new NamedArgument[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				this.args[i] = args[i];
			}
			return;
		}
		if (args.Length == 1)
		{
			arg1 = args[0];
			arg2 = default(NamedArgument);
			arg3 = default(NamedArgument);
			arg4 = default(NamedArgument);
		}
		else if (args.Length == 2)
		{
			arg1 = args[0];
			arg2 = args[1];
			arg3 = default(NamedArgument);
			arg4 = default(NamedArgument);
		}
		else if (args.Length == 3)
		{
			arg1 = args[0];
			arg2 = args[1];
			arg3 = args[2];
			arg4 = default(NamedArgument);
		}
		else if (args.Length == 4)
		{
			arg1 = args[0];
			arg2 = args[1];
			arg3 = args[2];
			arg4 = args[3];
		}
		else
		{
			arg1 = default(NamedArgument);
			arg2 = default(NamedArgument);
			arg3 = default(NamedArgument);
			arg4 = default(NamedArgument);
		}
		this.args = null;
	}

	public bool TryGetArg(int index, out NamedArgument arg)
	{
		if (index < 0 || index >= count)
		{
			arg = default(NamedArgument);
			return false;
		}
		if (args != null)
		{
			arg = args[index];
		}
		else
		{
			switch (index)
			{
			case 0:
				arg = arg1;
				break;
			case 1:
				arg = arg2;
				break;
			case 2:
				arg = arg3;
				break;
			default:
				arg = arg4;
				break;
			}
		}
		return true;
	}

	public bool TryGetArg(string name, out NamedArgument arg)
	{
		if (count == 0)
		{
			arg = default(NamedArgument);
			return false;
		}
		if (args != null)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].label == name)
				{
					arg = args[i];
					return true;
				}
			}
		}
		else
		{
			if (count >= 1 && arg1.label == name)
			{
				arg = arg1;
				return true;
			}
			if (count >= 2 && arg2.label == name)
			{
				arg = arg2;
				return true;
			}
			if (count >= 3 && arg3.label == name)
			{
				arg = arg3;
				return true;
			}
			if (count >= 4 && arg4.label == name)
			{
				arg = arg4;
				return true;
			}
		}
		arg = default(NamedArgument);
		return false;
	}

	public bool TryGetArg<T>(int index, out T arg)
	{
		if (!TryGetArg(index, out var arg2) || !(arg2.arg is T val))
		{
			arg = default(T);
			return false;
		}
		arg = val;
		return true;
	}

	public bool TryGetArg<T>(string name, out T arg)
	{
		if (!TryGetArg(name, out var arg2) || !(arg2.arg is T))
		{
			arg = default(T);
			return false;
		}
		arg = (T)arg2.arg;
		return true;
	}

	public NamedArgument GetArg(int index)
	{
		if (TryGetArg(index, out var arg))
		{
			return arg;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public NamedArgument GetArg(string name)
	{
		if (TryGetArg(name, out var arg))
		{
			return arg;
		}
		throw new ArgumentException("Could not find arg named " + name);
	}

	public T GetArg<T>(string name)
	{
		if (TryGetArg(name, out T arg))
		{
			return arg;
		}
		throw new ArgumentException("Could not find arg named " + name + " of type " + typeof(T).Name);
	}

	public TaggedString GetFormattedText(TaggedString text)
	{
		if (count == 0)
		{
			return text.Formatted();
		}
		if (args != null)
		{
			return text.Formatted(args);
		}
		if (count == 1)
		{
			return text.Formatted(arg1);
		}
		if (count == 2)
		{
			return text.Formatted(arg1, arg2);
		}
		if (count == 3)
		{
			return text.Formatted(arg1, arg2, arg3);
		}
		return text.Formatted(arg1, arg2, arg3, arg4);
	}

	public TaggedString GetTranslatedText(string textKey)
	{
		if (count == 0)
		{
			return textKey.Translate();
		}
		if (args != null)
		{
			return textKey.Translate(args);
		}
		if (count == 1)
		{
			return textKey.Translate(arg1);
		}
		if (count == 2)
		{
			return textKey.Translate(arg1, arg2);
		}
		if (count == 3)
		{
			return textKey.Translate(arg1, arg2, arg3);
		}
		return textKey.Translate(arg1, arg2, arg3, arg4);
	}

	public void Add(NamedArgument arg)
	{
		if (args != null)
		{
			NamedArgument[] array = new NamedArgument[args.Length + 1];
			for (int i = 0; i < args.Length; i++)
			{
				array[i] = args[i];
			}
			array[^1] = arg;
			args = array;
			count = args.Length;
			return;
		}
		if (count == 0)
		{
			arg1 = arg;
		}
		else if (count == 1)
		{
			arg2 = arg;
		}
		else if (count == 2)
		{
			arg3 = arg;
		}
		else if (count == 3)
		{
			arg4 = arg;
		}
		else
		{
			args = new NamedArgument[5];
			args[0] = arg1;
			args[1] = arg2;
			args[2] = arg3;
			args[3] = arg4;
			args[4] = arg;
			arg1 = default(NamedArgument);
			arg2 = default(NamedArgument);
			arg3 = default(NamedArgument);
			arg4 = default(NamedArgument);
		}
		count++;
	}

	public void Add(NamedArgument arg1, NamedArgument arg2)
	{
		Add(arg1);
		Add(arg2);
	}

	public void Add(NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
	{
		Add(arg1);
		Add(arg2);
		Add(arg3);
	}

	public void Add(NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
	{
		Add(arg1);
		Add(arg2);
		Add(arg3);
		Add(arg4);
	}

	public void Add(params NamedArgument[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			Add(args[i]);
		}
	}

	public void Add(SignalArgs args)
	{
		if (args.count == 0)
		{
			return;
		}
		if (args.args != null)
		{
			for (int i = 0; i < args.args.Length; i++)
			{
				Add(args.args[i]);
			}
			return;
		}
		if (args.count >= 1)
		{
			Add(args.arg1);
		}
		if (args.count >= 2)
		{
			Add(args.arg2);
		}
		if (args.count >= 3)
		{
			Add(args.arg3);
		}
		if (args.count >= 4)
		{
			Add(args.arg4);
		}
	}
}
