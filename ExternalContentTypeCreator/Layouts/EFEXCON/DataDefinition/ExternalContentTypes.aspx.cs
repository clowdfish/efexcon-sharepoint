using System;
using Microsoft.SharePoint.WebControls;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;
    using Helper;
    using Microsoft.SharePoint.BusinessData.Administration;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

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

        /// <summary>
        /// 
        /// </summary>
        protected void listExternalContentTypes()
        {
            ExternalContentTypesContainer.InnerHtml = "";

            int counter = 0;
            foreach (Entity contentType in Creator.listAllExternalContentTypes())
            {
                var separator = new LiteralControl("<div></div>");

                var label = new Label();
                label.Text = contentType.Name + " ";
                ExternalContentTypesContainer.Controls.Add(label);

                var link = new LinkButton
                {
                    ID = "delete_" + contentType.Name,
                    CommandArgument = contentType.Name,
                    Text = "delete"
                };
                link.Command += deleteContentType;
                ExternalContentTypesContainer.Controls.Add(link);

                ExternalContentTypesContainer.Controls.Add(separator);
                counter++;
            }

            if(counter == 0)
            {
                ExternalContentTypesContainer.InnerHtml = "No external content type available.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void saveExternalContentType(object sender, EventArgs e)
        {
            // hide form
            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

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

            string newContentTypeName = NewContentTypeName.Value;
            LobSystem lobSystem = Creator.getLobSystem(LobSystems.SelectedItem.Text);

            // start creation of new external content type
            Creator.createNewContentType(newContentTypeName, resultList, lobSystem);

            listExternalContentTypes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void deleteContentType(object sender, CommandEventArgs e)
        {
            string ectName = e.CommandArgument.ToString();
            var deleted = Creator.deleteContentType(ectName);

            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

            if (deleted)
            {
                listExternalContentTypes();
            }
            else
            {
                Status.InnerHtml = "External Content type could not be deleted.";
            }
        }
    } // end of class
}
