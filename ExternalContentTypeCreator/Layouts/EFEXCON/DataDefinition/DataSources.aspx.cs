using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.BusinessData.MetadataModel;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using EFEXCON.ExternalLookup.Core;

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
            string url = Request.Form["url"];
            string database = Request.Form["database"];
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            // the connection string must be conform to
            // Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;

            // TODO Check url format

            var connectionString = 
                String.Format("Server={0};Database={1};User Id={2};Password={3}", 
                    url, database, username, password);

            Status.InnerHtml = testDataConnection(connectionString);

            // TODO if test was successful add new LobSystem and LobSystemInstance
            // TODO set status and refresh view

            //Status.InnerHtml = "<span style='color:red;'>Not yet implemented.</span>";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected String testDataConnection(string connectionString)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SHOW TABLES;", cn);
                    cn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    rdr.Read();

                    return rdr[0].ToString(); //read a value
                }
            }
            catch(Exception e)
            {
                return "Could not create a connection to the data source.";
            }
        }

        protected void showAvailableECT(object sender, EventArgs e) 
        {
            Status.InnerHtml = Creator.getAllExternalContentTypes();
        }

        protected void listLobSystems()
        {
            DataSources.InnerHtml = Creator.listAllLobSystems();
        }

        protected void createLobSystem(object sender, EventArgs e)
        {
            Creator.createLobSystem("EFEXCON", SystemType.Database);
            listLobSystems();
        }

        protected void deleteLobSystem(object sender, EventArgs e)
        {
            var deleted = Creator.deleteLobSystem("EFEXCON", SystemType.Database);

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
