using UnityEngine;

namespace Verse
{
	public class SimpleCurveDrawerStyle
	{
		public bool DrawBackground
		{
			get;
			set;
		}

		public bool DrawBackgroundLines
		{
			get;
			set;
		}

		public bool DrawMeasures
		{
			get;
			set;
		}

		public bool DrawPoints
		{
			get;
			set;
		}

		public bool DrawLegend
		{
			get;
			set;
		}

		public bool DrawCurveMousePoint
		{
			get;
			set;
		}

		public bool OnlyPositiveValues
		{
			get;
			set;
		}

		public bool UseFixedSection
		{
			get;
			set;
		}

		public bool UseFixedScale
		{
			get;
			set;
		}

		public bool UseAntiAliasedLines
		{
			get;
			set;
		}

		public bool PointsRemoveOptimization
		{
			get;
			set;
		}

		public int MeasureLabelsXCount
		{
			get;
			set;
		}

		public int MeasureLabelsYCount
		{
			get;
			set;
		}

		public bool XIntegersOnly
		{
			get;
			set;
		}

		public bool YIntegersOnly
		{
			get;
			set;
		}

		public string LabelX
		{
			get;
			set;
		}

		public FloatRange FixedSection
		{
			get;
			set;
		}

		public Vector2 FixedScale
		{
			get;
			set;
		}

		public SimpleCurveDrawerStyle()
		{
			DrawBackground = false;
			DrawBackgroundLines = true;
			DrawMeasures = false;
			DrawPoints = true;
			DrawLegend = false;
			DrawCurveMousePoint = false;
			OnlyPositiveValues = false;
			UseFixedSection = false;
			UseFixedScale = false;
			UseAntiAliasedLines = false;
			PointsRemoveOptimization = false;
			MeasureLabelsXCount = 5;
			MeasureLabelsYCount = 5;
			XIntegersOnly = false;
			YIntegersOnly = false;
			LabelX = "x";
		}
	}
}
