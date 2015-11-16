using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;
    using Helper;
    using Microsoft.SharePoint.BusinessData.Administration;
    using System.Linq;

    public partial class ExternalContentTypes : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            listExternalContentTypes();

            if(!Page.IsPostBack) {
                ShowNewFormButton.Style.Add("display", "block");
                NewForm.Style.Add("display", "none");
              
                LobSystems.DataSource = Creator.listAllLobSystems().Select(x => x.Name);
                LobSystems.DataBind();
            }
            else
            {
                ShowNewFormButton.Style.Add("display", "none");
                NewForm.Style.Add("display", "block");

                if (!String.IsNullOrEmpty(LobSystems.SelectedItem.Text)) {
                    LobSystem lobSystem = Creator.getLobSystem(LobSystems.SelectedItem.Text);   

                    if (lobSystem == null)
                        throw new NullReferenceException("LobSystem can not be found.");

                    DataSourceTables.DataSource = SqlHelper.getTablesForLobSystem(lobSystem);
                    DataSourceTables.DataBind();
                }
            }
        }

        protected void listExternalContentTypes()
        {
            ExternalContentTypesContainer.InnerHtml = Creator.getAllExternalContentTypes();
        }

        protected void saveExternalContentType(object sender, EventArgs e)
        {
            Status.InnerHtml = "Not yet implemented.";
        }
    }
}
