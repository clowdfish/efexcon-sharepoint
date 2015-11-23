<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DataSources.aspx.cs" Inherits="EFEXCON.ExternalLookup.Layouts.DataDefinition.DataSources" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <link rel="stylesheet" type="text/css" media="screen" href="../css/style.css" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
<asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Configuration_Title%>" />
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Title%>" />
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <h2><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_List_Title%>" /></h2>
    <div id="DataSourceContainer" class="container" runat="server"></div>
    <asp:Button OnClientClick="showNewForm(); return false;" Text="<%$Resources:Resources,ExternalLookup_DataSource_Add%>" ClientIDMode="Static" ID="ShowNewFormButton" runat="server" />

    <div id="NewForm" class="container new-form data-source-form" runat="server">
        <h2><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_New_Title%>" /></h2>
        <span class="table">
            <span class="table-row heading">
                <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Attributes%>" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="TitleLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Name%>"></asp:Label></span>
                <span class="table-cell"><input type="text" name="title" id="title" placeholder="<asp:Literal runat='server' Text='<%$Resources:Resources,ExternalLookup_DataSource_Name_Placeholder%>' />"/></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataTypeLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Type%>"></asp:Label></span>
                <span class="table-cell">
                    <select id="dataType" name="dataType">
                        <option value="Database"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Type_SQL%>" /></option>
                    </select>
                </span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DatabaseLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Database%>"></asp:Label></span>
                <span class="table-cell"><input type="text" name="database" id="database" placeholder="<asp:Literal runat='server' Text='<%$Resources:Resources,ExternalLookup_DataSource_DatabaseName%>' />" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="ServerNameLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_ServerName%>"></asp:Label></span>
                <span class="table-cell"><input type="text" id="url" name="url" placeholder="<asp:Literal runat='server' Text='<%$Resources:Resources,ExternalLookup_DataSource_ServerName_Placeholder%>' />" /></span>
            </span>
            <span class="table-row heading">
                <span class="table-cell"><asp:Literal runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_Authentication_Title%>" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="SecureStoreApplicationIdLabel" runat="server" Text="<%$Resources:Resources,ExternalLookup_DataSource_SecureStoreServiceId%>"></asp:Label></span>
                <span class="table-cell"><input type="text" id="secureStoreApplicationId" name="secureStoreApplicationId" /></span>
            </span>
        </span>
        <div id="newFormStatus" class="status"></div>
        <asp:Button OnClick="SaveDataSource" OnClientClick="return checkForm()" Text="<%$Resources:Resources,ExternalLookup_General_Save%>" runat="server" />
        <asp:Button OnClientClick="hideNewForm(); return false;" Text="<%$Resources:Resources,ExternalLookup_General_Cancel%>" runat="server" />
    </div>
    <div id="Status" class="status" runat="server"></div>

    <script type="text/javascript">
        var allFieldsFilled = "<SharePoint:EncodedLiteral runat='server' text='<%$Resources:Resources,ExternalLookup_Message_AllFields%>' EncodeMethod='EcmaScriptStringLiteralEncode'/>";
    </script>
    <script type="text/javascript" src="../js/jquery-2.1.4.min.js"></script>
    <script type="text/javascript" src="../js/script.js"></script>
</asp:Content>