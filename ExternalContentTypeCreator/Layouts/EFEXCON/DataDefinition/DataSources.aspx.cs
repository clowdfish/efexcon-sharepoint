using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.BusinessData.MetadataModel;
using EFEXCON.ExternalLookup.Helper;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;

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
                Status.InnerHtml = HtmlHelper.CreateErrorString("Could not get credentials from Secure Store Service.", ex);
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
                        Status.InnerHtml = HtmlHelper.CreateErrorString("Could not create data source.", null);
                    }
                }
                catch(Exception ex)
                {
                    Status.InnerHtml = HtmlHelper.CreateErrorString("The data source could not be created. Do you have the right permissions?", ex);
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
                Status.InnerHtml = HtmlHelper.CreateErrorString("Could not create a connection to the data source." , ex);
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
                    DataSourceContainer.InnerHtml = "No data source configured.";
                }
            }
            catch(Exception ex)
            {
                Status.InnerHtml = HtmlHelper.CreateErrorString("Could not access Business Data Connectivity service to list data sources.", ex);
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
                    Status.InnerHtml = HtmlHelper.CreateErrorString("Data source could not be deleted.", null);
                }
            }
            catch(Exception ex)
            {
                Status.InnerHtml = HtmlHelper.CreateErrorString("Data source could not be deleted.", ex);
            }
            
        }
    }
}
