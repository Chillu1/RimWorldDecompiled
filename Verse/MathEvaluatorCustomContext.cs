using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Verse
{
	public class MathEvaluatorCustomContext : XsltContext
	{
		private XsltArgumentList argList;

		public override bool Whitespace => true;

		public XsltArgumentList ArgList => argList;

		public MathEvaluatorCustomContext()
		{
		}

		public MathEvaluatorCustomContext(NameTable nt, XsltArgumentList args)
			: base(nt)
		{
			argList = args;
		}

		public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
		{
			MathEvaluatorCustomFunctions.FunctionType[] functionTypes = MathEvaluatorCustomFunctions.FunctionTypes;
			for (int i = 0; i < functionTypes.Length; i++)
			{
				if (functionTypes[i].name == name)
				{
					return new MathEvaluatorCustomFunction(functionTypes[i], argTypes);
				}
			}
			return null;
		}

		public override IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			if (ArgList.GetParam(name, prefix) != null)
			{
				return new MathEvaluatorCustomVariable(prefix, name);
			}
			return null;
		}

		public override bool PreserveWhitespace(XPathNavigator node)
		{
			return false;
		}

		public override int CompareDocument(string baseUri, string nextbaseUri)
		{
			return 0;
		}
	}
}
