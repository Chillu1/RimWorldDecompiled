using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar;

public abstract class Rule
{
	public struct ConstantConstraint
	{
		public enum Type
		{
			Equal,
			NotEqual,
			Less,
			Greater,
			LessOrEqual,
			GreaterOrEqual
		}

		[MayTranslate]
		public string key;

		[MayTranslate]
		public string value;

		public Type type;
	}

	[MayTranslate]
	public string keyword;

	[NoTranslate]
	public string tag;

	[NoTranslate]
	public string requiredTag;

	public int? usesLimit;

	public List<ConstantConstraint> constantConstraints;

	public abstract float BaseSelectionWeight { get; }

	public virtual float Priority => 0f;

	public virtual Rule DeepCopy()
	{
		Rule rule = (Rule)Activator.CreateInstance(GetType());
		rule.keyword = keyword;
		rule.tag = tag;
		rule.requiredTag = requiredTag;
		if (constantConstraints != null)
		{
			rule.constantConstraints = constantConstraints.ToList();
		}
		return rule;
	}

	public abstract string Generate();

	public virtual void Init()
	{
	}

	public void AddConstantConstraint(string key, string value, ConstantConstraint.Type type)
	{
		if (constantConstraints == null)
		{
			constantConstraints = new List<ConstantConstraint>();
		}
		constantConstraints.Add(new ConstantConstraint
		{
			key = key,
			value = value,
			type = type
		});
	}

	public void AddConstantConstraint(string key, string value, string op)
	{
		ConstantConstraint.Type type;
		switch (op)
		{
		case "==":
			type = ConstantConstraint.Type.Equal;
			break;
		case "!=":
			type = ConstantConstraint.Type.NotEqual;
			break;
		case "<":
			type = ConstantConstraint.Type.Less;
			break;
		case "[less_than]":
			type = ConstantConstraint.Type.Less;
			break;
		case ">":
			type = ConstantConstraint.Type.Greater;
			break;
		case "[greater_than]":
			type = ConstantConstraint.Type.Greater;
			break;
		case "<=":
			type = ConstantConstraint.Type.LessOrEqual;
			break;
		case ">=":
			type = ConstantConstraint.Type.GreaterOrEqual;
			break;
		default:
			type = ConstantConstraint.Type.Equal;
			Log.Error("Unknown ConstantConstraint type: " + op);
			break;
		}
		AddConstantConstraint(key, value, type);
	}

	public bool ValidateConstraints(Dictionary<string, string> constraints)
	{
		bool result = true;
		if (constantConstraints != null)
		{
			for (int i = 0; i < constantConstraints.Count; i++)
			{
				ConstantConstraint constantConstraint = constantConstraints[i];
				string text = ((constraints != null) ? constraints.TryGetValue(constantConstraint.key, "") : "");
				float result2 = 0f;
				float result3 = 0f;
				bool flag = !text.NullOrEmpty() && !constantConstraint.value.NullOrEmpty() && float.TryParse(text, out result2) && float.TryParse(constantConstraint.value, out result3);
				bool flag2;
				switch (constantConstraint.type)
				{
				case ConstantConstraint.Type.Equal:
					flag2 = text.EqualsIgnoreCase(constantConstraint.value);
					break;
				case ConstantConstraint.Type.NotEqual:
					flag2 = !text.EqualsIgnoreCase(constantConstraint.value);
					break;
				case ConstantConstraint.Type.Less:
					flag2 = flag && result2 < result3;
					break;
				case ConstantConstraint.Type.Greater:
					flag2 = flag && result2 > result3;
					break;
				case ConstantConstraint.Type.LessOrEqual:
					flag2 = flag && result2 <= result3;
					break;
				case ConstantConstraint.Type.GreaterOrEqual:
					flag2 = flag && result2 >= result3;
					break;
				default:
					Log.Error("Unknown ConstantConstraint type: " + constantConstraint.type);
					flag2 = false;
					break;
				}
				if (!flag2)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}
}
