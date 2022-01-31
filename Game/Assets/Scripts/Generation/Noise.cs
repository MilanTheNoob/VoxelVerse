using UnityEngine;
using System.Collections;

public static class Noise
{
	/*
	public static float[,] GenerateHeightMap(NoiseSettings[] settings, Vector2 sampleCentre)
    {
		float[,] heightMap = new float[18, 18];

		for (int i = 0; i < settings.Length; i++)
        {
			float[,] noiseMap = GenerateNoiseMap(settings[i], sampleCentre);

			for (int x = 0; x < 18; x++)
            {
				for (int y = 0; y < 18; y++)
                {
					heightMap[x, y] += noiseMap[x, y];
                }
            }
        }

		return heightMap;
    }
	*/

	public static HeightmapData GenerateHeightMap(BiomeNoiseClass[] settings, NoiseSettings biomeMap, Vector2 sampleCentre, int seed)
	{
		float[,] heightMap = new float[18, 18];
        sampleCentre.x -= 14;sampleCentre.y -= 14;

        for (int x = 0; x < 18; x++)
        {
            for (int y = 0; y < 18; y++)
            {
                float l = (SingleNoise(biomeMap, seed, sampleCentre.x + x, sampleCentre.y + y) + 1) / 2f;
                float noise_height = 0;

                int biomes_count = 0;
                int id = 0;

                for (int i = 0; i < settings.Length; i++)
                {
                    if (use_biome(settings.Length, l, i))
                    {
                        biomes_count++;
                        id = i;
                    }
                }

                for (int i = 0; i < settings.Length; i++)
                {
                    if (use_biome(settings.Length, l, i))
                    {
                        float temp_noise = 0;

                        for (int j = 0; j < settings[i].Noise.Length; j++)
                        {
                            temp_noise = temp_noise + SingleNoise(settings[i].Noise[j], seed + j, sampleCentre.x + x, sampleCentre.y + y);
                        }
                        noise_height = noise_height + (temp_noise * biome_weight(settings.Length, l, i));
                    }
                }

                heightMap[x, y] = noise_height;
            }
        }

        return new HeightmapData( heightMap);
	}

    static float biome_weight(float biomes_count, float rand_num, float biome)
    {
        return Mathf.Clamp(-Mathf.Abs(biomes_count * rand_num - biome) + 1, (float)0, (float)1);
    }

    static bool use_biome(int biomes_count, float rand_num, int biome)
    {
        if ((biome - (float)1) / biomes_count <= rand_num && rand_num <= (biome + (float)1) / biomes_count) { return true; }
        return false;
    }


