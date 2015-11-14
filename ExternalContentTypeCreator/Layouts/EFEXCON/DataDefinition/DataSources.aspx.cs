using System;
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

        protected void saveDataSource(object sender, EventArgs e)
        {
            string title = Request.Form["title"];
            string type = Request.Form["dataType"];
            string url = Request.Form["connectionString"];
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            Status.InnerHtml = "<span style='color:red;'>Not yet implemented.</span>";
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
