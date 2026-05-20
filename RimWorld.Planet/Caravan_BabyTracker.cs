using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class Caravan_BabyTracker
{
	private readonly Caravan caravan;

	private readonly List<Pawn> babies = new List<Pawn>();

	private readonly List<Ideo> ideosForExposure = new List<Ideo>();

	private const int IdeoExposureIntervalTicks = 30;

	private float IdeoExposurePerCaravanIdeoMember => 0.003f / (float)ideosForExposure.Count;

	public Caravan_BabyTracker(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public void TickInterval(int delta)
	{
		if (!ModsConfig.IdeologyActive || !caravan.IsHashIntervalTick(30, delta))
		{
			return;
		}
		bool flag = false;
		foreach (Pawn baby in babies)
		{
			if (baby.Dead || !baby.DevelopmentalStage.Baby())
			{
				flag = true;
			}
			else
			{
				if (baby.ideo == null)
				{
					continue;
				}
				float ideoExposurePerCaravanIdeoMember = IdeoExposurePerCaravanIdeoMember;
				foreach (Ideo item in ideosForExposure)
				{
					baby.ideo.IncreaseIdeoExposureIfBaby(item, ideoExposurePerCaravanIdeoMember);
				}
			}
		}
		if (flag)
		{
			Recache();
		}
	}

	public void Recache()
	{
		RebuildBabyList();
		RebuildPawnIdeos();
	}

	private void RebuildBabyList()
	{
		babies.Clear();
		foreach (Pawn item in caravan.PawnsListForReading)
		{
			if (item.DevelopmentalStage.Baby())
			{
				babies.Add(item);
			}
		}
	}

	private void RebuildPawnIdeos()
	{
		if (!ModsConfig.IdeologyActive)
		{
			return;
		}
		ideosForExposure.Clear();
		foreach (Pawn item in caravan.PawnsListForReading)
		{
			if (item.Ideo != null)
			{
				ideosForExposure.Add(item.Ideo);
			}
		}
	}
}
