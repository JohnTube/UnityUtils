namespace PolyTics.UnityUtils
{

    using System;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;


    // TODO: add XML comments
    public static class ExtensionMethods
    {

        #region Generic

        public static bool IsNull<T>(this T o)
        {
            return o == null;
        }

        #endregion Generic

        #region Enum

        public static string Stringify(this Enum enumVal)
        {
            return Enum.GetName(enumVal.GetType(), enumVal);
        }

        #endregion Enum

        #region string

        // TODO: suppress ReSharper warning about PascalCase
        public static bool IsURL(this string source)
        {
            if (source.IsNullOrEmpty()) { return false; }// TODO: raise exception or log error
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        #endregion string

        #region Arrays

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ToBase64(this byte[] bytes)
        {
            if (bytes.IsNullOrEmpty()) { return null; } // TODO: raise exception or log error
            return Convert.ToBase64String(bytes);
        }

        #endregion Arrays

        #region misc.

        // missing from old .Net/Mono versions
        // investigate possible memory leaks or OOM exception if reused excessively
        public static void Clear(this StringBuilder builder)
        {
            if (builder.IsNull()) { return; } // TODO: raise exception or log error
            builder.Length = 0;
            //builder.Capacity = 0;
            //builder = new StringBuilder();
        }

        #endregion misc.

        #region Transform

        public static bool HasChild(this Transform trans, string name)
        {
            if (trans.IsNull()) { return false; } // TODO: raise exception or log error
            for (int i = 0; i < trans.childCount; i++)
            {
                if (trans.GetChild(i).name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

        public static void DestroyChildren(this Transform parent)
        {
            if (parent.IsNull()) { return; } // TODO: raise exception or log error
            int childCount = parent.childCount;
            for (int i = childCount - 1; i > -1; i--)
            {
                MonoBehaviour.DestroyObject(parent.GetChild(i).gameObject);
            }
        }

        public static void MoveChildren(this Transform oldParent, Transform newParent)
        {
            if (oldParent.IsNull() || newParent.IsNull()) { return; } // TODO: raise exception or log error
            int childCount = oldParent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                oldParent.GetChild(0).SetParent(newParent, false);
            }
        }

        #endregion Transform

        #region GameObject

        //<summary>http://answers.unity3d.com/questions/555101/possible-to-make-gameobjectgetcomponentinchildren.html</summary>
        ///////////////////////////////////////////////////////////
        // Essentially a reimplementation of
        // GameObject.GetComponentInChildren< T >()
        // Major difference is that this DOES NOT skip deactivated
        // game objects
        ///////////////////////////////////////////////////////////
        public static TType GetComponentInChildren<TType>(this GameObject objRoot, bool includeInactive) where TType : Component
        {
            if (includeInactive)
            {
                // if we don't find the component in this object
                // recursively iterate children until we do
                TType tRetComponent = objRoot.GetComponent<TType>();

                if (null == tRetComponent)
                {
                    // transform is what makes the hierarchy of GameObjects, so
                    // need to access it to iterate children
                    Transform trnsRoot = objRoot.transform;
                    int iNumChildren = trnsRoot.childCount;

                    // could have used foreach(), but it causes GC churn
                    for (int iChild = 0; iChild < iNumChildren; ++iChild)
                    {
                        // recursive call to this function for each child
                        // break out of the loop and return as soon as we find
                        // a component of the specified type
                        tRetComponent = trnsRoot.GetChild(iChild).gameObject.GetComponentInChildren<TType>(true);
                        if (null != tRetComponent)
                        {
                            break;
                        }
                    }
                }

                return tRetComponent;
            }
            else
            {
                return objRoot.GetComponentInChildren<TType>();
            }
        }
        
        public static void SetActiveSafe(this GameObject gameObject, bool active)
        {
            if (gameObject.IsNull())
            {
                Debug.LogWarning("GameObject is null");
                return;
            }
            if (active != gameObject.activeSelf)
            {
                gameObject.SetActive(active);
            }
        }

        #endregion GameObject

        #region UI

        #region UI Text

        public static void SetText(this Text text, object t)
        {
            if (text.IsNull() || t.IsNull()) { return; } // TODO: raise exception or log error
            text.text = t.ToString();
        }

        public static void SetText(this Text text, string format, params object[] objects)
        {
            if (text.IsNull() || format.IsNullOrEmpty()) { return; } // TODO: raise exception or log error
            text.text = Utils.Format(format, objects);
        }

        #endregion UI Text

        #region UI Image

        public static void SetTexture2D(this Image image, Texture2D texture)
        {
            if (image.IsNull() || texture.IsNull())
            {
                return; // TODO: raise exception or log error
            }
            image.overrideSprite =
                Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5F, 0.5F)
                );
        }

        #endregion UI Image

        #endregion UI

        #region Sprite serialization

        public static Sprite ToSprite(this byte[] bytes, TextureFormat format, int width = 2, int height = 2, bool mipmap = false)
        {
            if (bytes.IsNull() || bytes.Length == 0 || width < 0 || height < 0)
            {
                return null; // TODO: raise exception or log error
            }
            Texture2D texture = new Texture2D(width, height, format, false);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F));
        }

        // TODO: suppress ReSharper warning about PascalCase
        public static byte[] ToBytesPNG(this Sprite sprite)
        {
            if (sprite.IsNull()) { return null; } // TODO: raise exception or log error
            return sprite.texture.EncodeToPNG();
        }

        // TODO: suppress ReSharper warning about PascalCase
        public static byte[] ToBytesJPG(this Sprite sprite)
        {
            if (sprite.IsNull()) { return null; } // TODO: raise exception or log error
            return sprite.texture.EncodeToJPG();
        }

        public static Sprite ToSprite(this string base64, TextureFormat format, int width = 2, int height = 2, bool mipmap = false)
        {
            if (base64.IsNullOrEmpty()) { return null; } // TODO: raise exception or log error
            return Convert.FromBase64String(base64).ToSprite(format, width, height, mipmap);
        }

        #endregion Sprite serialization

        #region UnityEngine.Object

        // https://jacx.net/2015/11/20/dont-use-equals-null-on-unity-objects.html
        public static bool IsReallyNull(this UnityEngine.Object unityObject)
        {
            return unityObject;
            //return ReferenceEquals(unityObject, null);
            //return unityObject as object == null;
            //return "null".Equals(unityObject.ToString());
        }

        public static void DestroyAndMakeNull(this UnityEngine.Object unityObject)
        {
            UnityEngine.Object.Destroy(unityObject);
            unityObject = null;
        }

        public static bool IsDestroyedOrNotInAScene(this UnityEngine.Object unityObject)
        {
            return unityObject.IsReallyNull() && unityObject.IsNull();
        }

        #endregion
    }
}