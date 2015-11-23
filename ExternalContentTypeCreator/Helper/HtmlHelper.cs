using System;

namespace EFEXCON.ExternalLookup.Helper
{
    public class HtmlHelper
    {
        public static string CreateErrorString(string message, Exception ex)
        {
            var exceptionString = "";
            if (ex != null)
                exceptionString = " <a class='status-show-details'>Show details</a><div class='status-details'>" + ex.Message + "</div>";

            return "<div class='status error'>" + message + exceptionString + "</div>";
        }
    }
}