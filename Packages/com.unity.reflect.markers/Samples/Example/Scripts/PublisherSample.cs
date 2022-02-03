using System.Collections.Generic;
using System.IO;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Examples
{
    public class UnityUserData
    {
        public string AccessToken { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
    }

    public class UnityProjectData
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    public class PublisherSample : MonoBehaviour
    {
        [SerializeField] private string importPath = "C:/Import";

        private MarkerPublisher publisher = null;

        // Start is called before the first frame update
        private void Start()
        {
            List<SyncMarker> markers = new List<SyncMarker>();
            //Create an updated sync marker
            SyncMarker marker = new SyncMarker(new SyncId("Marker"), "VLMarker1")
            {
                Transform = new SyncTransform(
                    new System.Numerics.Vector3(25, 50, 0),
                    System.Numerics.Quaternion.CreateFromYawPitchRoll(0, 0, 0),
                    System.Numerics.Vector3.One)
            };
            markers.Add(marker);
            LoginToPublisher();
            publisher.PerformUpdate(markers);
        }

        private void LoginToPublisher()
        {
            //Create a local sync service project.
            string projectStr = File.ReadAllText($"{importPath}/project.json");
            UnityProjectData projectData = JsonUtility.FromJson<UnityProjectData>(projectStr);
            UnityProject project = new UnityProject(projectData.ProjectId, projectData.ProjectName);

            //Create a Unity User
            string userStr = File.ReadAllText($"{importPath}/user.json");
            UnityUserData userData = JsonUtility.FromJson<UnityUserData>(userStr);
            UnityUser user = new UnityUser(userData.AccessToken, userData.DisplayName, userData.UserId);

            publisher = new MarkerPublisher();
            publisher.UpdateProject(project, user);
        }
    }
}
