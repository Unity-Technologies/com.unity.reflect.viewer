#if !UNITY_DOTSPLAYER
using Unity.Properties.Internal;
using UnityEngine;

namespace Unity.Properties
{
    static class DefaultPropertyBagInitializer
    {
        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void Initialize()
        {
            PropertyBagStore.AddPropertyBag(new AnimationCurvePropertyBag());
            PropertyBagStore.AddPropertyBag(new KeyFramePropertyBag());
            PropertyBagStore.AddPropertyBag(new Vector2IntPropertyBag());
            PropertyBagStore.AddPropertyBag(new Vector3IntPropertyBag());
            PropertyBagStore.AddPropertyBag(new RectPropertyBag());
            PropertyBagStore.AddPropertyBag(new RectIntPropertyBag());
            PropertyBagStore.AddPropertyBag(new BoundsPropertyBag());
            PropertyBagStore.AddPropertyBag(new BoundsIntPropertyBag());
            PropertyBagStore.AddPropertyBag(new SystemVersionPropertyBag());
        }
    }

    class AnimationCurvePropertyBag : ContainerPropertyBag<AnimationCurve>
    {
        public AnimationCurvePropertyBag()
        {
            PropertyBag.RegisterArray<AnimationCurve, Keyframe>();
            
            AddProperty(new PreWrapModeProperty());
            AddProperty(new PostWrapModeProperty());
            AddProperty(new KeysProperty());
        }

        class PreWrapModeProperty : Property<AnimationCurve, WrapMode>
        {
            public override string Name => nameof(AnimationCurve.preWrapMode);
            public override bool IsReadOnly => false;
            public override WrapMode GetValue(ref AnimationCurve container) => container.preWrapMode;
            public override void SetValue(ref AnimationCurve container, WrapMode value) => container.preWrapMode = value;
        }

        class PostWrapModeProperty : Property<AnimationCurve, WrapMode>
        {
            public override string Name => nameof(AnimationCurve.postWrapMode);
            public override bool IsReadOnly => false;
            public override WrapMode GetValue(ref AnimationCurve container) => container.postWrapMode;
            public override void SetValue(ref AnimationCurve container, WrapMode value) => container.postWrapMode = value;
        }

        class KeysProperty : Property<AnimationCurve, Keyframe[]>
        {
            public override string Name => nameof(AnimationCurve.keys);
            public override bool IsReadOnly => false;
            public override Keyframe[] GetValue(ref AnimationCurve container) => container.keys;
            public override void SetValue(ref AnimationCurve container, Keyframe[] value) => container.keys = value;
        }
    }
    
    class KeyFramePropertyBag : ContainerPropertyBag<Keyframe>
    {
        public KeyFramePropertyBag()
        {
            AddProperty(new TimeProperty());
            AddProperty(new ValueProperty());
            AddProperty(new InTangentProperty());
            AddProperty(new OutTangentProperty());
            AddProperty(new InWeightProperty());
            AddProperty(new OutWeightProperty());
            AddProperty(new WeightedModeProperty());
        }

