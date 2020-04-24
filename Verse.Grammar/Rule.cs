using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
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

		public List<ConstantConstraint> constantConstraints;

		public abstract float BaseSelectionWeight
		{
			get;
		}

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
			if (!(op == "=="))
			{
				if (!(op == "!="))
				{
					if (!(op == "<"))
					{
						if (!(op == ">"))
						{
							if (!(op == "<="))
							{
								if (op == ">=")
								{
									type = ConstantConstraint.Type.GreaterOrEqual;
								}
								else
								{
									type = ConstantConstraint.Type.Equal;
									Log.Error("Unknown ConstantConstraint type: " + op);
								}
							}
							else
							{
								type = ConstantConstraint.Type.LessOrEqual;
							}
						}
						else
						{
							type = ConstantConstraint.Type.Greater;
						}
					}
					else
					{
						type = ConstantConstraint.Type.Less;
					}
				}
				else
				{
					type = ConstantConstraint.Type.NotEqual;
				}
			}
			else
			{
				type = ConstantConstraint.Type.Equal;
			}
			AddConstantConstraint(key, value, type);
		}
	}
}
