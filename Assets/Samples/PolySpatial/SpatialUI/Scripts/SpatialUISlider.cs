using UnityEngine;
using UnityEngine.UI;
using Unity;


namespace PolySpatial.Samples
{
    public class SpatialUISlider : SpatialUI
    {
        [Header("Config")]
        [SerializeField]
        MeshRenderer m_FillRenderer;
        [SerializeField] private bool resetOnEnable = true;


        float m_BoxColliderSizeX;


        public delegate void OnSliderUpdatedCallback(float _newState);
        public OnSliderUpdatedCallback OnSliderUpdated;

        [Header("Debug")]
        [SerializeField] private float currentPercentage = 0.5f;

        void Start()
        {
            m_BoxColliderSizeX = GetComponent<BoxCollider>().size.x;
        }

        private void OnEnable()
        {
            if (resetOnEnable)
            {
                SetPercentage(0.5f);
            }
        }


        public override void Press(Vector3 position)
        {
            base.Press(position);
            var localPosition = transform.InverseTransformPoint(position);
            var percentage = localPosition.x / m_BoxColliderSizeX + 0.5f;
            percentage = Mathf.Clamp(percentage, 0f, 1f);
            SetPercentage(percentage);
        }

        public void SetPercentage(float percentage)
        {
            if (m_FillRenderer != null)
                m_FillRenderer.material.SetFloat("_Percentage", Mathf.Clamp(percentage, 0.0f, 1.0f));
            currentPercentage = percentage;
            if (OnSliderUpdated != null) OnSliderUpdated(currentPercentage);
        }


    }
}
