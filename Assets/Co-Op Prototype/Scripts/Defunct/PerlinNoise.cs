using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
	public int width = 256;
	public int height = 256;

	public float offsetX = 100f;
	public float offsetY = 100f;

	public float scale = 20;

	new Renderer renderer;
	void Start()
	{
		renderer = GetComponent<Renderer> ();
		renderer.material.mainTexture = GenerateTexture ();
	}

	Texture2D GenerateTexture()
	{
		//Initialize texture
		Texture2D texture = new Texture2D (width, height);

		//Generate Perlin Noise map for the texture
		for (int x = 0; x < width; x++) 
		{
			for (int y = 0; y < width; y++) 
			{
				Color color = CalculateColor (x, y);
				texture.SetPixel (x, y, color);
			}
		}
		texture.Apply ();//needed when u SetPixel();

		return texture;
	}

	Color CalculateColor(int x, int y)
	{
		//Needed because perlin noise shouldnt have high numbers, but 0~1.
		float xCoord = (float)x / width * scale + offsetX;
		float yCoord = (float)y / height * scale + offsetY;

		float sample = Mathf.PerlinNoise (xCoord, yCoord);//Between 0 and 1. 0=blacc, 1=white
		return new Color (sample, sample, sample);//since between 0 and 1, shades of gray.
	}
}
