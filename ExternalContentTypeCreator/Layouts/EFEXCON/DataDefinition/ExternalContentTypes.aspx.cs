using System;
using System.Data;
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
            ListExternalContentTypes();

            if (!Page.IsPostBack)
            {
                ShowNewFormButton.Style.Add("display", "block");
                NewForm.Style.Add("display", "none");
                DataSourceStructureTable.Style.Add("display", "none");

                LobSystems.DataSource = Creator.ListAllLobSystems().Select(x => x.Name);
                LobSystems.DataBind();
            }
            else
            {
                ShowNewFormButton.Style.Add("display", "none");
                NewForm.Style.Add("display", "block");

                if (!String.IsNullOrEmpty(LobSystems.SelectedItem.Text))
                {
                    LobSystem lobSystem = Creator.GetLobSystem(LobSystems.SelectedItem.Text);

                    if (lobSystem == null)
                        throw new NullReferenceException("LobSystem can not be found.");

                    var sssId = "";
                    var providerimplementation = "";

                    foreach (Property prop in SqlHelper.GetLobSystemInstanceProperties(lobSystem))
                    {
                        if (prop.Name == "SsoApplicationId")
                            sssId = prop.Value.ToString();

                        if (prop.Name == "SsoProviderImplementation")
                            providerimplementation = prop.Value.ToString();
                    }

                    if (String.IsNullOrEmpty(sssId))
                        throw new Exception("Secure Store Application ID can not be identified.");

                    if (String.IsNullOrEmpty(providerimplementation))
                        throw new Exception("Provider implementation can not be identified.");

                    var credentials = new SecureStoreHelper(sssId, providerimplementation).GetCredentials();

                    if(credentials == null)
                        throw new NoNullAllowedException("Credentials could not be retrieved from Secure Store Service.");

                    if (DataSourceEntity.DataSource == null)
                    {
                        DataSourceEntity.DataSource = SqlHelper.GetTablesForLobSystem(lobSystem, credentials);
                        DataSourceEntity.DataBind();
                    }

                    if (!String.IsNullOrEmpty(DataSourceEntity.SelectedItem.Text))
                    {
                        DataSourceStructureTable.Style.Add("display", "block");

                        if (DataSourceStructure.DataSource == null)
                        {
                            DataSourceStructure.DataSource = SqlHelper.GetTableStructure(lobSystem, credentials, DataSourceEntity.SelectedItem.Text);
                            DataSourceStructure.DataBind();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ListExternalContentTypes()
        {
            ExternalContentTypesContainer.InnerHtml = "";

            int counter = 0;
            foreach (Entity contentType in Creator.ListAllExternalContentTypes())
            {
                var separator = new LiteralControl("<div></div>");

                var label = new Label { Text = contentType.Name + " " };
                ExternalContentTypesContainer.Controls.Add(label);

                var link = new LinkButton
                {
                    ID = "delete_" + contentType.Name,
                    CommandArgument = contentType.Name,
                    Text = "delete"
                };
                link.Command += DeleteContentType;
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
        protected void SaveExternalContentType(object sender, EventArgs e)
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

            List<ExternalColumnReference> resultList = (
                from item in list
                where checkList.Contains(item + "_check")
                select new ExternalColumnReference()
                {
                    SourceName = item, 
                    DestinationName = Request.Form["struct_" + item], 
                    Type = Request.Form["struct_" + item + "_type"], 
                    IsKey = !String.IsNullOrEmpty(Request.Form["struct_" + item + "_key"])
                }).ToList();

            string newContentTypeName = NewContentTypeName.Value;
            string tableName = DataSourceEntity.SelectedItem.Text;
            LobSystem lobSystem = Creator.GetLobSystem(LobSystems.SelectedItem.Text);

            try
            {
                Creator.CreateNewContentType(newContentTypeName, tableName, resultList, lobSystem);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("has an Duplicate value in Field"))
                {
                    Status.InnerHtml = 
                        "<div class='status error'>Could not create external content type. An external content type with " +
                        "the same name was deleted previously and is still in the cache.</div>";
                }
                else
                    Status.InnerHtml = "<div class='status error'>Could not create external content type.</div>";
            }

            ListExternalContentTypes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DeleteContentType(object sender, CommandEventArgs e)
        {
            string ectName = e.CommandArgument.ToString();
            var deleted = Creator.DeleteContentType(ectName);

            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

            if (deleted)
            {
                ListExternalContentTypes();
            }
            else
            {
                Status.InnerHtml = "External Content type could not be deleted.";
            }
        }
    } // end of class
}
