using Verse;

namespace RimWorld;

public class CompProperties_Transporter : CompProperties
{
	public float massCapacity = 150f;

	public float restEffectiveness;

	public bool max1PerGroup;

	public bool canChangeAssignedThingsAfterStarting;

	public bool showOverallStats = true;

	public SoundDef pawnLoadedSound;

	public SoundDef pawnExitSound;

	public bool shouldTickContents = true;

	public bool showMassInInspectString;

	public CompProperties_Transporter()
	{
		compClass = typeof(CompTransporter);
	}
}
