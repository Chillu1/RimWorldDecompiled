using System.Xml.XPath;
using System.Xml.Xsl;

namespace Verse
{
	public class MathEvaluatorCustomVariable : IXsltContextVariable
	{
		private string prefix;

		private string name;

		public bool IsLocal => false;

		public bool IsParam => false;

		public XPathResultType VariableType => XPathResultType.Any;

		public MathEvaluatorCustomVariable(string prefix, string name)
		{
			this.prefix = prefix;
			this.name = name;
		}

		public object Evaluate(XsltContext xsltContext)
		{
			return ((MathEvaluatorCustomContext)xsltContext).ArgList.GetParam(name, prefix);
		}
	}
}
