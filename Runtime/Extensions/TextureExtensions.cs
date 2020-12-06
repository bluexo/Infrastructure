using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Origine
{
    public static class TextureExtensions
    {
        public static void SaveToPNG(this Texture inputTex, string path)
        {
            var temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp);
            var tex2D = temp.ConvertTo2D();
            RenderTexture.ReleaseTemporary(temp);
            File.WriteAllBytes(path, tex2D.EncodeToPNG());
        }

        public static Texture2D ConvertTo2D(this RenderTexture rt)
        {
            var currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            RenderTexture.active = currentActiveRT;
            return tex;
        }
    }

}