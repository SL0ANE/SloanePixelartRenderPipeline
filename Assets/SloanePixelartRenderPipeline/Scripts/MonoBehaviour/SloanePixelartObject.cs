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
        Matrix4x4 m_ObjectMatrix = Matrix4x4.identity;
        Matrix4x4 m_ObjectOffset = Matrix4x4.identity;
        MaterialPropertyBlock m_ObjectMatrixPropertyBlock;
        Renderer m_Renderer;

        private static float m_UnitSize;
        public static float UnitSize
        {
            get
            {
                return m_UnitSize;
            }
            set
            {
                m_UnitSize = value;
            }
        }

        private static Matrix4x4 m_ViewMatrix;
        public static Matrix4x4 ViewMatrix
        {
            get
            {
                return m_ViewMatrix;
            }
            set
            {
                m_ViewMatrix = value;
            }
        }

        void OnEnable()
        {
            m_Renderer = GetComponent<Renderer>();
            if (m_ObjectMatrixPropertyBlock == null) m_ObjectMatrixPropertyBlock = new MaterialPropertyBlock();
            UpdateObjectMatrix();
        }

        void OnWillRenderObject()
        {
            if (transform.hasChanged)
            {
                UpdateObjectMatrix();
                transform.hasChanged = false;
            }

            ApplyObjectMatrix();
        }

        void ApplyObjectMatrix()
        {
            m_ObjectMatrixPropertyBlock.SetMatrix(ShaderPropertyStorage.SnapOffset, m_ObjectOffset);
            m_Renderer.SetPropertyBlock(m_ObjectMatrixPropertyBlock);
        }

        void UpdateObjectMatrix()
        {
            m_ObjectMatrix = GetSnappedMatrix(transform);

            m_ObjectOffset = m_ObjectMatrix * transform.localToWorldMatrix.inverse;
            Debug.Log("UpdateObjectMatrix");
        }

        Matrix4x4 GetSnappedMatrix(Transform curTrans)
        {
            Matrix4x4 curMatrix = Matrix4x4.identity;
            if(curTrans.parent != null)
            {
                curMatrix = GetSnappedMatrix(curTrans.parent);
            }
            curMatrix = curMatrix * Matrix4x4.TRS(curTrans.localPosition, curTrans.localRotation, curTrans.localScale);
            Matrix4x4 viewSpaceMatrix = m_ViewMatrix * curMatrix;
            Vector4 viewPos = viewSpaceMatrix.GetColumn(3);
            viewPos.x = Mathf.Round(viewPos.x / m_UnitSize) * m_UnitSize;
            viewPos.y = Mathf.Round(viewPos.y / m_UnitSize) * m_UnitSize;

            viewSpaceMatrix.SetColumn(3, viewPos);
            curMatrix = m_ViewMatrix.inverse * viewSpaceMatrix;

            return curMatrix;
        }
    }
}