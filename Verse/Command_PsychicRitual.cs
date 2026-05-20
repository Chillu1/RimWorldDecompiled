using UnityEngine;

namespace Verse;

public class Command_PsychicRitual : Command_Action
{
	public override Texture2D BGTexture => ContentFinder<Texture2D>.Get("UI/PsychicRituals/PsychicRitualButtonBackground");

	public override Texture2D BGTextureShrunk => ContentFinder<Texture2D>.Get("UI/PsychicRituals/PsychicRitualButtonBackground");
}