        class TimeProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.time);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.time;
            public override void SetValue(ref Keyframe container, float value) => container.time = value;
        }

        class ValueProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.value);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.value;
            public override void SetValue(ref Keyframe container, float value) => container.value = value;
        }

        class InTangentProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.inTangent);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.inTangent;
            public override void SetValue(ref Keyframe container, float value) => container.inTangent = value;
        }

        class OutTangentProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.outTangent);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.outTangent;
            public override void SetValue(ref Keyframe container, float value) => container.outTangent = value;
        }

        class InWeightProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.inWeight);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.inWeight;
            public override void SetValue(ref Keyframe container, float value) => container.inWeight = value;
        }

        class OutWeightProperty : Property<Keyframe, float>
        {
            public override string Name => nameof(Keyframe.outWeight);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Keyframe container) => container.outWeight;
            public override void SetValue(ref Keyframe container, float value) => container.outWeight = value;
        }

        class WeightedModeProperty : Property<Keyframe, WeightedMode>
        {
            public override string Name => nameof(Keyframe.weightedMode);
            public override bool IsReadOnly => false;
            public override WeightedMode GetValue(ref Keyframe container) => container.weightedMode;
            public override void SetValue(ref Keyframe container, WeightedMode value) => container.weightedMode = value;
        }
    }
    
    class Vector2IntPropertyBag : ContainerPropertyBag<Vector2Int>
    {
        public Vector2IntPropertyBag()
        {
            AddProperty(new XProperty());
            AddProperty(new YProperty());
        }
        
        class XProperty : Property<Vector2Int, int>
        {
            public override string Name => nameof(Vector2Int.x);
            public override bool IsReadOnly => false;
            public override int GetValue(ref Vector2Int container) => container.x;
            public override void SetValue(ref Vector2Int container, int value) => container.x = value;
        }
        
        class YProperty : Property<Vector2Int, int>
        {
            public override string Name => nameof(Vector2Int.y);
            public override bool IsReadOnly => false;
            public override int GetValue(ref Vector2Int container) => container.y;
            public override void SetValue(ref Vector2Int container, int value) => container.y = value;
        }
    }

    class Vector3IntPropertyBag : ContainerPropertyBag<Vector3Int>
    {
        public Vector3IntPropertyBag()
        {
            AddProperty(new XProperty());
            AddProperty(new YProperty());
            AddProperty(new ZProperty());
        }
        
        class XProperty : Property<Vector3Int, int>
        {
            public override string Name => nameof(Vector3Int.x);
            public override bool IsReadOnly => false;
            public override int GetValue(ref Vector3Int container) => container.x;
            public override void SetValue(ref Vector3Int container, int value) => container.x = value;
        }
        
        class YProperty : Property<Vector3Int, int>
        {
            public override string Name => nameof(Vector3Int.y);
            public override bool IsReadOnly => false;
            public override int GetValue(ref Vector3Int container) => container.y;
            public override void SetValue(ref Vector3Int container, int value) => container.y = value;
        }
        
        class ZProperty : Property<Vector3Int, int>
        {
            public override string Name => nameof(Vector3Int.z);
            public override bool IsReadOnly => false;
            public override int GetValue(ref Vector3Int container) => container.z;
            public override void SetValue(ref Vector3Int container, int value) => container.z = value;
        }
    }

    class RectPropertyBag : ContainerPropertyBag<Rect>
    {
        public RectPropertyBag()
        {
            AddProperty(new XProperty());
            AddProperty(new YProperty());
            AddProperty(new WidthProperty());
            AddProperty(new HeightProperty());
        }
        
        class XProperty : Property<Rect, float>
        {
            public override string Name => nameof(Rect.x);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Rect container) => container.x;
            public override void SetValue(ref Rect container, float value) => container.x = value;
        }
        
        class YProperty : Property<Rect, float>
        {
            public override string Name => nameof(Rect.y);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Rect container) => container.y;
            public override void SetValue(ref Rect container, float value) => container.y = value;
        }
        
        class WidthProperty : Property<Rect, float>
        {
            public override string Name => nameof(Rect.width);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Rect container) => container.width;
            public override void SetValue(ref Rect container, float value) => container.width = value;
        }
        
        class HeightProperty : Property<Rect, float>
        {
            public override string Name => nameof(Rect.height);
            public override bool IsReadOnly => false;
            public override float GetValue(ref Rect container) => container.height;
            public override void SetValue(ref Rect container, float value) => container.height = value;
        }
    }
    
    class RectIntPropertyBag : ContainerPropertyBag<RectInt>
    {
        public RectIntPropertyBag()
        {
            AddProperty(new XProperty());
            AddProperty(new YProperty());
            AddProperty(new WidthProperty());
            AddProperty(new HeightProperty());
        }
        
        class XProperty : Property<RectInt, int>
        {
            public override string Name => nameof(RectInt.x);
            public override bool IsReadOnly => false;
            public override int GetValue(ref RectInt container) => container.x;
            public override void SetValue(ref RectInt container, int value) => container.x = value;
        }
        
        class YProperty : Property<RectInt, int>
        {
            public override string Name => nameof(RectInt.y);
            public override bool IsReadOnly => false;
            public override int GetValue(ref RectInt container) => container.y;
            public override void SetValue(ref RectInt container, int value) => container.y = value;
        }
        
        class WidthProperty : Property<RectInt, int>
        {
            public override string Name => nameof(RectInt.width);
            public override bool IsReadOnly => false;
            public override int GetValue(ref RectInt container) => container.width;
            public override void SetValue(ref RectInt container, int value) => container.width = value;
        }
        
        class HeightProperty : Property<RectInt, int>
        {
            public override string Name => nameof(RectInt.height);
            public override bool IsReadOnly => false;
            public override int GetValue(ref RectInt container) => container.height;
            public override void SetValue(ref RectInt container, int value) => container.height = value;
        }
    }
    
    class BoundsPropertyBag : ContainerPropertyBag<Bounds>
    {
        public BoundsPropertyBag()
        {
            AddProperty(new CenterProperty());
            AddProperty(new ExtentsProperty());
        }
        
        class CenterProperty : Property<Bounds, Vector3>
        {
            public override string Name => nameof(Bounds.center);
            public override bool IsReadOnly => false;
            public override Vector3 GetValue(ref Bounds container) => container.center;
            public override void SetValue(ref Bounds container, Vector3 value) => container.center = value;
        }
        
        class ExtentsProperty : Property<Bounds, Vector3>
        {
            public override string Name => nameof(Bounds.extents);
            public override bool IsReadOnly => false;
            public override Vector3 GetValue(ref Bounds container) => container.extents;
            public override void SetValue(ref Bounds container, Vector3 value) => container.extents = value;
        }
    }
    
    class BoundsIntPropertyBag : ContainerPropertyBag<BoundsInt>
    {
        public BoundsIntPropertyBag()
        {
            AddProperty(new PositionProperty());
            AddProperty(new SizeProperty());
        }
        
        class PositionProperty : Property<BoundsInt, Vector3Int>
        {
            public override string Name => nameof(BoundsInt.position);
            public override bool IsReadOnly => false;
            public override Vector3Int GetValue(ref BoundsInt container) => container.position;
            public override void SetValue(ref BoundsInt container, Vector3Int value) => container.position = value;
        }
        
        class SizeProperty : Property<BoundsInt, Vector3Int>
        {
            public override string Name => nameof(BoundsInt.size);
            public override bool IsReadOnly => false;
            public override Vector3Int GetValue(ref BoundsInt container) => container.size;
            public override void SetValue(ref BoundsInt container, Vector3Int value) => container.size = value;
        }
    }
    
    class SystemVersionPropertyBag : ContainerPropertyBag<System.Version>
    {
        public SystemVersionPropertyBag()
        {
            AddProperty(new MajorProperty().WithAttribute(new MinAttribute(0)));
            AddProperty(new MinorProperty().WithAttribute(new MinAttribute(0)));
            AddProperty(new BuildProperty().WithAttribute(new MinAttribute(0)));
            AddProperty(new RevisionProperty().WithAttribute(new MinAttribute(0)));
        }
        
        class MajorProperty : Property<System.Version, int>
        {
            public override string Name => nameof(System.Version.Major);
            public override bool IsReadOnly => true;
            public override int GetValue(ref System.Version container) => container.Major;
            public override void SetValue(ref System.Version container, int value) {}
        }
        
        class MinorProperty : Property<System.Version, int>
        {
            public override string Name => nameof(System.Version.Minor);
            public override bool IsReadOnly => true;
            public override int GetValue(ref System.Version container) => container.Minor;
            public override void SetValue(ref System.Version container, int value) {}
        }
        
        class BuildProperty : Property<System.Version, int>
        {
            public override string Name => nameof(System.Version.Build);
            public override bool IsReadOnly => true;
            public override int GetValue(ref System.Version container) => container.Build;
            public override void SetValue(ref System.Version container, int value) {}
        }
        
        class RevisionProperty : Property<System.Version, int>
        {
            public override string Name => nameof(System.Version.Revision);
            public override bool IsReadOnly => true;
            public override int GetValue(ref System.Version container) => container.Revision;
            public override void SetValue(ref System.Version container, int value) {}
        }
    }
}
#endif