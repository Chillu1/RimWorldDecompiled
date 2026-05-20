using System.Linq;
using Verse;

namespace RimWorld;

public class Need_Authority : Need
{
	public const float LevelGainPerDayOfReigning = 2f;

	public const float LevelGainPerDayOfGivingSpeech = 3f;

	private readonly SimpleCurve FallFactorCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0f),
		new CurvePoint(3f, 0.5f),
		new CurvePoint(5f, 1f)
	};

	public override int GUIChangeArrow
	{
		get
		{
			if (IsFrozen)
			{
				return 0;
			}
			if (IsCurrentlyReigning || IsCurrentlyGivingSpeech)
			{
				return 1;
			}
			return -1;
		}
	}

	public AuthorityCategory CurCategory
	{
		get
		{
			float curLevel = CurLevel;
			if (curLevel < 0.01f)
			{
				return AuthorityCategory.Gone;
			}
			if (curLevel < 0.15f)
			{
				return AuthorityCategory.Weak;
			}
			if (curLevel < 0.3f)
			{
				return AuthorityCategory.Uncertain;
			}
			if (curLevel > 0.7f && curLevel < 0.85f)
			{
				return AuthorityCategory.Strong;
			}
			if (curLevel >= 0.85f)
			{
				return AuthorityCategory.Total;
			}
			return AuthorityCategory.Normal;
		}
	}

	public bool IsActive
	{
		get
		{
			if (pawn.royalty == null || !pawn.Spawned)
			{
				return false;
			}
			if (pawn.Map == null || !pawn.Map.IsPlayerHome)
			{
				return false;
			}
			if (!pawn.royalty.CanRequireThroneroom())
			{
				return false;
			}
			return true;
		}
	}

	protected override bool IsFrozen
	{
		get
		{
			if (pawn.Map != null && pawn.Map.IsPlayerHome)
			{
				return FallPerDay <= 0f;
			}
			return true;
		}
	}

	public float FallPerDay
	{
		get
		{
			if (pawn.royalty == null || !pawn.Spawned)
			{
				return 0f;
			}
			if (pawn.Map == null || !pawn.Map.IsPlayerHome)
			{
				return 0f;
			}
			float num = 0f;
			foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
			{
				_ = item;
			}
			int num2 = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Count();
			return num * FallFactorCurve.Evaluate(num2);
		}
	}

	public override bool ShowOnNeedList => IsActive;

	public bool IsCurrentlyReigning => pawn.CurJobDef == JobDefOf.Reign;

	public bool IsCurrentlyGivingSpeech => pawn.CurJobDef == JobDefOf.GiveSpeech;

	public Need_Authority(Pawn pawn)
		: base(pawn)
	{
	}

	public override void NeedInterval()
	{
		float num = 400f;
		float num2 = FallPerDay / num;
		if (IsFrozen)
		{
			CurLevel = 1f;
		}
		else if (pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Count <= 1)
		{
			SetInitialLevel();
		}
		else if (IsCurrentlyReigning)
		{
			CurLevel += 2f / num;
		}
		else if (IsCurrentlyGivingSpeech)
		{
			CurLevel += 3f / num;
		}
		else
		{
			CurLevel -= num2;
		}
	}
}
