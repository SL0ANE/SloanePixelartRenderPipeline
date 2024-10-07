using System;
using System.Collections;
using System.Collections.Generic;
using Sloane;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[Serializable]
public struct RimLight
{
    public Vector3 Direction;
    public Vector3 Color;
}

namespace Sloane
{
    [ExecuteAlways]
    public class SloanePixelartRimlight : MonoBehaviour
    {
        private static Dictionary<SloanePixelartRimlight, RimLight> m_Instances = new Dictionary<SloanePixelartRimlight, RimLight>();
        private static RimLight[] m_RimLights = new RimLight[MAX_RIMLIGHTS];
        private static int m_RimLightsCount = 0;

        public const int MAX_RIMLIGHTS = 32;
        public static RimLight[] RimLights => m_RimLights;
        public static int RimLightsCount => m_RimLightsCount;

        [SerializeField]
        private Color m_Color = new Color(1, 1, 1, 1);

        void OnEnable()
        {
            RimLight pendingRimLight = new RimLight()
            {
                Direction = transform.forward,
                Color = new Vector3(m_Color.r, m_Color.g, m_Color.b)
            };
            m_Instances.Add(this, pendingRimLight);
        }

        void OnDisable()
        {
            m_Instances.Remove(this);
        }

        void LateUpdate()
        {
            RimLight rimLight = m_Instances[this];
            rimLight.Direction = transform.forward;
            rimLight.Color = new Vector3(m_Color.r, m_Color.g, m_Color.b);
            m_Instances[this] = rimLight;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = m_Color;
            Gizmos.DrawIcon(transform.position, "Light Gizmo", true);
            Gizmos.DrawRay(transform.position, transform.forward);
        }

        public static void RefreshRimLights()
        {
            int i = 0;
            foreach (KeyValuePair<SloanePixelartRimlight, RimLight> entry in m_Instances)
            {
                m_RimLights[i] = entry.Value;
                i++;
            }

            m_RimLightsCount = i;

            // Debug.Log("RimLightsCount: " + m_RimLightsCount);
        }
    }
}
