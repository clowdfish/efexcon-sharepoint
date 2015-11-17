﻿using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.BusinessData.MetadataModel;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;

    public partial class DataSources : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

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
            // check out http://stackoverflow.com/questions/8243008/format-of-the-initialization-string-does-not-conform-to-specification-starting-a

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
                    listLobSystems();
                }
                else
                {
                    Status.InnerHtml = "Could not create data source.";
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

        /// <summary>
        /// 
        /// </summary>
        protected void listLobSystems()
        {
            DataSourceContainer.InnerHtml = "";

            int counter = 0;
            foreach (var lobSystem in Creator.listAllLobSystems())
            {
                var separator = new LiteralControl("<div></div>");

                var label = new Label();
                label.Text = lobSystem.Name + " ";
                DataSourceContainer.Controls.Add(label);

                var link = new LinkButton
                {
                    ID = "delete_" + lobSystem.Name,
                    CommandArgument = lobSystem.Name,
                    Text = "delete"
                };
                link.Command += deleteLobSystem;
                DataSourceContainer.Controls.Add(link);

                DataSourceContainer.Controls.Add(separator);
                counter++;
            }

            if (counter == 0)
            {
                DataSourceContainer.InnerHtml = "No LobSystem available.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void deleteLobSystem(object sender, CommandEventArgs e)
        {
            string lobName = e.CommandArgument.ToString();
            var deleted = Creator.deleteLobSystem(lobName, SystemType.Database);

            if(deleted)
            {                
                listLobSystems();
            }
            else
            {
                Status.InnerHtml = "LobSystem could not be deleted.";
            }
        }
    }
}
