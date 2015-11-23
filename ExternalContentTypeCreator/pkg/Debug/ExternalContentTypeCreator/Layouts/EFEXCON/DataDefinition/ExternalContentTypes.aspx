<%@ Assembly Name="ExternalContentTypeCreator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=99dce634a154d795" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Import Namespace="System.Data" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExternalContentTypes.aspx.cs" Inherits="EFEXCON.ExternalLookup.Layouts.DataDefinition.ExternalContentTypes" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <link rel="stylesheet" type="text/css" media="screen" href="../css/style.css" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
<asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Configuration_Title%>" />
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_Title%>" />
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <h2><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_List_Title%>" /></h2>
    <div id="ExternalContentTypesContainer" class="container" runat="server"></div>
    <asp:Button OnClientClick="showNewForm(); return false;" Text="<%$Resources:Resources,ExternalLookup_ContentType_Add%>" ClientIDMode="Static" ID="ShowNewFormButton" runat="server" />

    <div id="NewForm" class="container new-form" runat="server">
        <h2><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_New_Title%>" /></h2>
        <span class="table">
            <span class="table-row heading">
                <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_Name%>" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="NameLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_Name%>"></asp:Label></span>
                <span class="table-cell">
                    <input type="text" id="NewContentTypeName" name="name" placeholder="<%$Resources:Resources,ExternalLookup_ContentType_Name_Placeholder%>" runat="server" />
                </span>
            </span>
            <span class="table-row heading">
                <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_Selection%>" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataSourceLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_Type%>"></asp:Label></span>
                <span class="table-cell">
                    <asp:DropDownList ID="LobSystems" OnSelectedIndexChanged="LobSystems_SelectedIndexChanged" AppendDataBoundItems="true" AutoPostBack="true" runat="server" Width="100%">
                        <asp:ListItem Value="-1" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_Select%>"></asp:ListItem>
                    </asp:DropDownList>
                </span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataSourceEntityLabel" runat="server" Text="Table"></asp:Label></span>
                <span class="table-cell">
                    <asp:DropDownList ID="DataSourceEntity" OnSelectedIndexChanged="DataSourceEntity_SelectedIndexChanged" AppendDataBoundItems="true" AutoPostBack="true" runat="server" Width="100%"></asp:DropDownList>  
                </span>
            </span>
        </span>
      
        <span class="table data-source-structure-table" id="DataSourceStructureTable" runat="server">
            <asp:Repeater ID="DataSourceStructure" runat="server">
                <HeaderTemplate>
                    <span class="table-row">
                        <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_ColumnName%>" /></span>
                        <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_Include%>" /></span>
                        <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_IsKey%>" /></span>
                        <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_ContentType_DataSource_FieldName%>" /></span>
                    </span>
                </HeaderTemplate>
                <ItemTemplate>
                    <span class="table-row">
                        <span class="table-cell"><%# Eval("Name") %></span>
                        <span class="table-cell">
                            <input type="checkbox" name="struct_<%# Eval("Name") %>_check" value="Test"/>
                        </span>
                        <span class="table-cell">
                            <input type="checkbox" name="struct_<%# Eval("Name") %>_key" value="Test"/>
                        </span>
                        <span class="table-cell">
                            <input type="text" name="struct_<%# Eval("Name") %>" value="<%# Eval("Name") %>" />
                            <input type="hidden" name="struct_<%# Eval("Name") %>_type" value="<%# Eval("Type") %>" />
                        </span>
                    </span>
                </ItemTemplate>
            </asp:Repeater>
        </span>

        <div id="newFormStatus" class="status"></div>
        <asp:Button OnClick="SaveExternalContentType" OnClientClick="return checkForm() && validStructure()" Text="<%$Resources:Resources,ExternalLookup_General_Save%>" runat="server" />
        <asp:Button OnClientClick="hideNewForm(); return false;" Text="<%$Resources:Resources,ExternalLookup_General_Cancel%>" runat="server" />
    </div>
    <div id="Status" class="status" runat="server"></div>

    <script type="text/javascript" src="../js/jquery-2.1.4.min.js"></script>
    <script type="text/javascript" src="../js/script.js"></script>
</asp:Content>
