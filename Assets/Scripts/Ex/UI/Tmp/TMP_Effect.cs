using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum EffectType
{
    TypeWriter = 0,
    Floating = 1,
    Italic = 2,
    Grow = 3,
    CornerZoomType = 4
}

public enum CornerZoomType
{
    BottomLeft = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 3
}

namespace Ex.UI.Tmp
{
    [RequireComponent(typeof(TMP_Text)), DisallowMultipleComponent]
    public class TMP_Effect : MonoBehaviour
    {
        public TMP_Text m_text;
        [Range(0, 1)] public float speed = 1;
        [SerializeField] private EffectType EffectType;

        [Header("浮动")] [SerializeField, Range(1, 5), Tooltip("浮动效果频率")]
        private float frequence = 1.0f;

        [SerializeField, Range(1, 5), Tooltip("浮动效果范围")]
        private float floatRange = 1.0f;

        [Header("斜体")] [SerializeField, Range(0, 60), Tooltip("角度")]
        private float slopeAngleX = 0.0f; //弧度

        [SerializeField, Range(0, 45), Tooltip("角度")]
        private float slopeAngleY = 0.0f;

        [SerializeField, Tooltip("反转方向")] private bool reverseDirection;

        private int characterCount = 0; //总字符数
        private Vector3[] originalVertices; //原始的顶点数据

        [Header("生长")] private float[] lerpValue;

        [SerializeField, Range(0, 2), Tooltip("动画完成时间")]
        private float overallLerpTime = 1f;


        [SerializeField] private CornerZoomType _cornerZoomType;

        private void Awake()
        {
            gameObject.TryGetComponent(out m_text);
        }

        private void Start()
        {
            if (m_text == null)
            {
                gameObject.AddComponent<TMP_Text>();
                gameObject.TryGetComponent(out m_text);
            }

            m_text.ForceMeshUpdate();
            characterCount = m_text.textInfo.characterCount;
            lerpValue = new float[characterCount];
            originalVertices = m_text.textInfo.meshInfo[0].vertices;
            StartCoroutine(MainBody());
        }

        private IEnumerator MainBody()
        {
            while (true)
            {
                m_text.ForceMeshUpdate();
                var textInfo = m_text.textInfo;
                var textInfoCopy = textInfo.CopyMeshInfoVertexData();
                characterCount = textInfo.characterCount;
                for (var i = 0; i < characterCount; i++)
                {
                    //单个文本元素的信息结构
                    var characterInfo = textInfo.characterInfo[i];
                    //获取当前角色使用的材质索引
                    var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                    //获取文本第一个顶点的索引
                    var vertexIndex = textInfo.characterInfo[i].vertexIndex;
                    //网格顶点颜色
                    var vertexColor = textInfo.meshInfo[characterInfo.materialReferenceIndex].colors32;
                    //获取顶点属性的结构
                    var vertices = textInfo.meshInfo[materialIndex].vertices;
                    var copyVertices = textInfoCopy[materialIndex].vertices;

                    if (!characterInfo.isVisible)
                    {
                        continue;
                    }

                    switch (EffectType)
                    {
                        case EffectType.TypeWriter:
                            yield return StartCoroutine(TypeWriter());
                            break;
                        case EffectType.Floating:
                            Floating(vertexIndex, ref vertices);
                            break;
                        case EffectType.Italic:
                            Italic(vertices, vertexIndex);
                            break;
                        case EffectType.Grow:
                            lerpValue[i] += 1 / overallLerpTime * Time.deltaTime;
                            Grow(vertexIndex, vertices, copyVertices, lerpValue[i]);
                            break;
                        case EffectType.CornerZoomType:
                            EffectTemplate(i, vertexIndex, _cornerZoomType.GetHashCode(), vertices, copyVertices,
                                CornerZoom);
                            break;
                    }
                }

                for (var i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    var meshInfo = textInfo.meshInfo[i];
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    m_text.UpdateGeometry(meshInfo.mesh, i);
                }

                m_text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                yield return null;
            }
        }

        private IEnumerator TypeWriter()
        {
            m_text.ForceMeshUpdate();
            var textInfo = m_text.textInfo;
            var total = textInfo.characterCount;
            var current = 0;
            var complete = false;
            while (!complete)
            {
                if (current > total)
                {
                    current = total;
                    yield return new WaitForSecondsRealtime(1f);
                    complete = true;
                }

                m_text.maxVisibleCharacters = current;
                current += 1;
                yield return new WaitForSecondsRealtime(speed);
            }

            yield return null;
        }

        private void Floating(int vertexIndex, ref Vector3[] vertices)
        {
            for (var i = 0; i < 4; i++)
            {
                var originalValue = vertices[vertexIndex + i];
                var verticesPosY = Mathf.Sin(Time.time * frequence * Mathf.PI + originalValue.x) * floatRange;
                vertices[vertexIndex + i] = originalValue + new Vector3(0, verticesPosY, 0);
            }
        }

        private void Italic(Vector3[] vertices, int vertexIndex)
        {
            for (var i = 0; i < 4; i++)
            {
                //角度转弧度
                var xAngle = slopeAngleX * (Mathf.PI / 180);
                var yAngle = slopeAngleY * (Mathf.PI / 180);
                vertices[vertexIndex + i] = originalVertices[vertexIndex + i] + (reverseDirection ? -1 : 1) *
                    new Vector3(originalVertices[vertexIndex + i].y = Mathf.Tan(xAngle),
                        originalVertices[vertexIndex + i].x * Mathf.Tan(yAngle), 0);
            }
        }

        private void Grow(int vertexIndex, Vector3[] vertices, Vector3[] copyVertices, float lerp)
        {
            var a = copyVertices[vertexIndex];
            var a2 = copyVertices[vertexIndex + 3];
            vertices[vertexIndex] = Vector3.Lerp(a, copyVertices[vertexIndex], lerp);
            vertices[vertexIndex + 3] = Vector3.Lerp(a2, copyVertices[vertexIndex + 3], lerp);
            vertices[vertexIndex + 1] = Vector3.Lerp(a, copyVertices[vertexIndex + 1], lerp);
            vertices[vertexIndex + 2] = Vector3.Lerp(a2, copyVertices[vertexIndex + 2], lerp);
        }

        private void CornerZoom(int characterIndex, int vertexIndex, int cornerIndex, Vector3[] vertices,
            Vector3[] copyVertices)
        {
            lerpValue[characterIndex] += 1 / overallLerpTime * Time.deltaTime;
            var corner = copyVertices[cornerIndex];
            vertices[vertexIndex] = Vector3.Lerp(corner, copyVertices[vertexIndex], lerpValue[characterIndex]);
            vertices[vertexIndex + 3] = Vector3.Lerp(corner, copyVertices[vertexIndex + 3], lerpValue[characterIndex]);
            vertices[vertexIndex + 1] = Vector3.Lerp(corner, copyVertices[vertexIndex + 1], lerpValue[characterIndex]);
            vertices[vertexIndex + 2] = Vector3.Lerp(corner, copyVertices[vertexIndex + 2], lerpValue[characterIndex]);
        }

        private void EffectTemplate(int characterIndex, int vertexIndex, int effectIndex, Vector3[] vertices,
            Vector3[] copyVertices, Action<int, int, int, Vector3[], Vector3[]> _functionAction)
        {
            _functionAction(characterIndex, vertexIndex, effectIndex, vertices, copyVertices);
        }
    }
}