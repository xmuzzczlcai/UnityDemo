using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTexture : MonoBehaviour
{
    public Material mat;
    public Texture2D combineTex;
    public Texture2D tex01;
    public Texture2D tex02;
    //public Texture2D tex03;
    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<UnityEngine.UI.RawImage>().texture =  Combine(tex01, tex02);
        Texture2D texture = CombineGPU(tex01, tex02);
        mat.SetTexture("_MainTex", texture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Texture2D CombineGPU(Texture2D tex, Texture2D tex1)
    {
        int length = 1024;
        int blockSize = 0;
        switch (tex.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.ETC_RGB4:
            case TextureFormat.DXT5:
            case TextureFormat.ETC2_RGBA8:
            case TextureFormat.ASTC_4x4:
                blockSize = 4;
                break;
            default:
                UnityEngine.Debug.Log("Not supported.");
                return null;
        }
        var combinedTex = new Texture2D(length, length, tex.format, true);
        int nMipLevel = tex.mipmapCount;
        int nMipLevel01 = tex1.mipmapCount;
        int nMipLeveCombine = combinedTex.mipmapCount;

        int copyMipCount = nMipLevel01;
        int srcElement = 0;
        for(int i = 0; i < copyMipCount; ++i)
        {
            int tex0Width = tex.width >> i;
            int tex0Height = tex.height >> i;

            int tex1Width = tex1.width >> i;
            int tex1Height = tex1.height >> i;
            //左上角
            Graphics.CopyTexture(tex, srcElement, i, 0, 0, tex0Width, tex0Height, combinedTex, srcElement, i, 0, 0);

            Graphics.CopyTexture(tex1, srcElement, i, 0, 0, tex1Width, tex1Height, combinedTex, srcElement, i, tex0Width, 0);
        }

        return combinedTex;

    }
    Texture2D Combine(Texture2D tex, Texture2D tex1)
    {
        int length = 1024;
        var blcokBytes = 0;
        byte[] data = null;
        switch (tex.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.ETC_RGB4:
            case TextureFormat.ETC2_RGB:
                blcokBytes = 8;
                data = new byte[length / 2 * length];
                break;
            case TextureFormat.DXT5:
            case TextureFormat.ETC2_RGBA8:
            case TextureFormat.ASTC_4x4:
                blcokBytes = 16;
                data = new byte[length * length];
                break;
            default:
                UnityEngine.Debug.Log("Not supported.");
                return null;
        }
        //������½� 256
        CombineBlocks(tex.GetRawTextureData(), data, 0, 0, tex.width, 4, blcokBytes, length);
        //������Ͻ� 256 
        CombineBlocks(tex.GetRawTextureData(), data, 0, tex.width, tex.width, 4, blcokBytes, length);
        //������½� 256 
        CombineBlocks(tex.GetRawTextureData(), data, tex.width, 0, tex.width, 4, blcokBytes, length);

        //������Ͻ�����
        //���½� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width, tex.width, tex1.width, 4, blcokBytes, length);
        //���Ͻ� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width, tex.width + tex1.width, tex1.width, 4, blcokBytes, length);
        //���½� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width + tex1.width, tex.width, tex1.width, 4, blcokBytes, length);
        //���Ͻ� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width + tex1.width, tex.width + tex1.width, tex1.width, 4, blcokBytes, length);


        var combinedTex = new Texture2D(length, length, tex.format, true);
        combinedTex.LoadRawTextureData(data);
        combinedTex.Apply(false, true);

        return combinedTex;
    }

    void CombineBlocks(byte[] src, byte[] dst, int dstx, int dsty, int width, int block, int bytes, int length)
    {
        var dstbx = dstx / block;
        var dstby = dsty / block;

        for (int i = 0; i < width / block; i++)
        {
            int dstindex = (dstbx + (dstby + i) * (length / block)) * bytes;
            int srcindex = i * (width / block) * bytes;
            Buffer.BlockCopy(src, srcindex, dst, dstindex, width / block * bytes);
        }
    }
}
