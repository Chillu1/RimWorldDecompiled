using System;
using Verse;

namespace RimWorld;

public class StatModifierQuality
{
	public StatDef stat;

	private float awful;

	private float poor;

	private float normal;

	private float good;

	private float excellent;

	private float masterwork;

	private float legendary;

	public string ToStringAsOffsetRange
	{
		get
		{
			string text = stat.Worker.ValueToString(GetValue(QualityCategory.Awful), finalized: false, ToStringNumberSense.Offset);
			string text2 = stat.Worker.ValueToString(GetValue(QualityCategory.Legendary), finalized: false, ToStringNumberSense.Offset);
			return text + " ~ " + text2;
		}
	}

	public string ToStringAsFactorRange
	{
		get
		{
			string text = GetValue(QualityCategory.Awful).ToStringByStyle(ToStringStyle.PercentZero);
			string text2 = GetValue(QualityCategory.Legendary).ToStringByStyle(ToStringStyle.PercentZero);
			return text + " ~ " + text2;
		}
	}

	public float GetValue(QualityCategory qc)
	{
		return qc switch
		{
			QualityCategory.Awful => awful, 
			QualityCategory.Poor => poor, 
			QualityCategory.Normal => normal, 
			QualityCategory.Good => good, 
			QualityCategory.Excellent => excellent, 
			QualityCategory.Masterwork => masterwork, 
			QualityCategory.Legendary => legendary, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
