using System;
using System.Data;
using Microsoft.BusinessData.Infrastructure.SecureStore;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

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
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;

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
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_SecureStore", "Resources", language);
                throw new Exception(message);
            }

            if (String.IsNullOrEmpty(providerimplementation))
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_Provider", "Resources", language);
                throw new Exception(message);
            }

            var credentials = new SecureStoreHelper(sssId, providerimplementation).GetCredentials();

            if (credentials == null)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_Credentials", "Resources", language);
                throw new NoNullAllowedException(message);
            }

            return credentials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Credentials GetCredentials()
        {
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;
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
                    {
                        var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_DomainMissing", "Resources", language);
                        throw new FormatException(message);
                    }

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
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;
            Type providerType = Type.GetType(_providerImplementation);

            if (providerType == null)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_ProviderType", "Resources", language);
                throw new NoNullAllowedException(message);
            }

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
  