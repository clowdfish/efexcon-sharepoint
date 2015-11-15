using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Microsoft.BusinessData.MetadataModel;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using EFEXCON.ExternalLookup.Core;
    using Microsoft.SharePoint.BusinessData.Administration;
    using System.Collections.Generic;
    using System.Web.UI;

    public partial class Settings : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            listLobSystems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void saveDataSource(object sender, EventArgs e)
        {
            string title = Request.Form["title"];
            string type = Request.Form["dataType"];
            string server = Request.Form["url"];
            string database = Request.Form["database"];
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            // the connection string must be conform to
            // Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;

            // TODO Check server format

            var connectionString = 
                String.Format("Server={0};Database={1};User Id={2};Password={3};", 
                    server, database, username, password);

            if(connectionStringIsValid(connectionString))
            {
                var lobSystem = Creator.createLobSystem(title, SystemType.Database);
                var lobSystemInstance = Creator.createLobSystemInstance(lobSystem, server, database, username, password);

                if(lobSystem != null && lobSystemInstance != null)
                {
                    Status.InnerHtml = "LobSystem and LobSystemInstance created.";
                    listLobSystems();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected Boolean connectionStringIsValid(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch(Exception e)
            {
                Status.InnerHtml = "Could not create a connection to the data source: " + e.ToString();
                return false; 
            }
        }

        protected void showAvailableECT(object sender, EventArgs e) 
        {
            Status.InnerHtml = Creator.getAllExternalContentTypes();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void listLobSystems()
        {
            DataSources.InnerHtml = "";

            foreach (var lobSystem in Creator.listAllLobSystems())
            {
                var separator = new LiteralControl("<div></div>");

                var label = new Label();
                label.Text = lobSystem.Name + " ";
                DataSources.Controls.Add(label);

                var link = new LinkButton
                {
                    ID = "delete_" + lobSystem.Name,
                    CommandArgument = lobSystem.Name,
                    Text = "delete"
                };
                link.Command += deleteLobSystem;
                DataSources.Controls.Add(link);

                DataSources.Controls.Add(separator);
            }
        }

        protected void deleteLobSystem(object sender, CommandEventArgs e)
        {
            string lobName = e.CommandArgument.ToString();
            var deleted = Creator.deleteLobSystem(lobName, SystemType.Database);

            if(deleted)
            {
                Status.InnerHtml = "LobSystem was deleted.";
                listLobSystems();
            }
            else
            {
                Status.InnerHtml = "LobSystem could not be deleted.";
            }
        }
    }
}
