using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;

namespace EFEXCON.ExternalLookup.Helper
{
    public class HtmlHelper
    {
        public static string CreateErrorString(string message, Exception ex)
        {
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;
            var linkText = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Link_Details", "Resources", language);

            var exceptionString = "";
            if (ex != null)
                exceptionString = " <a class='status-show-details'>" + linkText + "</a><div class='status-details'>" + ex.Message + "</div>";

            return "<div class='status error'>" + message + exceptionString + "</div>";
        }
    }
}