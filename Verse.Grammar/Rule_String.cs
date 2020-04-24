using RimWorld.QuestGen;
using System.Text.RegularExpressions;

namespace Verse.Grammar
{
	public class Rule_String : Rule
	{
		[MustTranslate]
		private string output;

		private float weight = 1f;

		private float priority;

		private static Regex pattern = new Regex("\r\n\t\t# hold on to your butts, this is gonna get weird\r\n\r\n\t\t^\r\n\t\t(?<keyword>[a-zA-Z0-9_/]+)\t\t\t\t\t# keyword; roughly limited to standard C# identifier rules\r\n\t\t(\t\t\t\t\t\t\t\t\t\t\t# parameter list is optional, open the capture group so we can keep it or ignore it\r\n\t\t\t\\(\t\t\t\t\t\t\t\t\t\t# this is the actual parameter list opening\r\n\t\t\t\t(\t\t\t\t\t\t\t\t\t# unlimited number of parameter groups\r\n\t\t\t\t\t(?<paramname>[a-zA-Z0-9_/]+)\t# parameter name is similar\r\n\t\t\t\t\t(?<paramoperator>==|=|!=|>=|<=|>|<|) # operators; empty operator is allowed\r\n\t\t\t\t\t(?<paramvalue>[^\\,\\)]*)\t\t\t# parameter value, however, allows everything except comma and closeparen!\r\n\t\t\t\t\t,?\t\t\t\t\t\t\t\t# comma can be used to separate blocks; it is also silently ignored if it's a trailing comma\r\n\t\t\t\t)*\r\n\t\t\t\\)\r\n\t\t)?\r\n\t\t->(?<output>.*)\t\t\t\t\t\t\t\t# output is anything-goes\r\n\t\t$\r\n\r\n\t\t", RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

		private static string tmpPrefix;

		public override float BaseSelectionWeight => weight;

		public override float Priority => priority;

		public override Rule DeepCopy()
		{
			Rule_String obj = (Rule_String)base.DeepCopy();
			obj.output = output;
			obj.weight = weight;
			obj.priority = priority;
			return obj;
		}

		public Rule_String()
		{
		}

		public Rule_String(string keyword, string output)
		{
			base.keyword = keyword;
			this.output = output;
		}

		public Rule_String(string rawString)
		{
			Match match = pattern.Match(rawString);
			if (!match.Success)
			{
				Log.Error($"Bad string pass when reading rule {rawString}");
				return;
			}
			keyword = match.Groups["keyword"].Value;
			output = match.Groups["output"].Value;
			for (int i = 0; i < match.Groups["paramname"].Captures.Count; i++)
			{
				string value = match.Groups["paramname"].Captures[i].Value;
				string value2 = match.Groups["paramoperator"].Captures[i].Value;
				string value3 = match.Groups["paramvalue"].Captures[i].Value;
				if (value == "p")
				{
					if (value2 != "=")
					{
						Log.Error($"Attempt to compare p instead of assigning in rule {rawString}");
					}
					weight = float.Parse(value3);
				}
				else if (value == "priority")
				{
					if (value2 != "=")
					{
						Log.Error($"Attempt to compare priority instead of assigning in rule {rawString}");
					}
					priority = float.Parse(value3);
				}
				else if (value == "tag")
				{
					if (value2 != "=")
					{
						Log.Error($"Attempt to compare tag instead of assigning in rule {rawString}");
					}
					tag = value3;
				}
				else if (value == "requiredTag")
				{
					if (value2 != "=")
					{
						Log.Error($"Attempt to compare requiredTag instead of assigning in rule {rawString}");
					}
					requiredTag = value3;
				}
				else if (value == "debug")
				{
					Log.Error($"Rule {rawString} contains debug flag; fix before commit");
				}
				else if (value2 == "==" || value2 == "!=" || value2 == ">" || value2 == "<" || value2 == ">=" || value2 == "<=")
				{
					AddConstantConstraint(value, value3, value2);
				}
				else
				{
					Log.Error($"Unknown parameter {value} in rule {rawString}");
				}
			}
		}

		public override string Generate()
		{
			return output;
		}

		public override string ToString()
		{
			return ((keyword != null) ? keyword : "null_keyword") + " → " + ((output != null) ? output.Replace("\n", "\\n") : "null_output");
		}

		public void AppendPrefixToAllKeywords(string prefix)
		{
			tmpPrefix = prefix;
			if (output == null)
			{
				Log.Error("Rule_String output was null.");
				output = "";
			}
			output = Regex.Replace(output, "\\[(.*?)\\]", RegexMatchEvaluatorAppendPrefix);
			if (constantConstraints == null)
			{
				return;
			}
			for (int i = 0; i < constantConstraints.Count; i++)
			{
				ConstantConstraint value = default(ConstantConstraint);
				value.key = constantConstraints[i].key;
				if (!prefix.NullOrEmpty())
				{
					value.key = prefix + "/" + value.key;
				}
				value.key = QuestGenUtility.NormalizeVarPath(value.key);
				value.value = constantConstraints[i].value;
				value.type = constantConstraints[i].type;
				constantConstraints[i] = value;
			}
		}

		private static string RegexMatchEvaluatorAppendPrefix(Match match)
		{
			string text = match.Groups[1].Value;
			if (!tmpPrefix.NullOrEmpty())
			{
				text = tmpPrefix + "/" + text;
			}
			text = QuestGenUtility.NormalizeVarPath(text);
			return "[" + text + "]";
		}
	}
}