    public static float[,] GenerateNoiseMap(NoiseSettings settings, Vector2 sampleCentre)
	{
		float[,] noiseMap = new float[18, 18];

		System.Random prng = new System.Random(0);
		Vector2[] octaveOffsets = new Vector2[settings.octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < settings.octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + sampleCentre.x;
			float offsetY = prng.Next(-100000, 100000) - sampleCentre.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= settings.persistance;
		}


		for (int x = 0; x < 18; x++)
		{
			for (int y = 0; y < 18; y++)
			{

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < settings.octaves; i++)
				{
					float sampleX = (x - 9 + octaveOffsets[i].x) / settings.scale * frequency;
					float sampleY = (y - 9 + octaveOffsets[i].y) / settings.scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= settings.persistance;
					frequency *= settings.lacunarity;
				}

				noiseMap[x, y] = noiseHeight;

				float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
				noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue) * settings.height;
			}
		}

		return noiseMap;
	}

    static float[] gradients = new float[]
{
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.130526192220052f, 0.99144486137381f, 0.38268343236509f, 0.923879532511287f, 0.608761429008721f, 0.793353340291235f, 0.793353340291235f, 0.608761429008721f,
    0.923879532511287f, 0.38268343236509f, 0.99144486137381f, 0.130526192220051f, 0.99144486137381f, -0.130526192220051f, 0.923879532511287f, -0.38268343236509f,
    0.793353340291235f, -0.60876142900872f, 0.608761429008721f, -0.793353340291235f, 0.38268343236509f, -0.923879532511287f, 0.130526192220052f, -0.99144486137381f,
    -0.130526192220052f, -0.99144486137381f, -0.38268343236509f, -0.923879532511287f, -0.608761429008721f, -0.793353340291235f, -0.793353340291235f, -0.608761429008721f,
    -0.923879532511287f, -0.38268343236509f, -0.99144486137381f, -0.130526192220052f, -0.99144486137381f, 0.130526192220051f, -0.923879532511287f, 0.38268343236509f,
    -0.793353340291235f, 0.608761429008721f, -0.608761429008721f, 0.793353340291235f, -0.38268343236509f, 0.923879532511287f, -0.130526192220052f, 0.99144486137381f,
    0.38268343236509f, 0.923879532511287f, 0.923879532511287f, 0.38268343236509f, 0.923879532511287f, -0.38268343236509f, 0.38268343236509f, -0.923879532511287f,
    -0.38268343236509f, -0.923879532511287f, -0.923879532511287f, -0.38268343236509f, -0.923879532511287f, 0.38268343236509f, -0.38268343236509f, 0.923879532511287f,
};

    const int prime_x = 501125321;
    const int prime_y = 1136930381;

    static float grad_coord(int seed, int xPrimed, int yPrimed, float xd, float yd)
    {
        int hash = seed ^ xPrimed ^ yPrimed;
        hash *= 0x27d4eb2d;

        hash ^= hash >> 15;
        hash &= 127 << 1;

        float xg = gradients[hash];
        float yg = gradients[hash | 1];

        return xd * xg + yd * yg;
    }

    static float single_simplex(int seed, float x, float y)
    {
        const float SQRT3 = 1.7320508075688772935274463415059f;
        const float G2 = (3 - SQRT3) / 6;

        const float F2 = 0.5f * (SQRT3 - 1);
        float s = (x + y) * F2;
        x += s; y += s;

        int i = x >= 0 ? (int)x : (int)x - 1;
        int j = y >= 0 ? (int)y : (int)y - 1;
        float xi = (float)(x - i);
        float yi = (float)(y - j);

        float t = (xi + yi) * G2;
        float x0 = (float)(xi - t);
        float y0 = (float)(yi - t);

        i *= prime_x;
        j *= prime_y;

        float n0, n1, n2;

        float a = 0.5f - x0 * x0 - y0 * y0;
        if (a <= 0) n0 = 0;
        else
        {
            n0 = (a * a) * (a * a) * grad_coord(seed, i, j, x0, y0);
        }

        float c = (float)(2 * (1 - 2 * G2) * (1 / G2 - 2)) * t + ((float)(-2 * (1 - 2 * G2) * (1 - 2 * G2)) + a);
        if (c <= 0) n2 = 0;
        else
        {
            float x2 = x0 + (2 * (float)G2 - 1);
            float y2 = y0 + (2 * (float)G2 - 1);
            n2 = (c * c) * (c * c) * grad_coord(seed, i + prime_x, j + prime_y, x2, y2);
        }

        if (y0 > x0)
        {
            float x1 = x0 + (float)G2;
            float y1 = y0 + ((float)G2 - 1);
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0) n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * grad_coord(seed, i, j + prime_y, x1, y1);
            }
        }
        else
        {
            float x1 = x0 + ((float)G2 - 1);
            float y1 = y0 + (float)G2;
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0) n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * grad_coord(seed, i + prime_x, j, x1, y1);
            }
        }

        return (1 + (n0 + n1 + n2) * 99.83685446303647f) / 2;
    }

    public static float SingleNoise(NoiseSettings data, int seed, float x, float y)
    {
        float noise_height = 0;

        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < data.octaves; i++)
        {
            noise_height = noise_height + single_simplex(seed * (i + 1), x / data.scale * frequency, y / data.scale * frequency) * amplitude;

            amplitude = amplitude * data.persistance;
            frequency = frequency * data.lacunarity;
        }

        return (noise_height / data.octaves) * data.height;
    }
}

[System.Serializable]
public class NoiseSettings
{
	public float scale = 50;
	public float height = 50;

	[Space]

	public int octaves = 6;
	[Range(0, 1)]
	public float persistance = .6f;
	public float lacunarity = 2;

	public void ValidateValues()
	{
		scale = Mathf.Max(scale, 0.01f);
		octaves = Mathf.Max(octaves, 1);
		lacunarity = Mathf.Max(lacunarity, 1);
		persistance = Mathf.Clamp01(persistance);
	}
}

public struct HeightmapData
{
    public float[,] Heightmap;
    public HeightmapData(float[,] heightmap) { Heightmap = heightmap; }
}