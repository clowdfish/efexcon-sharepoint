using System;

namespace EFEXCON.ExternalLookup.Helper
{
    public class TableColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class ExternalColumnReference
    {
        /// <summary>
        /// The name of the original field of the external data source. 
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// The name of the field that is used in the new external content type.
        /// </summary>
        public string DestinationName { get; set; }
        public string _type { get; set; }
        public Boolean IsKey { get; set; }
        public Boolean IsSearchField { get; set; }

        public string Type
        {
            get {
                if (_type == "int")
                    return "System.Int32";
                else if (_type == "datetime" || _type == "date")
                    return "System.DateTime";
                else
                    return "System.String";
            }
            set {               
                _type = value;
            }
        }
    }
}