using NUnit.Framework;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;
using VivoxUnity;

namespace VivoxTests
{   
    public class VivoxAPITests
    {
        private const float TIMEOUT_INTERVAL = 5.0f;
        private const string TEST_ACCOUNT_NAME = "TestAccount";
        private const string TEST_CHANNEL_NAME = "TestChannel";

        private Uri _server = new Uri("https://GETFROMPORTAL.www.vivox.com/api2");
        private string _domain = "GET VALUE FROM VIVOX DEVELOPER PORTAL";
        private string _tokenIssuer = "GET VALUE FROM VIVOX DEVELOPER PORTAL";
        private string _tokenKey = "GET VALUE FROM VIVOX DEVELOPER PORTAL";

        private TimeSpan _tokenExpiration = TimeSpan.FromSeconds(90);
        private string testMessage = string.Empty;

        [UnityTest]
        [Ignore("Deployed to public repo, dont add the credentials")]
        public IEnumerator SentReceivedMessageComparison()
        {
            CheckCredentials();

            // Initialize the client.
            Client _client = new Client();
            _client.Initialize();

            float timeout;
            IAsyncResult waitHandle;

            // Login.
            bool isLoggedIn = false;
            string uniqueId = Guid.NewGuid().ToString();
            AccountId _accountId = new AccountId(_tokenIssuer, uniqueId, _domain, TEST_ACCOUNT_NAME);
            ILoginSession LoginSession = _client.GetLoginSession(_accountId);
            waitHandle = LoginSession.BeginLogin(_server, LoginSession.GetLoginToken(_tokenKey, _tokenExpiration), SubscriptionMode.Accept, null, null, null, ar =>
            {
                try
                {
                    isLoggedIn = true;
                    LoginSession.EndLogin(ar);
                }
                catch (Exception e)
                {
                    Assert.Fail($"BeginLogin failed: {e}");
                    return;
                }
            });
            timeout = Time.time + TIMEOUT_INTERVAL;
            yield return new WaitUntil(() => waitHandle.IsCompleted && LoginSession.State == LoginState.LoggedIn || Time.time > timeout);
            Assert.IsTrue(isLoggedIn, "Failed to login.");

            // Join a channel with audio and text enabled.
            ChannelId channelId = new ChannelId(_tokenIssuer, TEST_CHANNEL_NAME, _domain);
            IChannelSession channelSession = LoginSession.GetChannelSession(channelId);
            channelSession.MessageLog.AfterItemAdded += OnMessageLogRecieved;
            bool isInChannel = false;
            waitHandle = channelSession.BeginConnect(true, true, true, channelSession.GetConnectToken(_tokenKey, _tokenExpiration), ar =>
            {
                try
                {
                    isInChannel = true;
                    channelSession.EndConnect(ar);
                }
                catch (Exception e)
                {
                    Assert.Fail($"BeginConnect failed: {e}");
                    return;
                }
            });
            timeout = Time.time + TIMEOUT_INTERVAL;
            yield return new WaitUntil(() => waitHandle.IsCompleted && channelSession.TextState == ConnectionState.Connected && channelSession.AudioState == ConnectionState.Connected || Time.time > timeout);
            Assert.IsTrue(isInChannel, "Failed to join the specified channel.");

            // Send a message to the channel.
            string textMessage = "hello 😉";
            waitHandle = channelSession.BeginSendText(textMessage, ar =>
            {
                try
                {
                    channelSession.EndSendText(ar);
                }
                catch (Exception e)
                {
                    Assert.Fail($"BeginSendText failed: {e}");
                }
            });
            timeout = Time.time + TIMEOUT_INTERVAL;
            yield return new WaitUntil(() => waitHandle.IsCompleted || Time.time > timeout);

            // Make sure the sent and received messages are the same.
            var originalMsg = Encoding.UTF8.GetBytes(textMessage);
            var receivedMsg = Encoding.UTF8.GetBytes(testMessage);
            // Ensure that the same amount of bytes exist for the sent and received messages.
            Assert.IsTrue(originalMsg.Length == receivedMsg.Length, "Sent and received messages should have the same length but they do not.");
            // Make sure each byte matches between the sent and received messages.
            for (int i = 0; i < originalMsg.Length; i++)
            {
                Assert.IsTrue(originalMsg[i] == receivedMsg[i], "Comparison of sent and received message failed. They should be the same but are not.");
            }

            _client.Uninitialize();

            GameObject.Destroy(GameObject.FindObjectOfType<VxUnityInterop>());
        }

        private void OnMessageLogRecieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
        {
            testMessage = textMessage.Value.Message;
        }

        private void CheckCredentials()
        {
            if (_server.ToString() == "https://GETFROMPORTAL.www.vivox.com/api2" ||
                _domain == "GET VALUE FROM VIVOX DEVELOPER PORTAL" ||
                _tokenKey == "GET VALUE FROM VIVOX DEVELOPER PORTAL" ||
                _tokenIssuer == "GET VALUE FROM VIVOX DEVELOPER PORTAL")
            {
                Assert.Fail("The default VivoxVoiceServer values(Server, Domain, TokenIssuer, and TokenKey) must be replaced with application specific issuer and key values from your developer account.");
            }

        }
    }
}
