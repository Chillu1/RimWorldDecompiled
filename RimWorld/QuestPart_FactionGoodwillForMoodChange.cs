using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_FactionGoodwillForMoodChange : QuestPartActivable
{
	public string inSignal;

	public string outSignalSuccess;

	public string outSignalFailed;

	public List<Pawn> pawns = new List<Pawn>();

	private int currentInterval = 2500;

	private List<float> movingAverage = new List<float>();

	private float cachedMovingAverage;

	private static readonly SimpleCurve GoodwillFromAverageMoodCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 20f)
	};

	public const int Interval = 2500;

	public const int Range = 120000;

	public override string ExpiryInfoPart => "QuestAveragePawnMood".Translate(120000.ToStringTicksToPeriodVerbose(), cachedMovingAverage.ToStringPercent());

	public override string ExpiryInfoPartTip => "QuestAveragePawnMoodTargets".Translate(pawns.Select((Pawn p) => p.LabelShort).ToCommaList(useAnd: true), 120000.ToStringTicksToPeriodVerbose());

	private float AveragePawnMoodPercent
	{
		get
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].needs != null && pawns[i].needs.mood != null)
				{
					num += pawns[i].needs.mood.CurLevelPercentage;
					num2++;
				}
			}
			if (num2 == 0)
			{
				return 0f;
			}
			return num / (float)num2;
		}
	}

	private float MovingAveragePawnMoodPercent
	{
		get
		{
			if (movingAverage.Count == 0)
			{
				return AveragePawnMoodPercent;
			}
			float num = 0f;
			for (int i = 0; i < movingAverage.Count; i++)
			{
				num += movingAverage[i];
			}
			return num / (float)movingAverage.Count;
		}
	}

	public int SampleSize => Mathf.FloorToInt(48f);

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		currentInterval++;
		if (currentInterval >= 2500)
		{
			currentInterval = 0;
			while (movingAverage.Count >= SampleSize)
			{
				movingAverage.RemoveLast();
			}
			movingAverage.Insert(0, AveragePawnMoodPercent);
			cachedMovingAverage = MovingAveragePawnMoodPercent;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!inSignal.NullOrEmpty() && signal.tag == inSignal)
		{
			float movingAveragePawnMoodPercent = MovingAveragePawnMoodPercent;
			int num = Mathf.RoundToInt(GoodwillFromAverageMoodCurve.Evaluate(movingAveragePawnMoodPercent));
			SignalArgs args = new SignalArgs(num.Named("GOODWILL"), movingAveragePawnMoodPercent.ToStringPercent().Named("AVERAGEMOOD"));
			if (num > 0)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalSuccess, args));
			}
			else
			{
				Find.SignalManager.SendSignal(new Signal(outSignalFailed, args));
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref outSignalSuccess, "outSignalSuccess");
		Scribe_Values.Look(ref outSignalFailed, "outSignalFailed");
		Scribe_Values.Look(ref currentInterval, "currentInterval", 0);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Collections.Look(ref movingAverage, "movingAverage", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
			if (movingAverage == null)
			{
				movingAverage = new List<float>();
			}
			cachedMovingAverage = MovingAveragePawnMoodPercent;
		}
	}
}
