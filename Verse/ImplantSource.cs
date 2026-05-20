using RimWorld;

namespace Verse;

public class ImplantSource : IExposable
{
	private Pawn sourcePawn;

	private string descKey;

	private string descResolved;

	private int biosignature;

	private string cachedName;

	public bool WasPawn => sourcePawn != null;

	public Pawn Pawn => sourcePawn;

	public int Biosignature => biosignature;

	public string BiosignatureName => cachedName ?? (cachedName = AnomalyUtility.GetBiosignatureName(biosignature));

	public ImplantSource()
	{
		biosignature = Rand.Int;
	}

	public ImplantSource(Pawn pawn, string descKey = null, string descResolved = null)
	{
		sourcePawn = pawn;
		this.descKey = descKey;
		this.descResolved = descResolved;
		if (pawn != null && pawn.health.hediffSet.TryGetHediff<Hediff_MetalhorrorImplant>(out var hediff))
		{
			biosignature = hediff.Biosignature;
		}
		else
		{
			biosignature = Rand.Int;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref descKey, "descKeyDesc");
		Scribe_Values.Look(ref descResolved, "descResolved");
		Scribe_Values.Look(ref biosignature, "biosignature", 0);
		Scribe_References.Look(ref sourcePawn, "sourcePawn", saveDestroyedThings: true);
	}

	public void DebugSetBiosignature(int newBiosignature)
	{
		biosignature = newBiosignature;
		cachedName = null;
	}

	public string GetSourceDesc()
	{
		if (!string.IsNullOrEmpty(descResolved))
		{
			return descResolved;
		}
		if (string.IsNullOrEmpty(descKey))
		{
			return "ImplantSourceUnknown".Translate();
		}
		if (sourcePawn != null)
		{
			return descKey.Translate(sourcePawn.Named("SOURCE"));
		}
		return descKey.Translate();
	}
}
