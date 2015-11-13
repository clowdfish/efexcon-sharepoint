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
        }

        protected void showAvailableECT(object sender, EventArgs e) 
        {
            Div1.InnerHtml = Creator.getAllExternalContentTypes();

            // Add to beginning of body area
            //Response.Write("Another Test.");
        }

        protected void listLobSystems(object sender, EventArgs e)
        {
            Div1.InnerHtml = Creator.listAllLobSystems();

            // Add to beginning of body area
            //Response.Write("Another Test.");
        }

        protected void createLobSystem(object sender, EventArgs e)
        {
            Creator.createLobSystem("EFEXCON", SystemType.Database);

            // Add to beginning of body area
            //Response.Write("Another Test.");
        }
    }
}
