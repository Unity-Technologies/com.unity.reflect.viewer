using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using Unity.Reflect.Runtime;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ProjectSettingStateData : IEquatable<ProjectSettingStateData>, IProjectDataProvider<Project>
    {
        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public AccessToken accessToken { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public Project activeProject { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public Sprite activeProjectThumbnail { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public string url { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public string loadSceneName { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public string unloadSceneName { get; set; }

        public override string ToString()
        {
            return ToString("(Project {0} url {1} loadScene {2})");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)this.activeProject,
                url, loadSceneName);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProjectSettingStateData))
                return false;
            return Equals((ProjectSettingStateData)obj);
        }

        public bool Equals(ProjectSettingStateData other)
        {
            bool isEqual = true;
            isEqual &= Equals(accessToken, other.accessToken);
            isEqual &= Equals(activeProjectThumbnail, other.activeProjectThumbnail);
            isEqual &= Equals(activeProject, other.activeProject);
            isEqual &= url == other.url;
            isEqual &= loadSceneName == other.loadSceneName;
            isEqual &= unloadSceneName == other.unloadSceneName;

            return isEqual;
        }

        public static bool operator ==(ProjectSettingStateData a, ProjectSettingStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProjectSettingStateData a, ProjectSettingStateData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = activeProject != null ? activeProject.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ url.GetHashCode();
                hashCode = (hashCode * 397) ^ loadSceneName.GetHashCode();
                hashCode = (hashCode * 397) ^ activeProjectThumbnail.GetHashCode();
                hashCode = accessToken != null ? (hashCode * 397) ^ accessToken.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ unloadSceneName.GetHashCode();
                return hashCode;
            }
        }
    }

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginSettingStateData : IEquatable<LoginSettingStateData>, ILoginSettingDataProvider
    {
        EnvironmentInfo m_EnvironmentInfo;
        object m_EnvironmentInfoObject;

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public object environmentInfo
        {
            get => m_EnvironmentInfoObject;
            set
            {
                m_EnvironmentInfoObject = value;
                try
                {
                    m_EnvironmentInfo = m_EnvironmentInfoObject != null ? (EnvironmentInfo)value : default(EnvironmentInfo);
                }
                catch (InvalidCastException e)
                {
                    throw new ArgumentException("LoginSettingStateData.environmentInfo must be a value of EnvironmentInfo type or null", e);
                }
            }
        }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public bool deleteCloudEnvironmentSetting { get; set; }

        public override string ToString()
        {
            return ToString("(EnvironmentInfo {0})");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)m_EnvironmentInfo);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LoginSettingStateData))
                return false;
            return Equals((LoginSettingStateData)obj);
        }

        public bool Equals(LoginSettingStateData other)
        {
            bool isEqual = true;
            isEqual &= Equals(m_EnvironmentInfo, other.m_EnvironmentInfo);
            isEqual &= Equals(deleteCloudEnvironmentSetting, other.deleteCloudEnvironmentSetting);

            return isEqual;
        }

        public static bool operator ==(LoginSettingStateData a, LoginSettingStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LoginSettingStateData a, LoginSettingStateData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_EnvironmentInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ deleteCloudEnvironmentSetting.GetHashCode();
                return hashCode;
            }
        }
    }
}
