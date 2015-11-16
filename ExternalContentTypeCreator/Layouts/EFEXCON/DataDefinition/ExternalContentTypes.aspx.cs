using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;
    using Helper;
    using Microsoft.SharePoint.BusinessData.Administration;
    using System.Collections.Generic;
    using System.Linq;

    public partial class ExternalContentTypes : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            listExternalContentTypes();

            if (!Page.IsPostBack)
            {
                ShowNewFormButton.Style.Add("display", "block");
                NewForm.Style.Add("display", "none");
                DataSourceStructureTable.Style.Add("display", "none");

                LobSystems.DataSource = Creator.listAllLobSystems().Select(x => x.Name);
                LobSystems.DataBind();
            }
            else
            {
                ShowNewFormButton.Style.Add("display", "none");
                NewForm.Style.Add("display", "block");

                if (!String.IsNullOrEmpty(LobSystems.SelectedItem.Text))
                {
                    LobSystem lobSystem = Creator.getLobSystem(LobSystems.SelectedItem.Text);

                    if (lobSystem == null)
                        throw new NullReferenceException("LobSystem can not be found.");

                    if (DataSourceTables.DataSource == null)
                    {
                        DataSourceTables.DataSource = SqlHelper.getTablesForLobSystem(lobSystem);
                        DataSourceTables.DataBind();
                    }                         
       
                    if (!String.IsNullOrEmpty(DataSourceTables.SelectedItem.Text))
                    {
                        DataSourceStructureTable.Style.Add("display", "block");

                        if (DataSourceStructure.DataSource == null)
                        {
                            DataSourceStructure.DataSource = SqlHelper.getTableStructure(lobSystem, DataSourceTables.SelectedItem.Text);
                            DataSourceStructure.DataBind();
                        }
                    }
                }
            }
        }

        protected void listExternalContentTypes()
        {
            ExternalContentTypesContainer.InnerHtml = Creator.getAllExternalContentTypes();
        }

        protected void saveExternalContentType(object sender, EventArgs e)
        {    
            List<string> list = new List<string>();
            List<string> checkList = new List<string>();

            // process posted inputs
            foreach (string name in Request.Form.AllKeys)
            {               
                if (name.StartsWith("struct_"))
                {    
                    if(name.EndsWith("_check"))
                    {
                        checkList.Add(name.Substring(7));
                    }
                    else if(!name.EndsWith("_key") && !name.EndsWith("_type"))
                    {
                        list.Add(name.Substring(7));
                    }
                }
            }
            
            List<ExternalColumnReference> resultList = new List<ExternalColumnReference>();
            foreach (string item in list)
            {
                if (checkList.Contains(item + "_check"))
                {
                    resultList.Add(new ExternalColumnReference()
                    {
                        SourceName = item,
                        DestinationName = Request.Form["struct_" + item],
                        Type = Request.Form["struct_" + item + "_type"],
                        IsKey = String.IsNullOrEmpty(Request.Form["struct_" + item + "_key"]) ? false : true
                    });                  
                }
            }
        
            // TODO now start creation of new external content type

            /*
            foreach(ExternalColumnReference reference in resultList)
            {
                Status.InnerHtml += reference.SourceName + " (" + reference.Type + ")";

                if (reference.IsKey)
                    Status.InnerHtml += " KEY!; ";
                else
                    Status.InnerHtml += "; ";
            }
            */

            Status.InnerHtml = "Not yet implemented.";           
        }
    }
}
