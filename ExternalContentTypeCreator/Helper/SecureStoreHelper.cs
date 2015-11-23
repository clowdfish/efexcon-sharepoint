using System;
using System.Data;
using Microsoft.BusinessData.Infrastructure.SecureStore;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.SharePoint.BusinessData.Administration;

namespace EFEXCON.ExternalLookup.Helper
{
    /// <summary>
    /// Class Creator.
    /// </summary>
    public class SecureStoreHelper
    {
        private readonly string _sssId;
        private readonly string _providerImplementation;

        public SecureStoreHelper(string sssId, string providerImplementation)
        {
            _sssId = sssId;

            // "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"
            _providerImplementation = providerImplementation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <returns></returns>
        public static Credentials GetCredentialsFromLobSystem(LobSystem lobSystem)
        {
            var sssId = "";
            var providerimplementation = "";

            foreach (Property prop in SqlHelper.GetLobSystemInstanceProperties(lobSystem))
            {
                if (prop.Name == "SsoApplicationId")
                    sssId = prop.Value.ToString();

                if (prop.Name == "SsoProviderImplementation")
                    providerimplementation = prop.Value.ToString();
            }

            if (String.IsNullOrEmpty(sssId))
                throw new Exception("Secure Store Application ID can not be identified.");

            if (String.IsNullOrEmpty(providerimplementation))
                throw new Exception("Provider implementation for Secure Store Service can not be identified.");

            var credentials = new SecureStoreHelper(sssId, providerimplementation).GetCredentials();

            if (credentials == null)
                throw new NoNullAllowedException("Credentials could not be retrieved from Secure Store Service.");

            return credentials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Credentials GetCredentials()
        {
            Credentials userCredentials = new Credentials();

            ISecureStoreProvider provider = GetSecureStoreProvider();

            // Get the credentials for the user on whose behalf the code 
            // is executing. 
            using (SecureStoreCredentialCollection credentials =
                provider.GetRestrictedCredentials(_sssId))
            {
                SecureString secureUsername = null;
                SecureString securePassword = null;

                // Look for username and password in credentials. 
                foreach (ISecureStoreCredential credential in credentials)
                {
                    switch (credential.CredentialType)
                    {
                        case SecureStoreCredentialType.UserName:
                        case SecureStoreCredentialType.WindowsUserName:
                            secureUsername = credential.Credential;
                            break;
                        case SecureStoreCredentialType.Password:
                        case SecureStoreCredentialType.WindowsPassword:
                            securePassword = credential.Credential;
                            break;
                    }
                }

                // Username and password have been read. 
                if (secureUsername != null && securePassword != null)
                {
                    var loginName = SecureStringToString(secureUsername);

                    if (!loginName.Contains("\\"))
                        throw new FormatException("Login name does not include domain information.");

                    var userArray = loginName.Split('\\');

                    var domain = userArray[0];
                    var username = userArray[1];
                    var password = SecureStringToString(securePassword);

                    userCredentials.Domain = domain;
                    userCredentials.User = username;
                    userCredentials.Password = password;
                }
            }

            return userCredentials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ISecureStoreProvider GetSecureStoreProvider()
        {
            Type providerType = Type.GetType(_providerImplementation);

            if(providerType == null)
                throw new NoNullAllowedException("Provider type of secure store provider cannot be identified.");

            return Activator.CreateInstance(providerType)
                as ISecureStoreProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }

    public class Credentials
    {
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
 }
  