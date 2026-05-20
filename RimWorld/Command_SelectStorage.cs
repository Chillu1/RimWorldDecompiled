using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Command_SelectStorage : Command_Action
	{
		public override Texture2D BGTexture => TexCommand.SelectShelf;

		public override Texture2D BGTextureShrunk => TexCommand.SelectShelf;
	}
}
