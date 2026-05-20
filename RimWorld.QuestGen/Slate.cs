using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.QuestGen;

public class Slate
{
	public struct VarRestoreInfo
	{
		public string name;

		public bool exists;

		public object value;

		public VarRestoreInfo(string name, bool exists, object value)
		{
			this.name = name;
			this.exists = exists;
			this.value = value;
		}
	}

	private Dictionary<string, object> vars = new Dictionary<string, object>();

	private string prefix = "";

	private bool allowNonPrefixedLookup;

	private Stack<bool> prevAllowNonPrefixedLookupStack = new Stack<bool>();

	public const char Separator = '/';

	public string CurrentPrefix => prefix;

	public T Get<T>(string name, T defaultValue = default(T), bool isAbsoluteName = false)
	{
		if (TryGet<T>(name, out var var, isAbsoluteName))
		{
			return var;
		}
		return defaultValue;
	}

	public bool TryGet<T>(string name, out T var, bool isAbsoluteName = false)
	{
		if (name.NullOrEmpty())
		{
			var = default(T);
			return false;
		}
		if (!isAbsoluteName && !prefix.NullOrEmpty())
		{
			name = prefix + "/" + name;
		}
		name = QuestGenUtility.NormalizeVarPath(name);
		if (allowNonPrefixedLookup)
		{
			name = TryResolveFirstAvailableName(name);
		}
		if (!vars.TryGetValue(name, out var value))
		{
			var = default(T);
			return false;
		}
		if (value == null)
		{
			var = default(T);
			return true;
		}
		if (value is T)
		{
			var = (T)value;
			return true;
		}
		if (ConvertHelper.CanConvert<T>(value))
		{
			var = ConvertHelper.Convert<T>(value);
			return true;
		}
		Log.Error("Could not convert slate variable \"" + name + "\" (" + value.GetType().Name + ") to " + typeof(T).Name);
		var = default(T);
		return false;
	}

	public void Set<T>(string name, T var, bool isAbsoluteName = false)
	{
		if (name.NullOrEmpty())
		{
			Log.Error("Tried to set a variable with null name. var=" + var.ToStringSafe());
			return;
		}
		if (!isAbsoluteName && !prefix.NullOrEmpty())
		{
			name = prefix + "/" + name;
		}
		name = QuestGenUtility.NormalizeVarPath(name);
		if (var is ISlateRef slateRef)
		{
			slateRef.TryGetConvertedValue<object>(this, out var value);
			vars[name] = value;
		}
		else
		{
			vars[name] = var;
		}
	}

	public void SetIfNone<T>(string name, T var, bool isAbsoluteName = false)
	{
		if (!Exists(name, isAbsoluteName))
		{
			Set(name, var, isAbsoluteName);
		}
	}

	public bool Remove(string name, bool isAbsoluteName = false)
	{
		if (name.NullOrEmpty())
		{
			return false;
		}
		if (!isAbsoluteName && !prefix.NullOrEmpty())
		{
			name = prefix + "/" + name;
		}
		name = QuestGenUtility.NormalizeVarPath(name);
		return vars.Remove(name);
	}

	public bool Exists(string name, bool isAbsoluteName = false)
	{
		if (name.NullOrEmpty())
		{
			return false;
		}
		if (!isAbsoluteName && !prefix.NullOrEmpty())
		{
			name = prefix + "/" + name;
		}
		name = QuestGenUtility.NormalizeVarPath(name);
		if (allowNonPrefixedLookup)
		{
			name = TryResolveFirstAvailableName(name);
		}
		return vars.ContainsKey(name);
	}

	private string TryResolveFirstAvailableName(string nameWithPrefix)
	{
		if (nameWithPrefix == null)
		{
			return null;
		}
		nameWithPrefix = QuestGenUtility.NormalizeVarPath(nameWithPrefix);
		if (vars.ContainsKey(nameWithPrefix))
		{
			return nameWithPrefix;
		}
		int num = nameWithPrefix.LastIndexOf('/');
		if (num < 0)
		{
			return nameWithPrefix;
		}
		string text = nameWithPrefix.Substring(num + 1);
		string text2 = nameWithPrefix.Substring(0, num);
		while (true)
		{
			string text3 = text;
			if (!text2.NullOrEmpty())
			{
				text3 = text2 + "/" + text3;
			}
			if (vars.ContainsKey(text3))
			{
				return text3;
			}
			if (text2.NullOrEmpty())
			{
				break;
			}
			int num2 = text2.LastIndexOf('/');
			text2 = ((num2 < 0) ? "" : text2.Substring(0, num2));
		}
		return nameWithPrefix;
	}

	public void PushPrefix(string newPrefix, bool allowNonPrefixedLookup = false)
	{
		if (newPrefix.NullOrEmpty())
		{
			Log.Error("Tried to push a null prefix.");
			newPrefix = "unnamed";
		}
		if (!prefix.NullOrEmpty())
		{
			prefix += "/";
		}
		prefix += newPrefix;
		prevAllowNonPrefixedLookupStack.Push(this.allowNonPrefixedLookup);
		if (allowNonPrefixedLookup)
		{
			this.allowNonPrefixedLookup = true;
		}
	}

	public void PopPrefix()
	{
		int num = prefix.LastIndexOf('/');
		if (num >= 0)
		{
			prefix = prefix.Substring(0, num);
		}
		else
		{
			prefix = "";
		}
		if (prevAllowNonPrefixedLookupStack.Count != 0)
		{
			allowNonPrefixedLookup = prevAllowNonPrefixedLookupStack.Pop();
		}
	}

	public VarRestoreInfo GetRestoreInfo(string name)
	{
		bool flag = allowNonPrefixedLookup;
		allowNonPrefixedLookup = false;
		try
		{
			object var;
			bool exists = TryGet<object>(name, out var);
			return new VarRestoreInfo(name, exists, var);
		}
		finally
		{
			allowNonPrefixedLookup = flag;
		}
	}

	public void Restore(VarRestoreInfo varRestoreInfo)
	{
		if (varRestoreInfo.exists)
		{
			Set(varRestoreInfo.name, varRestoreInfo.value);
		}
		else
		{
			Remove(varRestoreInfo.name);
		}
	}

	public void SetAll(Slate otherSlate)
	{
		vars.Clear();
		foreach (KeyValuePair<string, object> var in otherSlate.vars)
		{
			vars.Add(var.Key, var.Value);
		}
	}

	public void Reset()
	{
		vars.Clear();
	}

	public Slate DeepCopy()
	{
		Slate slate = new Slate();
		slate.prefix = prefix;
		foreach (KeyValuePair<string, object> var in vars)
		{
			slate.vars.Add(var.Key, var.Value);
		}
		return slate;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, object> item in vars.OrderBy((KeyValuePair<string, object> x) => x.Key))
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			string text = ((item.Value is IEnumerable && !(item.Value is string)) ? ((IEnumerable)item.Value).ToStringSafeEnumerable() : item.Value.ToStringSafe());
			stringBuilder.Append(item.Key + "=" + text);
		}
		if (stringBuilder.Length == 0)
		{
			stringBuilder.Append("(none)");
		}
		return stringBuilder.ToString();
	}
}
