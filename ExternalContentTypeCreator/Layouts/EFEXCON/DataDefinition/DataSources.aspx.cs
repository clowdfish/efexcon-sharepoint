using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.BusinessData.MetadataModel;
using EFEXCON.ExternalLookup.Helper;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;    

    public partial class DataSources : LayoutsPageBase
    {
        protected uint _language = 1033;

        protected void Page_Load(object sender, EventArgs e)
        {
            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

            if (SPContext.Current.Web != null)
                _language =  SPContext.Current.Web.Language;

            ListLobSystems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SaveDataSource(object sender, EventArgs e)
        {
            string title = Request.Form["title"];
            string type = Request.Form["dataType"];
            string server = Request.Form["url"];
            string database = Request.Form["database"];
            string sssId = Request.Form["secureStoreApplicationId"];
            const string providerImplementation = 
                "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, " +
                "Microsoft.Office.SecureStoreService, " +
                "Version=14.0.0.0, Culture=neutral, " +
                "PublicKeyToken=71e9bce111e9429c";

            // the connection string must be conform to
            // Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
            // check out http://stackoverflow.com/questions/8243008/format-of-the-initialization-string-does-not-conform-to-specification-starting-a

            Credentials credentials = null;
            try
            {
                credentials = new SecureStoreHelper(sssId, providerImplementation).GetCredentials();
            }
            catch(Exception ex)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_SecureStore_Credentials", "Resources", _language);
                Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
            }

            var connectionString = 
                String.Format("Server={0};Database={1};Integrated Security=SSPI;",
                server, database);

            if(credentials != null && ConnectionStringIsValid(credentials, connectionString))
            {
                try
                {
                    var lobSystem = Creator.CreateLobSystem(title, SystemType.Database);
                    var lobSystemInstance = Creator.CreateLobSystemInstance(lobSystem, server, database, sssId, providerImplementation);

                    if (lobSystem != null && lobSystemInstance != null)
                    {
                        Status.InnerHtml = "";
                        ListLobSystems();
                    }
                    else
                    {
                        var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Create", "Resources", _language);
                        Status.InnerHtml = HtmlHelper.CreateErrorString(message, null);
                    }
                }
                catch(Exception ex)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Permissions", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected Boolean ConnectionStringIsValid(Credentials credentials, string connectionString)
        {
            try
            {
                using (new Impersonator(credentials.User, credentials.Domain, credentials.Password))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        return connection.State == ConnectionState.Open;
                    }
                }
            }
            catch(Exception ex)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Connection", "Resources", _language);
                Status.InnerHtml = HtmlHelper.CreateErrorString(message , ex);
                return false; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ListLobSystems()
        {
            DataSourceContainer.InnerHtml = "";

            try
            {
                int counter = 0;
                foreach (var lobSystem in Creator.ListAllLobSystems())
                {
                    var separator = new LiteralControl("<div></div>");

                    var label = new Label();
                    label.Text = lobSystem.Name + " ";
                    DataSourceContainer.Controls.Add(label);

                    var link = new LinkButton
                    {
                        ID = "delete_" + lobSystem.Name,
                        CommandArgument = lobSystem.Name,
                        Text = SPUtility.GetLocalizedString("$Resources:ExternalLookup_General_Delete", "Resources", _language)
                    };
                    link.Command += DeleteLobSystem;
                    DataSourceContainer.Controls.Add(link);

                    DataSourceContainer.Controls.Add(separator);
                    counter++;
                }

                if (counter == 0)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_None", "Resources", _language);
                    DataSourceContainer.InnerHtml = message;
                }
            }
            catch(Exception ex)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Listing", "Resources", _language);
                Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DeleteLobSystem(object sender, CommandEventArgs e)
        {
            string lobName = e.CommandArgument.ToString();

            try
            {
                var deleted = Creator.DeleteLobSystem(lobName, SystemType.Database);

                if (deleted)
                {
                    ListLobSystems();
                }
                else
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Delete", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, null);
                }
            }
            catch(Exception ex)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Delete", "Resources", _language);
                Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
            }            
        }
    }
}
