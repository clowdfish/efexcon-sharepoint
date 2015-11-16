<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
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
EFEXCON Configuration
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
External Content Types
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <h2>Available external content types</h2>
    <div id="ExternalContentTypesContainer" class="container" runat="server"></div>
    <asp:Button OnClientClick="showNewForm(); return false;" Text="Add external content type" ClientIDMode="Static" ID="ShowNewFormButton" runat="server" />

    <div id="NewForm" class="container new-form" runat="server">
        <h2>New external content type</h2>
        <span class="table">
            <span class="table-row heading">
                <span class="table-cell">Data source selection</span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataSourceLabel" runat="server" Text="Type"></asp:Label></span>
                <span class="table-cell">
                    <asp:DropDownList ID="LobSystems" AutoPostBack="true" runat="server" AppendDataBoundItems="true" Width="100%">
                        <asp:ListItem></asp:ListItem>
                    </asp:DropDownList>
                </span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataSourceEntityLabel" runat="server" Text="Entity"></asp:Label></span>
                <span class="table-cell">
                    <asp:DropDownList ID="DataSourceTables" AutoPostBack="true" runat="server" Width="100%"></asp:DropDownList>  
                </span>
            </span>
        </span>
      
        <span class="table data-source-structure-table" id="DataSourceStructureTable" runat="server">
            <asp:Repeater ID="DataSourceStructure" runat="server">
                <HeaderTemplate>
                    <span class="table-row">
                        <span class="table-cell">Column name</span>
                        <span class="table-cell">Include in ECT</span>
                        <span class="table-cell">Field name in ECT</span>
                    </span>
                </HeaderTemplate>
                <ItemTemplate>
                    <span class="table-row">
                        <span class="table-cell"><%# Eval("Name") %></span>
                        <span class="table-cell"><input type="checkbox" name="struct_<%# Eval("Name") %>_check" value="Test"/></span>
                        <span class="table-cell"><input type="text" name="struct_<%# Eval("Name") %>" value="<%# Eval("Name") %>" /></span>
                    </span>
                </ItemTemplate>
            </asp:Repeater>
        </span>

        <div id="newFormStatus" class="status"></div>
        <asp:Button OnClick="saveExternalContentType" OnClientClick="return checkForm() && validStructure()" Text="Save" runat="server" />
        <asp:Button OnClientClick="hideNewForm(); return false;" Text="Cancel" runat="server" />
    </div>
    <div id="Status" class="status" runat="server"></div>

    <script type="text/javascript" src="../js/jquery-2.1.4.min.js"></script>
    <script type="text/javascript" src="../js/script.js"></script>
</asp:Content>