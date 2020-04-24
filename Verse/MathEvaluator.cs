using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Verse
{
	public static class MathEvaluator
	{
		private static XPathDocument doc;

		private static XPathNavigator navigator;

		private static readonly Regex AddSpacesRegex = new Regex("([\\+\\-\\*])");

		private static readonly MathEvaluatorCustomContext Context = new MathEvaluatorCustomContext(new NameTable(), new XsltArgumentList());

		private static XPathNavigator Navigator
		{
			get
			{
				if (doc == null)
				{
					doc = new XPathDocument(new StringReader("<root />"));
				}
				if (navigator == null)
				{
					navigator = doc.CreateNavigator();
				}
				return navigator;
			}
		}

		public static double Evaluate(string expr)
		{
			if (expr.NullOrEmpty())
			{
				return 0.0;
			}
			expr = AddSpacesRegex.Replace(expr, " ${1} ");
			expr = expr.Replace("/", " div ");
			expr = expr.Replace("%", " mod ");
			try
			{
				XPathExpression xPathExpression = XPathExpression.Compile("number(" + expr + ")");
				xPathExpression.SetContext(Context);
				double num = (double)Navigator.Evaluate(xPathExpression);
				if (double.IsNaN(num))
				{
					Log.ErrorOnce("Expression \"" + expr + "\" evaluated to NaN.", expr.GetHashCode() ^ 0x2E1910A);
					num = 0.0;
				}
				return num;
			}
			catch (XPathException ex)
			{
				Log.ErrorOnce("Could not evaluate expression \"" + expr + "\". Error: " + ex, expr.GetHashCode() ^ 0x3A78A909);
				return 0.0;
			}
		}
	}
}
