using System;

namespace EFEXCON.ExternalLookup.Helper
{
    public class ExternalColumnReference
    {
        public string SourceName { get; set; }
        public string DestinationName { get; set; }
        public string _type { get; set; }
        public Boolean IsKey { get; set; }

        public string Type
        {
            get {
                if (_type == "int")
                    return "System.Int32";
                else 
                    return "System.String";
            }
            set {               
                _type = value;
            }
        }
    }
}