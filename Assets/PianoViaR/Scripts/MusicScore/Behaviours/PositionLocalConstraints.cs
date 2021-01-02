﻿using UnityEngine;

namespace PianoViaR.Score.Behaviors
{
    public class PositionLocalConstraints : MonoBehaviour
    {
        [Header("Freeze Local Position")]
        [SerializeField]
        bool x = false;
        [SerializeField]
        bool y = false;
        [SerializeField]
        bool z = false;

        Vector3 localPosition0;    //original local position

        private void Start()
        {
            SetOriginalLocalPosition();
        }

        private void Update()
        {
            float x, y, z;


            if (this.x)
                x = localPosition0.x;
            else
                x = transform.localPosition.x;

            if (this.y)
                y = localPosition0.y;
            else
                y = transform.localPosition.y;

            if (this.z)
                z = localPosition0.z;
            else
                z = transform.localPosition.z;


            transform.localPosition = new Vector3(x, y, z);

        }

        public void SetOriginalLocalPosition()
        {
            localPosition0 = transform.localPosition;
        }

    }
}