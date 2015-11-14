﻿<%@ Assembly Name="ExternalContentTypeCreator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=99dce634a154d795" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DataSources.aspx.cs" Inherits="EFEXCON.ExternalLookup.Layouts.DataDefinition.Settings" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <link rel="stylesheet" type="text/css" media="screen" href="../css/style.css" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
EFEXCON Configuration
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Data Sources
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <h2>Available data sources</h2>
    <div id="DataSources" class="container" runat="server"></div>
    <asp:Button OnClientClick="showNewForm(this); return false;" Text="Add data source" runat="server" />

    <div id="NewForm" class="container new-form" runat="server">
        <h2>New data source</h2>
        <span class="table">
            <span class="table-row heading">
                <span class="table-cell">Data source attributes</span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="TitleLabel" runat="server" Text="Name"></asp:Label></span>
                <span class="table-cell"><input type="text" name="title" id="title" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="DataTypeLabel" runat="server" Text="Type"></asp:Label></span>
                <span class="table-cell">
                    <select id="dataType" name="dataType">
                        <option value="Database">SQL Database</option>
                    </select>
                </span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="ConnectionStringLabel" runat="server" Text="Connection url"></asp:Label></span>
                <span class="table-cell"><input type="text" id="connectionString" name="connectionString" /></span>
            </span>
            <span class="table-row heading">
                <span class="table-cell">Authentication parameters</span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="UsernameLabel" runat="server" Text="User name"></asp:Label></span>
                <span class="table-cell"><input type="text" id="username" name="username" /></span>
            </span>
            <span class="table-row">
                <span class="table-cell"><asp:Label ID="PasswordLabel" runat="server" Text="Password"></asp:Label></span>
                <span class="table-cell"><input type="password" id="password" name="password" /></span>
            </span>
        </span>
        <div id="newFormStatus" class="status"></div>
        <asp:Button OnClick="saveDataSource" OnClientClick="return checkForm()" Text="Save" runat="server" />
    </div>

    <div class="container">
        <div id="Status" class="status" runat="server"></div>

        <asp:Button OnClick="showAvailableECT" Text="List ECTs" runat="server" />
        <asp:Button OnClick="createLobSystem" Text="Create Lob System" runat="server" />
        <asp:Button OnClick="deleteLobSystem" Text="Delete Lob System" runat="server" />
    </div>

    <script type="text/javascript" src="../js/jquery-2.1.4.min.js"></script>
    <script type="text/javascript" src="../js/script.js"></script>
</asp:Content>
