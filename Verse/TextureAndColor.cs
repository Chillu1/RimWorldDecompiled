using UnityEngine;

namespace Verse
{
	public struct TextureAndColor
	{
		private Texture2D texture;

		private Color color;

		public bool HasValue => texture != null;

		public Texture2D Texture => texture;

		public Color Color => color;

		public static TextureAndColor None => new TextureAndColor(null, Color.white);

		public TextureAndColor(Texture2D texture, Color color)
		{
			this.texture = texture;
			this.color = color;
		}

		public static implicit operator TextureAndColor(Texture2D texture)
		{
			return new TextureAndColor(texture, Color.white);
		}
	}
}
