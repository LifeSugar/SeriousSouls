using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace INab.BetterFog.Runtime
{
    public interface IGradientTextureForEditor
    {
        void CreateTexture();

        Texture2D GetTexture();

        void LoadExisitingTexture();
    }

    /// <summary>
    /// Main Asset, holds settings, create, hold and change Texture2D's pixels, name
    /// </summary>
    [CreateAssetMenu(fileName = "NewGradientName", menuName = "BetterFog/Gradient")]
    public class GradientTexture : ScriptableObject, IEquatable<Texture2D>, ISerializationCallbackReceiver,
        IGradientTextureForEditor
    {
        [SerializeField] Vector2Int _resolution = new Vector2Int(256, 1);
        [SerializeField, GradientUsage(false)] Gradient _gradient = GetDefaultGradient();
        [SerializeField, HideInInspector] Texture2D _texture = default;
        [SerializeField] List<string> _hexadecimals;

        public Texture2D GetTexture() => _texture;

        int _width => _resolution.x;
        int _height => _resolution.y;

        public static implicit operator Texture2D(GradientTexture asset) => asset.GetTexture();

        //TODO better default gradient
        static Gradient GetDefaultGradient() => new Gradient
        {
            alphaKeys = new[] { new GradientAlphaKey(1, 1) },
            colorKeys = new[]
            {
                new GradientColorKey(Color.black, 0),
                new GradientColorKey(Color.white, 1)
            }
        };

        public void FillGradientWithHexadecimals()
        {
            if(_hexadecimals.Count < 1)
            {
                return;
            }

            int i = 0;
            float lerp = 0;
            float step = (float)1 / (float)(_hexadecimals.Count-1);

            var colorKeys = new GradientColorKey[_hexadecimals.Count];
            var alphaKeys = new GradientAlphaKey[_hexadecimals.Count];

            foreach (var item in _hexadecimals)
            {
                string hex = item;
                if (hex[0] != '#') hex = "#" + hex;


                Color color;
                ColorUtility.TryParseHtmlString(hex, out color);

                colorKeys[i] = new GradientColorKey(color,lerp);
                alphaKeys[i] = new GradientAlphaKey(1, lerp);
                lerp += step;

                i++;
            }

            _gradient.SetKeys(colorKeys, alphaKeys);

            ValidateTextureValues();
        }

        public void InvertGradient()
        {
            // Invert colors
            GradientColorKey[] originalColorKeys = _gradient.colorKeys;
            GradientColorKey[] invertedColorKeys = new GradientColorKey[originalColorKeys.Length];

            for (int i = 0; i < originalColorKeys.Length; i++)
            {
                invertedColorKeys[i].color = originalColorKeys[originalColorKeys.Length - 1 - i].color;
                invertedColorKeys[i].time = 1.0f - originalColorKeys[originalColorKeys.Length - 1 - i].time;
            }

            // Invert alphas
            GradientAlphaKey[] originalAlphaKeys = _gradient.alphaKeys;
            GradientAlphaKey[] invertedAlphaKeys = new GradientAlphaKey[originalAlphaKeys.Length];

            for (int i = 0; i < originalAlphaKeys.Length; i++)
            {
                invertedAlphaKeys[i].alpha = originalAlphaKeys[originalAlphaKeys.Length - 1 - i].alpha;
                invertedAlphaKeys[i].time = 1.0f - originalAlphaKeys[originalAlphaKeys.Length - 1 - i].time;
            }

            _gradient.SetKeys(invertedColorKeys, invertedAlphaKeys);

            ValidateTextureValues();
        }

        public void FillColors()
        {
            _texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    float tHorizontal = (float) x / _width;

                    Color color = _gradient.Evaluate(tHorizontal);
                    _texture.SetPixel(x, y, color);
                }
            }

            _texture.Apply();
        }

        public bool Equals(Texture2D other)
        {
            return _texture.Equals(other);
        }

        void OnValidate() => ValidateTextureValues();

        void IGradientTextureForEditor.LoadExisitingTexture()
        {
            #if UNITY_EDITOR
            if (!_texture)
            {
                string assetPath = AssetDatabase.GetAssetPath(this);
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
            #endif
        }

        private string GetName()
        {
            return "Preview_" + name;
        }

        void IGradientTextureForEditor.CreateTexture()
        {
            #if UNITY_EDITOR

            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath)) return;

            if (!_texture && this != null && !EditorApplication.isUpdating)
            {
                AssetDatabase.ImportAsset(assetPath);
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }

            if (!_texture)
            {
                _texture = new Texture2D(_resolution.x, _resolution.y,TextureFormat.RGBAFloat,false,true);
                if (_texture.name != GetName()) _texture.name = GetName();
            }

            if (!_texture) return;

            ValidateTextureValues();

            if (!EditorUtility.IsPersistent(this)) return;
            if (AssetDatabase.IsSubAsset(_texture)) return;
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath)) return;

            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
            AssetDatabase.AddObjectToAsset(_texture, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            #endif
        }

        void ValidateTextureValues()
        {
            if (!_texture) return;
            if (_texture.name != GetName())
            {
                _texture.name = GetName();
            }
            else
            {
                if (_texture.width != _resolution.x ||
                    _texture.height != _resolution.y)
                {
                    _texture.Reinitialize(_resolution.x, _resolution.y);
                }
              
#if UNITY_EDITOR
                _texture.alphaIsTransparency = true;
#endif
                FillColors();
                SetDirtyTexture();
            }
        }

        #region Editor

        void SetDirtyTexture()
        {
#if UNITY_EDITOR
            if (!_texture) return;

            EditorUtility.SetDirty(_texture);
#endif
        }

        #endregion

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!_texture || _texture.name == GetName()) return;

            _texture.name = GetName();

            //AssetDatabase.SaveAssets();
  #endif
        }
    }
}
