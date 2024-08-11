using System.Collections;
using System.Collections.Generic;
using Sloane;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Sloane
{
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public class SloanePixelartObject : MonoBehaviour
    {
        private struct TransformCache
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        TransformCache transformCache;
        bool PerformedSnap;
        void OnWillRenderObject()
        {
            if(SloanePixelartCastCamera.Current != null)
            {
                PerformedSnap = true;
                transformCache = new TransformCache
                {
                    Position = transform.position,
                    Rotation = transform.rotation
                };

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
            }
        }

        void OnRenderObject()
        {
            if(PerformedSnap)
            {
                PerformedSnap = false;
                transform.position = transformCache.Position;
                transform.rotation = transformCache.Rotation;
            }
        }
    }
}