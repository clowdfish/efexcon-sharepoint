using System;
using System.Data;
using Microsoft.SharePoint.WebControls;

namespace EFEXCON.ExternalLookup.Layouts.DataDefinition
{
    using Core;
    using Helper;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.BusinessData.Administration;
    using Microsoft.SharePoint.BusinessData.SharedService;
    using Microsoft.SharePoint.Utilities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class ExternalContentTypes : LayoutsPageBase
    {
        protected uint _language = 1033;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SPContext.Current.Web != null)
                _language = SPContext.Current.Web.Language;

            ListExternalContentTypes();

            if (!Page.IsPostBack)
            {
                ShowNewFormButton.Style.Add("display", "block");
                NewForm.Style.Add("display", "none");
                DataSourceStructureTable.Style.Add("display", "none");                

                try
                {
                    LobSystems.DataSource = Creator.ListAllLobSystems().Select(x => x.Name);
                    LobSystems.DataBind();
                }
                catch(Exception ex)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Access", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }
            }
            else
            {
                ShowNewFormButton.Style.Add("display", "none");
                NewForm.Style.Add("display", "block");               
            }
        }

        protected void LobSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LobSystems.SelectedItem.Value != (-1).ToString() && !String.IsNullOrEmpty(LobSystems.SelectedItem.Text))
            {
                // remove select instruction items
                LobSystems.Items.RemoveAt(0);

                LobSystem lobSystem = Creator.GetLobSystem(LobSystems.SelectedItem.Text);

                if (lobSystem == null)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Unavailable", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, null);
                    return;
                }

                try
                {
                    var credentials = SecureStoreHelper.GetCredentialsFromLobSystem(lobSystem);               

                    DataSourceEntity.Items.Add(new ListItem
                    {
                        Text = SPUtility.GetLocalizedString("$Resources:ExternalLookup_ContentType_DataSource_Select", "Resources", _language),
                        Value = "-1"
                    });
               
                    DataSourceEntity.DataSource = SqlHelper.GetTablesForLobSystem(lobSystem, credentials);
                    DataSourceEntity.DataBind();
                }
                catch(Exception ex)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Structure", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }
            }
            else
            {
                DataSourceEntity.DataSource = null;
                DataSourceEntity.DataBind();
                DataSourceEntity.Items.Clear();                

                DataSourceStructure.DataSource = null;
                DataSourceStructure.DataBind();
            }
        }

        protected void DataSourceEntity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DataSourceEntity.SelectedItem.Value != (-1).ToString() && !String.IsNullOrEmpty(DataSourceEntity.SelectedItem.Text))
            {
                // remove select instruction items
                DataSourceEntity.Items.RemoveAt(0);           

                LobSystem lobSystem = Creator.GetLobSystem(LobSystems.SelectedItem.Text);

                if (lobSystem == null)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Unavailable", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, null);
                    return;
                }

                try
                {
                    var credentials = SecureStoreHelper.GetCredentialsFromLobSystem(lobSystem);

                    DataSourceStructure.DataSource = SqlHelper.GetTableStructure(lobSystem, credentials, DataSourceEntity.SelectedItem.Text);
                    DataSourceStructure.DataBind();

                    DataSourceStructureTable.Style.Add("display", "block");
                }
                catch (Exception ex)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_DataSource_Structure", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }               
            }      
            else
            {
                DataSourceStructure.DataSource = null;
                DataSourceStructure.DataBind();
            }     
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ListExternalContentTypes()
        {
            ExternalContentTypesContainer.InnerHtml = "";

            try
            {
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
                        Text = SPUtility.GetLocalizedString("$Resources:ExternalLookup_General_Delete", "Resources", _language)
                    };
                    link.Command += DeleteContentType;
                    ExternalContentTypesContainer.Controls.Add(link);                  

                    ExternalContentTypesContainer.Controls.Add(separator);
                    counter++;
                }

                if (counter == 0)
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_ContentType_None", "Resources", _language);
                    ExternalContentTypesContainer.InnerHtml = message;
                }
            }
            catch(Exception ex)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_ContentType_Permissions", "Resources", _language);
                Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);

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

            // clear old status messages 
            Status.InnerHtml = "";

            var resultList = new List<ExternalColumnReference>();

            // process posted inputs
            foreach (string name in Request.Form.AllKeys)
            {               
                if (name.StartsWith("struct_") && name.EndsWith("_check"))
                {
                    var resultName = name.Substring(7, name.Length - 13);

                    resultList.Add(new ExternalColumnReference()
                    {
                        SourceName = resultName,
                        DestinationName = Request.Form["struct_" + resultName],
                        Type = Request.Form["struct_" + resultName + "_type"],
                        IsKey = !String.IsNullOrEmpty(Request.Form["struct_" + resultName + "_key"]),
                        IsSearchField = !String.IsNullOrEmpty(Request.Form["struct_" + resultName + "_search"])
                    });
                }                 
            }         

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
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_ContentType_Duplicate", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }
                else
                {
                    var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_ContentType_Create", "Resources", _language);
                    Status.InnerHtml = HtmlHelper.CreateErrorString(message, ex);
                }
            }

            ListExternalContentTypes();
            ClearForm();
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
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Status_ContentType_Delete", "Resources", _language);
                Status.InnerHtml = message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PrintContentType(object sender, CommandEventArgs e)
        {
            string name = e.CommandArgument.ToString();

            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog =
                service.GetAdministrationMetadataCatalog(context);

            EntityCollection availableEcts = catalog.GetEntities("*", "*", true);

            ShowNewFormButton.Style.Add("display", "block");
            NewForm.Style.Add("display", "none");

            var message = "";
            foreach (var entity in availableEcts)
            {
                if (entity.Name == name)
                {
                    message += "Entity:<br />";
                  
                    foreach (var method in entity.Methods)
                    {
                        message += "<br />Method '" + method.Name + "'<br />";
                        
                        foreach(var param in method.Parameters)
                        {
                            message += "Parameter with TypeDescriptor '" + param.RootTypeDescriptor.Name + "'<br />";
                            message += "LobName: " + param.RootTypeDescriptor.LobName + "<br />";
                            message += "Has Filter? " + param.RootTypeDescriptor.ContainsFilterDescriptor + "<br />";

                            foreach (var childDescriptor in param.RootTypeDescriptor.ChildTypeDescriptors)
                            {
                                message += "Child TypeDescriptor '" + childDescriptor.Name + "'<br />";
                                message += "Has Filter? " + childDescriptor.ContainsFilterDescriptor + "<br />";
                            }
                            
                            message += "<br />";
                        }

                        foreach (var filter in method.FilterDescriptors)
                        {
                            message += "<br />Filter '" + filter.Name + "' for field: '" + filter.FilterField + "'<br />";
                            foreach(var property in filter.Properties)
                            {
                                message += property.Name + " : " + property.Value + "<br />"; 
                            }
                        }
                    }
                }
            }
            
            Status.InnerHtml = message;            
        }

        protected void ClearForm()
        {
            NewContentTypeName.Value = "";

            LobSystems.Items.Clear();
            LobSystems.Items.Add(new ListItem
            {
                Text = SPUtility.GetLocalizedString("$Resources:ExternalLookup_ContentType_DataSource_Select", "Resources", _language),
                Value = "-1"
            });
            LobSystems.DataSource = Creator.ListAllLobSystems().Select(x => x.Name);
            LobSystems.DataBind();

            DataSourceEntity.DataSource = null;
            DataSourceEntity.DataBind();
            DataSourceEntity.Items.Clear();

            DataSourceStructure.DataSource = null;
            DataSourceStructure.DataBind();
        }
    } // end of class
}
