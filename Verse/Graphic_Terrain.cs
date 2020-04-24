namespace Verse
{
	public class Graphic_Terrain : Graphic_Single
	{
		public override void Init(GraphicRequest req)
		{
			base.Init(req);
		}

		public override string ToString()
		{
			return "Terrain(path=" + path + ", shader=" + base.Shader + ", color=" + color + ")";
		}
	}
}
