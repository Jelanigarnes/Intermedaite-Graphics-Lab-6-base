using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NoiseTexture : MonoBehaviour
{
    public Texture2D noise;
    public Material perlinMaterial;
    public int width = 512;
    public int height = 512;
    public float scale = 1.0f;
    public int _nameCounter = 0;
    public ComputeShader perlinCompute;
    public Color textureColor;

    private void SaveTexturesToJpg(Texture2D textureToSave)
    {
        byte[] bytes = textureToSave.EncodeToJPG();
        string filepath = "./Assets/JPG_" + _nameCounter + ".jpg";
        _nameCounter++;
        File.WriteAllBytes(filepath, bytes);
    }

    [ContextMenu("Generate Texture")]
    private void GenerateTexture()
    {
        noise = new Texture2D(width, height, TextureFormat.RGBA32, true);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float xOrg = 0;
                float yOrg = 0;

                float xCoord = xOrg + i / (float)width * scale;
                float yCoord = yOrg + j / (float)height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                noise.SetPixel(i, j, new Color(sample, sample, sample));
            }
        }
        noise.Apply();

        SaveTexturesToJpg(noise);
    }
    [ContextMenu("Generate GPU texture")]
    private void GenerateTextureGPU()
    {
        noise = new Texture2D(width, height, TextureFormat.RGBA32, true);

        int kernelHandle = perlinCompute.FindKernel("CSMain");

        RenderTexture tempTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        tempTexture.enableRandomWrite = true;
        tempTexture.Create();

        perlinCompute.SetTexture(kernelHandle, "resultBuffer", tempTexture);
        //Execute the Compute Shader
        perlinCompute.Dispatch(kernelHandle, width, height, 1);

        //Convert the render texture to a texture 2d
        Texture2D texture2d = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture.active = tempTexture;
        texture2d.ReadPixels(new Rect(0, 0, tempTexture.width, tempTexture.height), 0, 0);
        texture2d.Apply();

        SaveTexturesToJpg(texture2d);

        float[] tempColor = new float[4];
        for (int i = 0; i < 4; i++)
        {
            tempColor[i] = textureColor[i];
        }

        perlinCompute.SetFloats("color", tempColor);
    }
}
