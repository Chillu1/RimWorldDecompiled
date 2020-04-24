using System.Xml.XPath;
using System.Xml.Xsl;

namespace Verse
{
	public class MathEvaluatorCustomFunction : IXsltContextFunction
	{
		private XPathResultType[] argTypes;

		private MathEvaluatorCustomFunctions.FunctionType functionType;

		public XPathResultType[] ArgTypes => argTypes;

		public int Maxargs => functionType.maxArgs;

		public int Minargs => functionType.minArgs;

		public XPathResultType ReturnType => XPathResultType.Number;

		public MathEvaluatorCustomFunction(MathEvaluatorCustomFunctions.FunctionType functionType, XPathResultType[] argTypes)
		{
			this.functionType = functionType;
			this.argTypes = argTypes;
		}

		public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
		{
			return functionType.func(args);
		}
	}
}
