using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Reflect.Model;
using Unity.Reflect.Source.Utils.Errors;
using UnityEngine;

namespace Unity.Reflect.Markers
{
    public class MarkerPublisher : IDisposable
    {
        IPublisherClient m_PublisherClient = null;
        PublisherSettings m_PublisherSettings;

        public MarkerPublisher()
        {
        }

        ~MarkerPublisher()
        {
            Dispose();
        }

        public void UpdateProject(UnityProject project, UnityUser user)
        {
            if (m_PublisherClient != null)
                m_PublisherClient.CloseAndWait();
            try
            {
                m_PublisherClient = CreatePublisher(project, user);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                m_PublisherClient?.CloseAndWait();
            }

            if (m_PublisherClient == null)
            {
                Debug.LogError("Publisher failed to open");
                throw new NullReferenceException("Publisher Missing");
            }
            if (!m_PublisherClient.IsTypeSendable<SyncMarker>())
            {
                Debug.LogError("SyncMarkers not supported");
                throw new SyncModelNotSupportedException("SyncMarkers not supported by the project host");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            if (m_PublisherClient != null)
            {
                m_PublisherClient.CloseAndWait();
            }
        }

        private IPublisherClient CreatePublisher(UnityProject project, UnityUser user)
        {
            // This is the public name of the Source Project you want to export (it doesn't have to be unique).
            string sourceName = $"{project.Name}_markers";
            // This identifies the Source Project you want to export and must be unique and persistent over multiple publishing sessions.
            string baseIdString = $"{project.ProjectId}_markers";
            string sourceId;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(baseIdString));
                sourceId = new Guid(hash).ToString();
            }

            var version = Regex.Replace(Application.version, "[^0-9.]", "");
            //Create the publisher settings for the client
            m_PublisherSettings = new PublisherSettings(project, user)
            {
                PluginName = Application.productName + " MarkerPublisher",
                //TODO: Use Reflect DLL version
                PluginVersion = Version.Parse(version),
                LengthUnit = LengthUnit.Meters,
                AxisInversion = AxisInversion.None
            };

            // Create a Publisher Client, that will allow us to publish data into the selected Unity Project.
            return Publisher.OpenClient(sourceName, sourceId, m_PublisherSettings, false);
        }

        public void PerformUpdate(List<SyncMarker> markers)
        {
            // Start a transaction and attach it to the publisher client.
            // Note that the publisher client can only be attached to one transaction at a time.
            PublisherTransaction transaction = m_PublisherClient.StartTransaction();

            for(int i = 0; i < markers.Count; i++)
            {
                 //Send the update
                 transaction.Send(markers[i]);
            }

            //Finish the transaction
            transaction.Commit();

            //The publisher is finished
            transaction.Dispose();
        }

        public async Task PerformUpdateAsync(List<SyncMarker> markers)
        {
            // Start a transaction and attach it to the publisher client.
            // Note that the publisher client can only be attached to one transaction at a time.
            PublisherTransaction transaction = m_PublisherClient.StartTransaction();

            for(int i = 0; i < markers.Count; i++)
            {
                //Send the update
                transaction.Send(markers[i]);
            }

            //Finish the transaction
            await transaction.CommitAsync();

            //The publisher is finished
            transaction.Dispose();
        }

        public void Delete(List<SyncId> deletedMarkers, List<SyncMarker> keptMarkers)
        {
            var transaction = m_PublisherClient.StartTransaction();
            for(int i = 0; i < deletedMarkers.Count; i++)
            {
                transaction.RemoveObjectInstance(deletedMarkers[i]);
            }
            for(int i = 0; i < keptMarkers.Count; i++)
            {
                transaction.Send(keptMarkers[i]);
            }
            transaction.Commit();
            transaction.Dispose();
        }

        public async Task DeleteAsync(List<SyncId> deletedMarkers, List<SyncMarker> keptMarkers)
        {
            var transaction = m_PublisherClient.StartTransaction();
            for(int i = 0; i < deletedMarkers.Count; i++)
            {
                transaction.RemoveObjectInstance(deletedMarkers[i]);
            }
            for(int i = 0; i < keptMarkers.Count; i++)
            {
                transaction.Send(keptMarkers[i]);
            }
            await transaction.CommitAsync();
            transaction.Dispose();
        }
    }
}
