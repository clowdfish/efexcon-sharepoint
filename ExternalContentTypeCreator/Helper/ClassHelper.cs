using System;

namespace EFEXCON.ExternalLookup.Helper
{
    public class TableColumn
    {
        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Nullable { get; set; }
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

        private Boolean _isNullable;
        private string nullableDescription =
            "System.Nullable`1[[{0}, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

        public string Type
        {
            get {
                if (_type == "int") {
                    if(_isNullable)
                        return String.Format(nullableDescription, "System.Int32");
                    else
                        return "System.Int32";
                }                    
                else if (_type == "datetime" || _type == "date") {
                    if(_isNullable)
                        return String.Format(nullableDescription, "System.DateTime");
                    else
                        return "System.DateTime";
                }
                else
                {   
                    // there is no nullable pendent for string
                    return "System.String";
                }
            }
            set {
                if (value.EndsWith(";Nullable"))
                {
                    _isNullable = true;
                    _type = value.Substring(0, value.Length - 9);
                }
                else
                {
                    _type = value;
                }                                
            }
        }
    }
}