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
        //Texture2D texture = CombineGPU(tex01, tex02);
        Texture2D texture = new Texture2D(1024, 1024, tex01.format, true);
        //Texture2D texture = CombineLawData(tex01, tex02);
        bool bSucces = CombineToTex(texture, 0, 0, tex01);

        bSucces = CombineToTex(texture, tex01.width, 0, tex02);
        texture.Apply(false, true);
        mat.SetTexture("_MainTex", texture);
        GetComponent<UnityEngine.UI.RawImage>().texture = texture;
        if (!bSucces)
            Debug.LogError("combine error!");
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
    Texture2D CombineCpu(Texture2D tex, Texture2D tex1)
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

        // tint each mip level
        for (int mip = 0; mip < nMipLevel; ++mip)
        {
            Color[] cols = combinedTex.GetPixels(mip);
           
            combinedTex.SetPixels(cols, mip);
        }

        combinedTex.Apply(false);
        return combinedTex;
    }

    bool CombineToTex(Texture2D combinedTex, int destX, int destY, Texture2D tex)
    {
        int width = combinedTex.width;
        int height = combinedTex.height;
        var blcokBytes = 0;
        int blockSize = 1;
        byte[] data = null;
        switch (tex.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.ETC_RGB4:
            case TextureFormat.ETC2_RGB:
                blcokBytes = 8;
                //data = new byte[length / 2 * length];
                break;
            case TextureFormat.DXT5:
            case TextureFormat.ETC2_RGBA8:
            case TextureFormat.ASTC_4x4:
                blcokBytes = 16;
                blockSize = 4;
                //data = new byte[length * length];
                break;
            default:
                UnityEngine.Debug.Log("Not supported.");
                return false;
        }

        //var combinedTex = new Texture2D(width, height, tex.format, true);

        data = combinedTex.GetRawTextureData();

        int nMipMap = 2;
        for(int i = 0; i < nMipMap; ++i)
        {
            CombineBlocks(tex.GetRawTextureData(), data, destX, destY, tex.width, tex.height, blockSize, blcokBytes, width, height, i);
        }
       


        combinedTex.LoadRawTextureData(data);
        //combinedTex.Apply(false, true);

        return true;
    }

    Texture2D CombineLawData(Texture2D tex, Texture2D tex1)
    {
        int width = 1024;
        int height = 1024;
        var blcokBytes = 0;
        byte[] data = null;
        switch (tex.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.ETC_RGB4:
            case TextureFormat.ETC2_RGB:
                blcokBytes = 8;
                //data = new byte[length / 2 * length];
                break;
            case TextureFormat.DXT5:
            case TextureFormat.ETC2_RGBA8:
            case TextureFormat.ASTC_4x4:
                blcokBytes = 16;
                //data = new byte[length * length];
                break;
            default:
                UnityEngine.Debug.Log("Not supported.");
                return null;
        }

        var combinedTex = new Texture2D(width, height, tex.format, true);

        data = combinedTex.GetRawTextureData();
        //������½� 256
        CombineBlocks(tex.GetRawTextureData(), data, 0, 0, tex.width, tex.height, 4, blcokBytes, width, height);
        //������Ͻ� 256 
        CombineBlocks(tex.GetRawTextureData(), data, 0, tex.width, tex.width, tex.height, 4, blcokBytes, width, height);
        //������½� 256 
        CombineBlocks(tex.GetRawTextureData(), data, tex.width, 0, tex.width, tex.height, 4, blcokBytes, width, height);

        //������Ͻ�����
        //���½� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width, tex.width, tex1.width, tex1.height, 4, blcokBytes, width, height);
        //���Ͻ� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width, tex.width + tex1.width, tex1.width, tex1.height, 4, blcokBytes, width, height);
        //���½� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width + tex1.width, tex.width, tex1.width, tex1.height, 4, blcokBytes, width, height);
        //���Ͻ� 128
        CombineBlocks(tex1.GetRawTextureData(), data, tex.width + tex1.width, tex.width + tex1.width, tex1.width, tex1.height, 4, blcokBytes, width, height);



        combinedTex.LoadRawTextureData(data);
        combinedTex.Apply(false, true);

        return combinedTex;
    }

    void CombineBlocks(byte[] src, byte[] dst, int dstx, int dsty, int width, int height, int block, int blcokBytes, int destTexWidth, int destTexHeight, int mipLevel = 0)
    {
        int srcMipLevel0Size = (width / block) * (height / block) * blcokBytes;
        int destMipLevel0Size = (destTexWidth / block) * (destTexHeight / block) * blcokBytes;

        width = width >> mipLevel;
        height = height >> mipLevel;
        destTexWidth = destTexWidth >> mipLevel;
        destTexHeight = destTexHeight >> mipLevel;

        var dstbx = dstx / block;
        var dstby = dsty / block;

       

        int srcMipOffset = 0;
        int destMipOffset = 0;
        for (int i = 0; i < mipLevel; ++i)
        {
            srcMipOffset += srcMipLevel0Size >> (i * 4);

            destMipOffset += destMipLevel0Size >> (i * 4);
        }
       
        for (int i = 0; i < width / block; i++)
        {
            int dstindex = (dstby + (dstbx + i) * (destTexHeight / block)) * blcokBytes;
            int byteCount = (height / block) * blcokBytes;
            int srcindex = i * byteCount;
            Buffer.BlockCopy(src, srcindex + srcMipOffset, dst, dstindex + destMipOffset, byteCount);
        }
    }
}
