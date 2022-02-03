using System.Collections;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public interface IAlignmentObject
    {
        /// <summary>
        /// Move Alignable Object to the World Space position
        /// </summary>
        /// <param name="worldSpaceTransformData"></param>
        public void Move(TransformData worldSpaceTransformData);

        /// <summary>
        /// Get the worldspace position of the alignable Object
        /// </summary>
        /// <returns></returns>
        public TransformData Get();

        /// <summary>
        /// Get's the current alignment transform object
        /// </summary>
        /// <returns></returns>
        public Transform Transform { get; }
    }
}
