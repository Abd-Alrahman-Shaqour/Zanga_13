using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MystifyFX {

    public enum IncludeType {
        OnlyThisObject,
        IncludeChildren,
        Custom,
        ByLayer
    }

    public enum MeshType {
        Existing,
        Quad,
        Disc,
        Sphere,
        Hexasphere,
        Cylinder
    }

    [ExecuteAlways]
    [HelpURL("https://kronnect.com/guides")]
    public class MystifyEffect : MonoBehaviour {

        public IncludeType includeType = IncludeType.OnlyThisObject;
        public MeshType meshType = MeshType.Existing;
        public LayerMask includeLayerMask;
        public MystifyEffectProfile profile;

        public readonly static List<MystifyEffect> instances = new List<MystifyEffect>();

        [NonSerialized]
        public readonly List<HitRequest> hitRequests = new List<HitRequest>();

        [SerializeField]
        public List<Renderer> customRenderers;

        [NonSerialized]
        public List<Renderer> renderers;

        [NonSerialized]
        public Material mat;

        [NonSerialized]
        public bool writesToDepth;

        [NonSerialized]
        public bool needsUpdateMaterial;

        static readonly List<string> keywords = new List<string>();
        static string[] keywordsArray;

        void OnEnable () {
            if (!instances.Contains(this)) {
                instances.Add(this);
            }
            needsUpdateMaterial = true;
        }

        void OnDisable () {
            if (instances.Contains(this)) {
                instances.Remove(this);
            }
        }


        void OnDestroy () {
            if (mat != null) {
                DestroyImmediate(mat);
            }
        }

        void OnValidate () {
            Refresh();
        }

        public void Refresh () {
            renderers = null;
            if (profile != null) {
                profile.ValidateSettings();
            }
            needsUpdateMaterial = true;
        }

        public virtual void GetRenderers () {
            if (renderers != null) {
                renderers.Clear();
            }
            else {
                renderers = new List<Renderer>();
            }
            switch (includeType) {
                case IncludeType.OnlyThisObject:
                    Renderer r = GetComponent<Renderer>();
                    if (r == null) {
                        includeType = IncludeType.IncludeChildren;
                        GetRenderers();
                        return;
                    }
                    else {
                        renderers.Add(r);
                    }
                    break;
                case IncludeType.ByLayer:
                    GetRenderersByLayer();
                    break;
                case IncludeType.IncludeChildren:
                    GetComponentsInChildren(true, renderers);
                    break;
                case IncludeType.Custom:
                    renderers.AddRange(customRenderers);
                    break;
            }
        }

        public void GetRenderersByLayer () {
            renderers.Clear();
            foreach (Renderer renderer in Misc.FindObjectsOfType<Renderer>(true)) {
                if ((1 << renderer.gameObject.layer & includeLayerMask) != 0) {
                    renderers.Add(renderer);
                }
            }
        }

        public void UpdateMaterialProperties () {
            needsUpdateMaterial = true;
        }


        public void UpdateMaterialPropertiesNow () {

            if (profile == null || !needsUpdateMaterial) return;
            needsUpdateMaterial = false;

            if (mat == null) {
                mat = Resources.Load<Material>("Materials/MystifyFX");
                mat = Instantiate(mat);
            }

            // Rendering settings
            mat.SetInt(ShaderParams.CullMode, (int)profile.cullMode);
            mat.SetInt(ShaderParams.ZTest, profile.renderOnTop ? (int)CompareFunction.Always : (int)CompareFunction.LessEqual);

            bool uses3Deffects = profile.intersection || profile.fakeLightIntensity > 0;
            if (uses3Deffects) {
                MystifyFXRendererFeature.usesCameraDepthTexture = true;
            }

            writesToDepth = profile.intersection;
            mat.SetInt(ShaderParams.ZWrite, writesToDepth ? 1 : 0);
            mat.SetTexture(ShaderParams.maskTex, profile.maskTexture);
            mat.SetFloat(ShaderParams.alphaCutoff, meshType == MeshType.Existing ? profile.alphaCutoff : 0);

            keywords.Clear();

            // Wrap mode
            if (profile.wrapMode == WrapMode.Repeat) {
                keywords.Add(ShaderParams.SKW_WRAP_MODE);
            }

            // Blur
            if (profile.blurIntensity > 0) {
                keywords.Add(ShaderParams.SKW_BLUR);
            }

            // LUT
            switch (profile.lutEffect) {
                case ColorEffect.LUT:
                    bool hasLut = profile.lutIntensity > 0 && profile.lutTexture != null;
                    bool hasLut3D = hasLut && profile.lutTexture is Texture3D;

                    if (hasLut || hasLut3D) {
                        if (hasLut3D) {
                            mat.SetTexture(ShaderParams.lut3DTexture, profile.lutTexture);
                            float x = 1f / profile.lutTexture.width;
                            float y = profile.lutTexture.width - 1f;
                            mat.SetVector(ShaderParams.lut3DParams, new Vector4(x * 0.5f, x * y, 0, 0));
                            keywords.Add(ShaderParams.SKW_LUT3D);
                        }
                        else {
                            mat.SetTexture(ShaderParams.lutTex, profile.lutTexture);
                            keywords.Add(ShaderParams.SKW_LUT);
                        }
                    }
                    break;
                case ColorEffect.Thermal:
                    keywords.Add(ShaderParams.SKW_THERMAL);
                    break;
                case ColorEffect.DirectionalTint:
                    if (profile.tintGradientTex == null) {
                        profile.CheckTintColors();
                    }
                    mat.SetTexture(ShaderParams.tintGradientTex, profile.tintGradientTex == null ? Texture2D.whiteTexture : profile.tintGradientTex);
                    mat.SetVector(ShaderParams.tintPos1, profile.tintPos1);
                    mat.SetVector(ShaderParams.tintPos2, profile.tintPos2);
                    mat.SetInt(ShaderParams.tintApplyAlphaToAll, profile.tintApplyAlphaToAll ? 1 : 0);
                    keywords.Add(ShaderParams.SKW_DIRECTIONAL_TINT);
                    break;
            }

            // Color boost
            mat.SetVector(ShaderParams.colorBoost, new Vector4(profile.brightness, profile.contrast, profile.vibrance, profile.globalOpacity));
            mat.SetVector(ShaderParams.scaleOffset, new Vector4(profile.sourceScale.x, profile.sourceScale.y, profile.sourceOffset.x, profile.sourceOffset.y));
            mat.SetVector(ShaderParams.uvScaleOffset, new Vector4(profile.uvScale.x, profile.uvScale.y, profile.uvOffset.x, profile.uvOffset.y));
            mat.SetVector(ShaderParams.fxData, new Vector4(profile.lutIntensity, profile.pixelate > 0 ? Screen.width / (1 + profile.pixelate) : 0, profile.scanLinesScreenSpace ? 1 : 0, profile.distortionFrequency * Mathf.PI));
            mat.SetVector(ShaderParams.fxData2, new Vector4(Screen.width / profile.noiseSize, profile.noiseAmount, profile.intersectionThickness, profile.blurIntensity));
            mat.SetVector(ShaderParams.fxData3, new Vector4(profile.scaleFactor, profile.hazeIntensity, profile.hazeThreshold, profile.distortionAnimationSpeed * 60f));
            mat.SetVector(ShaderParams.fxData7, new Vector4(profile.scanLinesIntensity > 0f ? 1f / profile.scanLinesSize : 0, profile.scanLinesAnimationSpeed * 20f, 1f / profile.zoomFactor, profile.intersectionNoise ? 0.002f : 0));
            mat.SetVector(ShaderParams.fxData11, new Vector4(profile.vertexDistortion * 0.1f, profile.vertexDistortionSpeed, profile.glassWaterDropIntensity, profile.scanLinesMaxDistance));

            float distortion = profile.distortion ? 1f : 0f;
            mat.SetVector(ShaderParams.fxData10, new Vector4(profile.distortionAmplitude.x * distortion, profile.distortionAmplitude.y * distortion, profile.distortionFalloff, profile.distortionRimPower));

            if (profile.scanLinesIntensity > 0) {
                mat.SetVector(ShaderParams.scanLinesColor, new Vector4(profile.scanLinesColor.r, profile.scanLinesColor.g, profile.scanLinesColor.b, profile.scanLinesIntensity));
            }

            if (profile.hazeIntensity > 0) {
                mat.SetVector(ShaderParams.fxData4, new Vector4(profile.hazeScale.x, profile.hazeScale.y, profile.hazeMode == MappingMode._2D ? profile.hazeSpeed * 20f : profile.hazeSpeed * 2f, 0));
                if (profile.hazeGradientTex == null) {
                    profile.CheckHazeColors();
                }
                mat.SetTexture(ShaderParams.hazeGradientTex, profile.hazeGradientTex == null ? Texture2D.whiteTexture : profile.hazeGradientTex);
                if (profile.hazeMode == MappingMode._2D) {
                    keywords.Add(ShaderParams.SKW_HAZE_OS);
                }
                else {
                    keywords.Add(ShaderParams.SKW_HAZE_WS);
                }
            }

            if (profile.rimIntensity > 0) {
                mat.SetColor(ShaderParams.rimColor, new Color(profile.rimColor.r, profile.rimColor.g, profile.rimColor.b, profile.rimIntensity));
                mat.SetVector(ShaderParams.fxData8, new Vector4(profile.rimPower, profile.rimTextureOpacity * 32f, profile.rimTextureScale, 0));
                mat.SetTexture(ShaderParams.rimTexture, profile.rimTexture);
                keywords.Add(ShaderParams.SKW_RIM);
            }

            /*
                        mat.SetVector(ShaderParams.fxData13, new Vector4(profile.patternIntensity, profile.patternScale, profile.patternMaskScale, profile.patternMaskThreshold));
                        if (profile.patternIntensity > 0) {
                            mat.SetTexture(ShaderParams.patternTex, profile.patternTexture);
                            mat.SetColor(ShaderParams.patternColor, profile.patternColor);
                            mat.SetTexture(ShaderParams.patternMask, profile.patternMask);
                        }
            */
            mat.SetVector(ShaderParams.fxData5, new Vector4(profile.frostIntensity, profile.frostSpread, profile.crystalize, profile.crystalizeSpread));
            if (profile.frostIntensity > 0 || profile.crystalize > 0) {
                if (profile.crystalize > 0) {
                    mat.SetVector(ShaderParams.fxData6, new Vector4(profile.crystalizeScale.x, profile.crystalizeScale.y, profile.crystalizeAnimationSpeed.x * 20f, profile.crystalizeAnimationSpeed.y * 20f));
                    keywords.Add(ShaderParams.SKW_CRYSTALIZE);
                }
                if (profile.frostIntensity > 0) {
                    mat.SetColor(ShaderParams.frostColor, profile.frostColor);
                    keywords.Add(ShaderParams.SKW_FROST);
                }
            }

            if (profile.intersection) {
                mat.SetColor(ShaderParams.intersectionColor, profile.intersectionColor);
                keywords.Add(ShaderParams.SKW_INTERSECTION);
            }

            if (profile.fakeLight) {
                if (profile.fakeLightGradientTex == null) {
                    profile.CheckFakeLightColors();
                }
                mat.SetVector(ShaderParams.fxData12, new Vector4(profile.fakeLightIntensity, profile.fakeLightFog, (1f - profile.fakeLightFalloff) * 0.99f, profile.fakeLightSpeed * 20f));
                mat.SetTexture(ShaderParams.fakeLightGradientTex, profile.fakeLightGradientTex == null ? Texture2D.whiteTexture : profile.fakeLightGradientTex);
                keywords.Add(ShaderParams.SKW_FAKE_LIGHT);
            }

            if (profile.rain) {
                mat.SetVector(ShaderParams.fxData9, new Vector4(profile.glassTinyDrops, profile.glassLargeDrops, profile.rainfall, profile.rainWind));
                mat.SetVector(ShaderParams.fxData14, new Vector4(profile.rainSpeed, profile.glassTinyDropsGridSize, profile.glassLargeDropsGridSize, profile.glassLargeDropsSpeed * 20f));
                keywords.Add(ShaderParams.SKW_RAIN);
            }

            int keywordsCount = keywords.Count;
            if (keywordsArray == null || keywordsArray.Length < keywordsCount) {
                keywordsArray = new string[keywordsCount];
            }
            int keywordsArrayCount = keywordsArray.Length;
            for (int k = 0; k < keywordsArrayCount; k++) {
                if (k < keywordsCount) {
                    keywordsArray[k] = keywords[k];
                }
                else {
                    keywordsArray[k] = "";
                }
            }
            mat.shaderKeywords = keywordsArray;
        }


        public void HitFX (Vector3 position, float intensity = 0, Color color = default, float radius = 0, float speed = 0, float noiseAmount = 0, float noiseScale = 0) {
            if (intensity <= 0) {
                intensity = profile.hitIntensity;
            }
            if (intensity <= 0) return;
            if (color == default) {
                color = profile.hitColor;
            }
            if (radius <= 0) {
                radius = profile.hitRadius;
            }
            if (speed <= 0) {
                speed = profile.hitSpeed;
            }
            if (noiseAmount <= 0) {
                noiseAmount = profile.hitNoiseAmount;
            }
            if (noiseScale <= 0) {
                noiseScale = profile.hitNoiseScale;
            }

            hitRequests.Add(new HitRequest {
                intensity = intensity,
                position = position,
                radius = radius,
                color = color,
                speed = speed,
                startTime = Time.timeSinceLevelLoad,
                noiseScale = noiseScale * 5f,
                noiseAmount = noiseAmount * 0.2f
            });
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected () {
            if (!enabled || meshType == MeshType.Existing) return;

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Matrix4x4 matrix = transform.localToWorldMatrix;

            switch (meshType) {
                case MeshType.Quad:
                    DrawQuadGizmo(matrix);
                    break;
                case MeshType.Disc:
                    DrawDiscGizmo(matrix);
                    break;
                case MeshType.Sphere:
                case MeshType.Hexasphere:
                    DrawSphereGizmo(matrix);
                    break;
                case MeshType.Cylinder:
                    DrawCylinderGizmo(matrix);
                    break;
            }
        }

        private void DrawQuadGizmo (Matrix4x4 matrix) {
            Vector3[] corners = new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            };

            for (int i = 0; i < 4; i++) {
                Vector3 start = matrix.MultiplyPoint(corners[i]);
                Vector3 end = matrix.MultiplyPoint(corners[(i + 1) % 4]);
                Gizmos.DrawLine(start, end);
            }

            // Only show warning if profile exists and culling is enabled
            if (profile != null && profile.cullMode != CullMode.Off) {
                Vector3 quadNormal = matrix.MultiplyVector(Vector3.forward);
                Vector3 toCam = SceneView.lastActiveSceneView.camera.transform.position - transform.position;
                bool isBackface = Vector3.Dot(quadNormal, toCam) > 0;

                // Show warning if looking at back face with back culling, or front face with front culling
                if ((isBackface && profile.cullMode == CullMode.Back) || (!isBackface && profile.cullMode == CullMode.Front)) {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = new Color(0, 0, 0, 0.8f);
                    style.fontSize = 18;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.richText = true;

                    Matrix4x4 prevMatrix = Handles.matrix;
                    Handles.matrix = matrix;
                    Handles.Label(new Vector3(0, 0.15f, 0), "THIS SIDE IS HIDDEN", style);
                    Handles.matrix = prevMatrix;
                }
            }
        }

        private void DrawDiscGizmo (Matrix4x4 matrix) {
            int segments = 32;
            float step = 360f / segments;

            for (int i = 0; i < segments; i++) {
                float angle1 = step * i * Mathf.Deg2Rad;
                float angle2 = step * (i + 1) * Mathf.Deg2Rad;

                Vector3 pos1 = new Vector3(Mathf.Cos(angle1) * 0.5f, Mathf.Sin(angle1) * 0.5f, 0);
                Vector3 pos2 = new Vector3(Mathf.Cos(angle2) * 0.5f, Mathf.Sin(angle2) * 0.5f, 0);

                Gizmos.DrawLine(matrix.MultiplyPoint(pos1), matrix.MultiplyPoint(pos2));
            }
        }

        private void DrawSphereGizmo (Matrix4x4 matrix) {
            Gizmos.matrix = matrix;
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DrawCylinderGizmo (Matrix4x4 matrix) {
            Gizmos.matrix = matrix * Matrix4x4.Scale(new Vector3(1, 2, 1));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }

#endif

    }

}