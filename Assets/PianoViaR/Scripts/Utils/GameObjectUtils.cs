using UnityEngine;

namespace PianoViaR.Utils
{
    public enum Axis { X, Y, Z };

    public static class GameObjectUtils
    {
        public static Vector2 TextBoxSize(this GameObject gameObject)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            Ensure.ArgumentNotNull(rectTransform);

            return rectTransform.sizeDelta;
        }

        public static Vector2 TextBoxExtents(this GameObject gameObject)
        {
            return gameObject.TextBoxSize() / 2;
        }

        public static void TextSetText(this GameObject gameObject, string text)
        {
            var tmpText = gameObject.GetComponent<TMPro.TMP_Text>();
            Ensure.ArgumentNotNull(tmpText, "TextMeshPro TMP_Text");

            tmpText.text = text;
        }

        public static void TextSetBoxSize(this GameObject gameObject, in Vector2 size)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            Ensure.ArgumentNotNull(rectTransform);

            rectTransform.sizeDelta = size;
        }

        public static void TextFitTo(this GameObject gameObject, in float reference, Axis axis)
        {
            var boxDimensions = gameObject.TextBoxSize();
            float dimension;

            switch (axis)
            {
                case Axis.X:
                    dimension = boxDimensions.x;
                    break;
                case Axis.Y:
                    dimension = boxDimensions.y;
                    break;
                default:
                    dimension = reference;
                    break;
            }

            if (dimension == reference) return;

            gameObject.TextSetBoxSize(boxDimensions * (reference / dimension));
        }

        public static void TextFitOnlyTo(this GameObject gameObject, in float reference, Axis localAxisToFit, Axis globalAxisReference)
        {
            var boxDimensions = gameObject.TextBoxSize();
            float dimension;
            float axisScale;

            switch (localAxisToFit)
            {
                case Axis.X:
                    dimension = boxDimensions.x;
                    break;
                case Axis.Y:
                    dimension = boxDimensions.y;
                    break;
                default:
                    dimension = reference;
                    break;
            }

            if (dimension == reference) return;

            axisScale = reference / dimension;

            Vector2 newScale;

            switch (localAxisToFit)
            {
                case Axis.X:
                    newScale = new Vector2(axisScale, 1);
                    break;
                case Axis.Y:
                    newScale = new Vector2(1, axisScale);
                    break;
                default:
                    newScale = Vector2.one;
                    break;
            }

            gameObject.TextSetBoxSize(Vector2.Scale(boxDimensions, newScale));
        }

        public static void TextFitToX(this GameObject gameObject, in float x)
        {
            gameObject.TextFitTo(x, Axis.X);
        }

        public static void TextFitToY(this GameObject gameObject, in float y)
        {
            gameObject.TextFitTo(y, Axis.Y);
        }

        public static void TextFitToWidth(this GameObject gameObject, in float width) => gameObject.TextFitToX(width);
        public static void TextFitToHeight(this GameObject gameObject, in float height) => gameObject.TextFitToY(height);

        public static void TextFitOnlyToX(this GameObject gameObject, in float x)
        {
            gameObject.TextFitOnlyTo(x, Axis.X, Axis.X);
        }

        public static void TextFitOnlyToY(this GameObject gameObject, in float y)
        {
            gameObject.TextFitOnlyTo(y, Axis.Y, Axis.Y);
        }

        public static void TextFitOnlyToWidth(this GameObject gameObject, in float width) => gameObject.TextFitOnlyToX(width);
        public static void TextFitOnlyToHeight(this GameObject gameObject, in float height) => gameObject.TextFitOnlyToY(height);

        public static Vector2 TextUpperLeftOffset(this GameObject gameObject)
        {
            var textExtents = gameObject.TextBoxExtents();
            return Vector2.Scale(textExtents, new Vector2(1, -1));
        }

        public static Vector2 TextUpperCenterOffset(this GameObject gameObject)
        {
            var textExtents = gameObject.TextBoxExtents();
            return Vector2.Scale(textExtents, new Vector2(0, -1));
        }

        // public static Vector2 TextCenterOffset(this GameObject gameObject)
        // {
        //     var textExtents = gameObject.TextBoxExtents();
        //     return textExtents;
        // }

        public static Vector2 TextCenterLeftOffset(this GameObject gameObject)
        {
            var textExtents = gameObject.TextBoxExtents();
            return Vector2.Scale(textExtents, new Vector2(1, 0));
        }

        public static void TextPlaceUpperLeft(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + (Vector3)gameObject.TextUpperLeftOffset(), offsets);
        }

        public static void TextPlaceCenterLeft(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + (Vector3)gameObject.TextCenterLeftOffset(), offsets);
        }

        public static void TextPlaceUpperCenter(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + (Vector3)gameObject.TextUpperCenterOffset(), offsets);
        }

        public static void TextPlaceCenter(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.PlaceCenter(position, offsets);
        }

        public static Bounds GlobalBounds(this GameObject gameObject, bool force = false)
        {
            // Save original rotation
            var originalRotation = gameObject.transform.rotation;

            if (force)
            {
                // Reset object's rotation so it doesn't affect the measured bounds
                gameObject.transform.rotation = Quaternion.identity;
            }

            var renderer = gameObject.GetComponent<Renderer>();
            Bounds globalBounds;

            if (renderer == null)
            {
                var collider = gameObject.GetComponent<Collider>();

                if (collider == null)
                {
                    throw new System.ArgumentException("GameObject has no Collider or Renderer component");
                }
                else
                {
                    globalBounds = collider.bounds;
                }
            }
            else
            {
                globalBounds = renderer.bounds;
            }

            // Reset rotation back to normal
            gameObject.transform.rotation = originalRotation;

            return globalBounds;
        }

        public static Bounds LocalBounds(this GameObject gameObject)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null)
            {
                throw new System.ArgumentException("GameObject has no MeshFilter component");
            }
            else
            {
                return meshFilter.mesh.bounds;
            }
        }

        public static Vector3 LocalBoxSize(this GameObject gameObject)
        {
            return gameObject.LocalBounds().size;
        }

        public static Vector3 LocalExtents(this GameObject gameObject)
        {
            return gameObject.LocalBounds().extents;
        }
        public static Vector3 BoxSize(this GameObject gameObject, bool force = false)
        {
            return gameObject.GlobalBounds(force).size;
        }

        public static Vector3 BoxExtents(this GameObject gameObject, bool force = false)
        {
            return gameObject.GlobalBounds(force).extents;
        }

        public static Vector3 UpperLeftOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(1, -1, 0));
        }

        public static Vector3 UpperCenterOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(0, -1, 0));
        }
        public static Vector3 CenterLeftOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(1, 0, 0));
        }

        public static Vector3 BottomLeftOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(1, 1, 0));
        }

        public static Vector3 BottomCenterOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(0, 1, 0));
        }

        public static Vector3 UpperRightOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(-1, 1, 0));
        }

        public static Vector3 CenterRightOffset(this GameObject gameObject, bool force = false)
        {
            var extents = gameObject.BoxExtents(force);
            return Vector3.Scale(extents, new Vector3(-1, 0, 0));
        }

        public static void ApplyOffset(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            var totalOffset = position;

            foreach (Vector3 offset in offsets)
                totalOffset += offset;

            gameObject.transform.position = totalOffset;
        }

        public static void PlaceUpperLeft(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.UpperLeftOffset(), offsets);
        }

        public static void PlaceUpperCenter(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.UpperCenterOffset(), offsets);
        }

        public static void PlaceCenterLeft(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.CenterLeftOffset(), offsets);
        }

        public static void PlaceBottomLeft(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.BottomLeftOffset(), offsets);
        }

        public static void PlaceBottomCenter(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.BottomCenterOffset(), offsets);
        }

        public static void PlaceUpperRight(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.UpperRightOffset(), offsets);
        }

        public static void PlaceCenterRight(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position + gameObject.CenterRightOffset(), offsets);
        }

        public static void PlaceCenter(this GameObject gameObject, in Vector3 position, params Vector3[] offsets)
        {
            gameObject.ApplyOffset(position, offsets);
        }

        public static void FitTo(this GameObject gameObject, in float reference, Axis axis)
        {
            var boxDimensions = gameObject.BoxSize();
            float dimension;

            switch (axis)
            {
                case Axis.X:
                    dimension = boxDimensions.x;
                    break;
                case Axis.Y:
                    dimension = boxDimensions.y;
                    break;
                case Axis.Z:
                    dimension = boxDimensions.z;
                    break;
                default:
                    dimension = reference;
                    break;
            }

            if (dimension == reference) return;

            gameObject.transform.localScale *= reference / dimension;
        }

        public static void FitOnlyTo(this GameObject gameObject, in float reference, Axis localAxisToFit, Axis globalAxisReference)
        {
            var boxDimensions = gameObject.BoxSize();
            float dimension;
            float axisScale;

            switch (globalAxisReference)
            {
                case Axis.X:
                    dimension = boxDimensions.x;
                    break;
                case Axis.Y:
                    dimension = boxDimensions.y;
                    break;
                case Axis.Z:
                    dimension = boxDimensions.z;
                    break;
                default:
                    dimension = reference;
                    break;

            }

            if (dimension == reference) return;

            axisScale = reference / dimension;

            Vector3 localScale = gameObject.transform.localScale;
            Vector3 newScale;

            switch (localAxisToFit)
            {
                case Axis.X:
                    newScale = new Vector3(axisScale, 1, 1);
                    break;
                case Axis.Y:
                    newScale = new Vector3(1, axisScale, 1);
                    break;
                case Axis.Z:
                    newScale = new Vector3(1, 1, axisScale);
                    break;
                default:
                    newScale = Vector3.one;
                    break;
            }
            gameObject.transform.localScale = Vector3.Scale(localScale, newScale);
        }

        public static void FitToX(this GameObject gameObject, in float x)
        {
            gameObject.FitTo(x, Axis.X);
        }

        public static void FitToY(this GameObject gameObject, in float y)
        {
            gameObject.FitTo(y, Axis.Y);
        }

        public static void FitToZ(this GameObject gameObject, in float z)
        {
            gameObject.FitTo(z, Axis.Z);
        }

        public static void FitToWidth(this GameObject gameObject, in float width) => gameObject.FitToX(width);
        public static void FitToHeight(this GameObject gameObject, in float height) => gameObject.FitToY(height);
        public static void FitToDepth(this GameObject gameObject, in float depth) => gameObject.FitToZ(depth);

        public static void FitOnlyToX(this GameObject gameObject, in float x)
        {
            gameObject.FitOnlyTo(x, Axis.X, Axis.X);
        }

        public static void FitOnlyToY(this GameObject gameObject, in float y)
        {
            gameObject.FitOnlyTo(y, Axis.Y, Axis.Y);
        }

        public static void FitOnlyToZ(this GameObject gameObject, in float z)
        {
            gameObject.FitOnlyTo(z, Axis.Z, Axis.Z);
        }

        public static void FitOnlyToWidth(this GameObject gameObject, in float width) => gameObject.FitOnlyToX(width);
        public static void FitOnlyToHeight(this GameObject gameObject, in float height) => gameObject.FitOnlyToY(height);
        public static void FitOnlyToDepth(this GameObject gameObject, in float depth) => gameObject.FitOnlyToZ(depth);

        // Remove all the game object's children
        public static void Clear(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static void ResizeCollider(Collider collider, Vector3 center, Vector3 size)
        {

        }

        public static bool ResizeBoxCollider(this GameObject gameObject, Vector3 center, Vector3 size)
        {
            var boxCollider = gameObject.GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                return false;
            }

            boxCollider.size = size;
            boxCollider.center = center;

            return true;
        }


        /****************************************************************
        *****************************************************************
        *****************************************************************
                                WORK IN PROGRESS
        *****************************************************************
        *****************************************************************
        ****************************************************************
        */
        // public static Vector3 Centroid(params GameObject[] gameObjects)
        // {
        //     var centroid = Vector3.zero;

        //     for (int i = 0; i < gameObjects.Length; i++)
        //     {
        //         if (gameObjects[i] != null)
        //         {
        //             centroid += gameObjects[i].transform.position;
        //         }
        //     }

        //     centroid /= gameObjects.Length;

        //     return centroid;
        // }

        // public static Vector3 Centroid(this Transform[] transforms)
        // {
        //     var centroid = Vector3.zero;

        //     for (int i = 0; i < transforms.Length; i++)
        //     {
        //         centroid += transforms[i].position;
        //     }

        //     centroid /= transforms.Length;

        //     return centroid;
        // }

        // public static Vector3 Centroid(this GameObject[] gameObjects, bool recursive = false)
        // {
        //     var centroid = Vector3.zero;
        //     var totalTransforms = 0;

        //     if (recursive)
        //     {
        //         for (int i = 0; i < gameObjects.Length; i++)
        //         {
        //             var currentGO = gameObjects[i];
        //             // *GetComponentsInChildren* already returns the parent object so it is included in the nested iteration
        //             var attachedTransforms = currentGO.transform.GetComponentsInChildren<Transform>();

        //             for (int transformIdx = 0; transformIdx < attachedTransforms.Length; transformIdx++)
        //             {
        //                 centroid += attachedTransforms[transformIdx].position;

        //                 totalTransforms += 1;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         for (int i = 0; i < gameObjects.Length; i++)
        //         {
        //             centroid += gameObjects[i].transform.position;
        //         }
        //     }

        //     centroid /= totalTransforms;

        //     return centroid;
        // }

        // public static void PlaceAtCentroid(this GameObject gameObject)
        // {
        //     var centroid = Vector3.zero;
        //     var totalTransforms = 0;
        //     var allChildrenTransforms = gameObject.GetComponentsInChildren<Transform>();

        //     for (int i = 0; i < allChildrenTransforms.Length; i++)
        //     {
        //         var currentChild = allChildrenTransforms[i];
        //         if (currentChild.transform.position != Vector3.zero)
        //         {
        //             centroid += allChildrenTransforms[i].transform.position;
        //             totalTransforms += 1;
        //         }
        //     }

        //     centroid /= totalTransforms;

        //     var directChildren = gameObject.transform.GetComponents<Transform>();

        //     // gameObject.transform.DetachChildren();
        //     // foreach (Transform directChild in directChildren)
        //     //     directChild.SetParent(null);

        //     for (int i = 0; i < directChildren.Length; i++)
        //     {
        //         if (directChildren[i] != gameObject.transform)
        //         {
        //             directChildren[i].SetParent(null);
        //         }
        //     }

        //     gameObject.transform.position = centroid;

        //     for (int i = 0; i < directChildren.Length; i++)
        //     {
        //         if (directChildren[i] != gameObject.transform)
        //         {
        //             directChildren[i].SetParent(gameObject.transform);
        //         }
        //     }

        //     // foreach (Transform directChild in directChildren)
        //     //     directChild.SetParent(gameObject.transform);
        // }
    }
}