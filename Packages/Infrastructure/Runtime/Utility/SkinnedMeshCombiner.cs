using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Wars
{
    public static class SkinnedMeshCombiner
    {
        private const int COMBINE_TEXTURE_MAX = 512;
        private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";

        static Shader msShader;
        //the list of meshes
        readonly static List<CombineInstance> msCombineInstances = new List<CombineInstance>(8);
        //the list of materials
        readonly static List<Material> msMaterials = new List<Material>(8); 
        //the list of bones
        readonly static List<Transform> msBones = new List<Transform>(64);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="skeletonBones"></param>
        /// <param name="meshes"></param>
        /// <param name="combine"></param>
        public static void CombineObject(GameObject skeleton,
            Transform[] skeletonBones,
            IList<SkinnedMeshRenderer> meshes,
            bool combine = false)
        {
            msMaterials.Clear();
            msCombineInstances.Clear();
            msBones.Clear();

            // Collect information from meshes
            for (int i = 0; i < meshes.Count; i++)
            {
                SkinnedMeshRenderer smr = meshes[i];
                msMaterials.AddRange(smr.materials); // Collect materials

                // Collect meshes
                for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    CombineInstance tmpCI = new CombineInstance();
                    tmpCI.mesh = smr.sharedMesh;
                    tmpCI.subMeshIndex = sub;
                    msCombineInstances.Add(tmpCI);
                }

                // Collect bones
                for (int j = 0; j < smr.bones.Length; j++)
                    for (int tBase = 0; tBase < skeletonBones.Length; tBase++)
                        if (smr.bones[j].name.Equals(skeletonBones[tBase].name))
                        {
                            msBones.Add(skeletonBones[tBase]);
                            break;
                        }
            }

            // Below informations only are used for merge materilas(bool combine = true)
            List<Vector2[]> tmpOldUV = null;
            Material tmpNewMaterial = null;

            // merge materials
            if (combine)
            {
                if (null == msShader)
                    msShader = Shader.Find("Mobile/Diffuse");

                tmpNewMaterial = new Material(msShader);
                tmpOldUV = new List<Vector2[]>();
                // merge the texture
                List<Texture2D> Textures = new List<Texture2D>();
                for (int i = 0; i < msMaterials.Count; i++)
                    Textures.Add(msMaterials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);

                Texture2D newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, true);
                Rect[] uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0);
                tmpNewMaterial.mainTexture = newDiffuseTex;

                // reset uv
                Vector2[] uva, uvb;
                for (int j = 0; j < msCombineInstances.Count; j++)
                {
                    uva = msCombineInstances[j].mesh.uv;
                    uvb = new Vector2[uva.Length];
                    for (int k = 0; k < uva.Length; k++)
                        uvb[k] = new Vector2((uva[k].x * uvs[j].width) + uvs[j].x, (uva[k].y * uvs[j].height) + uvs[j].y);
                    tmpOldUV.Add(msCombineInstances[j].mesh.uv);
                    msCombineInstances[j].mesh.uv = uvb;
                }
            }

            // Create a new SkinnedMeshRenderer
            var tmpSkin = skeleton.GetComponent<SkinnedMeshRenderer>();

            if (tmpSkin == null)
                tmpSkin = skeleton.AddComponent<SkinnedMeshRenderer>();

            tmpSkin.sharedMesh = new Mesh();
            tmpSkin.sharedMesh.CombineMeshes(msCombineInstances.ToArray(), combine, false);// Combine meshes
            tmpSkin.bones = msBones.ToArray();// Use new bones

            if (combine)
            {
                tmpSkin.material = tmpNewMaterial;
                for (int i = 0; i < msCombineInstances.Count; i++)
                    msCombineInstances[i].mesh.uv = tmpOldUV[i];
            }
            else
            {
                tmpSkin.materials = msMaterials.ToArray();
            }
        }
    }
}
