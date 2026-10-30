using Unity.Services.Multiplayer;
using UnityEngine;
using System.Threading.Tasks;
using Citlali.Config;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using Unity.Netcode;

namespace Citlali.Networking
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public string Status { get; private set; } = "";
        public bool IsBusy { get; private set; }
        public bool InSession => session != null;
        public string JoinCode => session != null ? session.Code : "";

        ISession session;
        float startedAt;

        void Awake()
        {
            Instance = this;
        }

        public async void Host()
        {
            await Run("Creating game....", async() =>
            {
                await SignIn();
                var options = new SessionOptions { MaxPlayers = GameConfig.MaxPlayer }.WithRelayNetwork();
                session = await MultiplayerService.Instance.CreateSessionAsync(options);
            });
        }

        public async void Join(string code)
        {
            code = code.Trim().ToUpperInvariant();
            if (code.Length == 0) { Status = "Enter a join code"; return; }

            await Run("Joining....", async() =>
            {
                await SignIn();
                session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
            });
        }

        public async void Leave()
        {
            await Run("Leaving....", async() =>
            {
                if (session is IHostSession host) await host.DeleteAsync();
                else if (session != null) await session.LeaveAsync();
                ClearSession();
            });
        }

        async Task SignIn()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                var options = new InitializationOptions();
                
                options.SetProfile(Guid.NewGuid().ToString("N").Substring(0, 12));
                await UnityServices.InitializeAsync(options);
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        async Task Run(string status, Func<Task> action)
        {
            if (IsBusy) return;
            IsBusy = true;
            Status = status;
            try
            {
                await action();
                Status = "";
                startedAt = Time.unscaledTime;
            }
            catch (Exception e)
            {
                Status = e.Message;
                Debug.LogException(e);
                ClearSession();
            }
            finally { IsBusy = false; }
        }

        void ClearSession()
        {
            session = null;
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening) nm.Shutdown();
        }

        void Update()
        {
            var nm = NetworkManager.Singleton;
            if (session == null || IsBusy || nm == null) return;
            if (Time.unscaledTime - startedAt > 3f && !nm.IsListening)
            {
                session = null;
                Status = "Disconnected.";
            }
        }
    }
}