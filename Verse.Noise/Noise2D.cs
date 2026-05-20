using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Verse.Noise;

public class Noise2D : IDisposable
{
	public static readonly double South = -90.0;

	public static readonly double North = 90.0;

	public static readonly double West = -180.0;

	public static readonly double East = 180.0;

	public static readonly double AngleMin = -180.0;

	public static readonly double AngleMax = 180.0;

	public static readonly double Left = -1.0;

	public static readonly double Right = 1.0;

	public static readonly double Top = -1.0;

	public static readonly double Bottom = 1.0;

	private int m_width;

	private int m_height;

	private float[,] m_data;

	private int m_ucWidth;

	private int m_ucHeight;

	private int m_ucBorder = 1;

	private float[,] m_ucData;

	private float m_borderValue = float.NaN;

	private ModuleBase m_generator;

	[NonSerialized]
	[XmlIgnore]
	private bool m_disposed;

	public float this[int x, int y, bool isCropped = true]
	{
		get
		{
			if (isCropped)
			{
				if (x < 0 && x >= m_width)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= m_height)
				{
					throw new ArgumentOutOfRangeException("Inavlid y position");
				}
				return m_data[x, y];
			}
			if (x < 0 && x >= m_ucWidth)
			{
				throw new ArgumentOutOfRangeException("Invalid x position");
			}
			if (y < 0 && y >= m_ucHeight)
			{
				throw new ArgumentOutOfRangeException("Inavlid y position");
			}
			return m_ucData[x, y];
		}
		set
		{
			if (isCropped)
			{
				if (x < 0 && x >= m_width)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= m_height)
				{
					throw new ArgumentOutOfRangeException("Invalid y position");
				}
				m_data[x, y] = value;
			}
			else
			{
				if (x < 0 && x >= m_ucWidth)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= m_ucHeight)
				{
					throw new ArgumentOutOfRangeException("Inavlid y position");
				}
				m_ucData[x, y] = value;
			}
		}
	}

	public float Border
	{
		get
		{
			return m_borderValue;
		}
		set
		{
			m_borderValue = value;
		}
	}

	public ModuleBase Generator
	{
		get
		{
			return m_generator;
		}
		set
		{
			m_generator = value;
		}
	}

	public int Height => m_height;

	public int Width => m_width;

	public bool IsDisposed => m_disposed;

	protected Noise2D()
	{
	}

	public Noise2D(int size)
		: this(size, size, null)
	{
	}

	public Noise2D(int size, ModuleBase generator)
		: this(size, size, generator)
	{
	}

	public Noise2D(int width, int height)
		: this(width, height, null)
	{
	}

	public Noise2D(int width, int height, ModuleBase generator)
	{
		m_generator = generator;
		m_width = width;
		m_height = height;
		m_data = new float[width, height];
		m_ucWidth = width + m_ucBorder * 2;
		m_ucHeight = height + m_ucBorder * 2;
		m_ucData = new float[width + m_ucBorder * 2, height + m_ucBorder * 2];
	}

	public float[,] GetNormalizedData(bool isCropped = true, int xCrop = 0, int yCrop = 0)
	{
		return GetData(isCropped, xCrop, yCrop, isNormalized: true);
	}

	public float[,] GetData(bool isCropped = true, int xCrop = 0, int yCrop = 0, bool isNormalized = false)
	{
		float[,] array;
		int num;
		int num2;
		if (isCropped)
		{
			num = m_width;
			num2 = m_height;
			array = m_data;
		}
		else
		{
			num = m_ucWidth;
			num2 = m_ucHeight;
			array = m_ucData;
		}
		num -= xCrop;
		num2 -= yCrop;
		float[,] array2 = new float[num, num2];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num3 = ((!isNormalized) ? array[i, j] : ((array[i, j] + 1f) / 2f));
				array2[i, j] = num3;
			}
		}
		return array2;
	}

	public void Clear()
	{
		Clear(0f);
	}

	public void Clear(float value)
	{
		for (int i = 0; i < m_width; i++)
		{
			for (int j = 0; j < m_height; j++)
			{
				m_data[i, j] = value;
			}
		}
	}

	private double GeneratePlanar(double x, double y)
	{
		return m_generator.GetValue(x, 0.0, y);
	}

	public void GeneratePlanar(double left, double right, double top, double bottom)
	{
		GeneratePlanar(left, right, top, bottom, isSeamless: true);
	}

	public void GeneratePlanar(double left, double right, double top, double bottom, bool isSeamless)
	{
		if (right <= left || bottom <= top)
		{
			throw new ArgumentException("Invalid right/left or bottom/top combination");
		}
		if (m_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = right - left;
		double num2 = bottom - top;
		double num3 = num / ((double)m_width - (double)m_ucBorder);
		double num4 = num2 / ((double)m_height - (double)m_ucBorder);
		double num5 = left;
		double num6 = top;
		float num7 = 0f;
		for (int i = 0; i < m_ucWidth; i++)
		{
			num6 = top;
			for (int j = 0; j < m_ucHeight; j++)
			{
				if (isSeamless)
				{
					num7 = (float)GeneratePlanar(num5, num6);
				}
				else
				{
					double a = GeneratePlanar(num5, num6);
					double b = GeneratePlanar(num5 + num, num6);
					double a2 = GeneratePlanar(num5, num6 + num2);
					double b2 = GeneratePlanar(num5 + num, num6 + num2);
					double position = 1.0 - (num5 - left) / num;
					double position2 = 1.0 - (num6 - top) / num2;
					double a3 = Utils.InterpolateLinear(a, b, position);
					double b3 = Utils.InterpolateLinear(a2, b2, position);
					num7 = (float)Utils.InterpolateLinear(a3, b3, position2);
				}
				m_ucData[i, j] = num7;
				if (i >= m_ucBorder && j >= m_ucBorder && i < m_width + m_ucBorder && j < m_height + m_ucBorder)
				{
					m_data[i - m_ucBorder, j - m_ucBorder] = num7;
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	private double GenerateCylindrical(double angle, double height)
	{
		double x = Math.Cos(angle * (Math.PI / 180.0));
		double z = Math.Sin(angle * (Math.PI / 180.0));
		return m_generator.GetValue(x, height, z);
	}

	public void GenerateCylindrical(double angleMin, double angleMax, double heightMin, double heightMax)
	{
		if (angleMax <= angleMin || heightMax <= heightMin)
		{
			throw new ArgumentException("Invalid angle or height parameters");
		}
		if (m_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = angleMax - angleMin;
		double num2 = heightMax - heightMin;
		double num3 = num / ((double)m_width - (double)m_ucBorder);
		double num4 = num2 / ((double)m_height - (double)m_ucBorder);
		double num5 = angleMin;
		double num6 = heightMin;
		for (int i = 0; i < m_ucWidth; i++)
		{
			num6 = heightMin;
			for (int j = 0; j < m_ucHeight; j++)
			{
				m_ucData[i, j] = (float)GenerateCylindrical(num5, num6);
				if (i >= m_ucBorder && j >= m_ucBorder && i < m_width + m_ucBorder && j < m_height + m_ucBorder)
				{
					m_data[i - m_ucBorder, j - m_ucBorder] = (float)GenerateCylindrical(num5, num6);
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	private double GenerateSpherical(double lat, double lon)
	{
		double num = Math.Cos(Math.PI / 180.0 * lat);
		return m_generator.GetValue(num * Math.Cos(Math.PI / 180.0 * lon), Math.Sin(Math.PI / 180.0 * lat), num * Math.Sin(Math.PI / 180.0 * lon));
	}

	public void GenerateSpherical(double south, double north, double west, double east)
	{
		if (east <= west || north <= south)
		{
			throw new ArgumentException("Invalid east/west or north/south combination");
		}
		if (m_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = east - west;
		double num2 = north - south;
		double num3 = num / ((double)m_width - (double)m_ucBorder);
		double num4 = num2 / ((double)m_height - (double)m_ucBorder);
		double num5 = west;
		double num6 = south;
		for (int i = 0; i < m_ucWidth; i++)
		{
			num6 = south;
			for (int j = 0; j < m_ucHeight; j++)
			{
				m_ucData[i, j] = (float)GenerateSpherical(num6, num5);
				if (i >= m_ucBorder && j >= m_ucBorder && i < m_width + m_ucBorder && j < m_height + m_ucBorder)
				{
					m_data[i - m_ucBorder, j - m_ucBorder] = (float)GenerateSpherical(num6, num5);
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	public Texture2D GetTexture()
	{
		return GetTexture(GradientPresets.Grayscale);
	}

	public Texture2D GetTexture(Gradient gradient)
	{
		Texture2D texture2D = new Texture2D(m_width, m_height);
		texture2D.name = "Noise2DTex";
		Color[] array = new Color[m_width * m_height];
		for (int i = 0; i < m_width; i++)
		{
			for (int j = 0; j < m_height; j++)
			{
				float num = 0f;
				num = ((float.IsNaN(m_borderValue) || (i != 0 && i != m_width - m_ucBorder && j != 0 && j != m_height - m_ucBorder)) ? m_data[i, j] : m_borderValue);
				array[i + j * m_width] = gradient.Evaluate((num + 1f) / 2f);
			}
		}
		texture2D.SetPixels(array);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		return texture2D;
	}

	public Texture2D GetNormalMap(float intensity)
	{
		Texture2D texture2D = new Texture2D(m_width, m_height);
		texture2D.name = "Noise2DTex";
		Color[] array = new Color[m_width * m_height];
		for (int i = 0; i < m_ucWidth; i++)
		{
			for (int j = 0; j < m_ucHeight; j++)
			{
				float num = (m_ucData[Mathf.Max(0, i - m_ucBorder), j] - m_ucData[Mathf.Min(i + m_ucBorder, m_height + m_ucBorder), j]) / 2f;
				float num2 = (m_ucData[i, Mathf.Max(0, j - m_ucBorder)] - m_ucData[i, Mathf.Min(j + m_ucBorder, m_width + m_ucBorder)]) / 2f;
				Vector3 vector = new Vector3(num * intensity, 0f, 1f);
				Vector3 vector2 = new Vector3(0f, num2 * intensity, 1f);
				Vector3 vector3 = vector + vector2;
				vector3.Normalize();
				Vector3 zero = Vector3.zero;
				zero.x = (vector3.x + 1f) / 2f;
				zero.y = (vector3.y + 1f) / 2f;
				zero.z = (vector3.z + 1f) / 2f;
				if (i >= m_ucBorder && j >= m_ucBorder && i < m_width + m_ucBorder && j < m_height + m_ucBorder)
				{
					array[i - m_ucBorder + (j - m_ucBorder) * m_width] = new Color(zero.x, zero.y, zero.z);
				}
			}
		}
		texture2D.SetPixels(array);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		return texture2D;
	}

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = Disposing();
		}
		GC.SuppressFinalize(this);
	}

	protected virtual bool Disposing()
	{
		if (m_data != null)
		{
			m_data = null;
		}
		m_width = 0;
		m_height = 0;
		return true;
	}
}
