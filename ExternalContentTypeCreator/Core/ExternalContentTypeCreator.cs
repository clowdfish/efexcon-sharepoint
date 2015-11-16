//using Microsoft.SharePoint.BusinessData.Administration.Client;
using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using System;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.Collections.Generic;

namespace EFEXCON.ExternalLookup.Core
{
    /// <summary>
    /// Class Creator.
    /// </summary>
    public class Creator
    {

        public void Initialize()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string getAllExternalContentTypes()
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            //SPSite site = new SPSite("http://sharepoint-dev");
            //SPServiceContext context = SPServiceContext.GetContext(site);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog = 
                service.GetAdministrationMetadataCatalog(context);

            EntityCollection ects = catalog.GetEntities("*", "*", true);

            string result = "";

            foreach (Entity ect in ects)
            {
                result += "ECT Name: " + ect.Name + "<br />";
            }

            if (String.IsNullOrEmpty(result))
                result = "No external content type available.";

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LobSystem createLobSystem(string name, SystemType type)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

            LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

            foreach (var lobSystem in availableLobSystems)
            {
                if(lobSystem.Name == name && lobSystem.SystemType == type)
                {
                    return lobSystem;
                }
            }

            // if no LobSystem was found, create a new one and return it
            return availableLobSystems.Create(name, true, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static LobSystem getLobSystem(string name)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

            LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

            foreach (var lobSystem in availableLobSystems)
            {
                if (lobSystem.Name == name)
                {
                    return lobSystem;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Boolean deleteLobSystem(string name, SystemType type)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

            LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

            foreach (var lobSystem in availableLobSystems)
            {
                if (lobSystem.Name == name && lobSystem.SystemType == type)
                {
                    lobSystem.Delete();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static LobSystemInstance createLobSystemInstance(LobSystem lobSystem, string server, string database, string username, string password)
        {
            LobSystemInstance lobSystemInstance = null;

            foreach (var instance in lobSystem.LobSystemInstances)
            {
                if (instance.Name == lobSystem.Name && instance.LobSystem == lobSystem)
                {
                    lobSystemInstance = instance;
                }
            }

            if(lobSystemInstance == null)
            {
                lobSystemInstance = lobSystem.LobSystemInstances.Create(lobSystem.Name, true, lobSystem);
            }

            // Set the connection properties 
            lobSystemInstance.Properties.Add("AuthenticationMode", "Credentials");
            lobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            lobSystemInstance.Properties.Add("RdbConnection Data Source", server);
            lobSystemInstance.Properties.Add("RdbConnection Initial Catalog", database);
            lobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            lobSystemInstance.Properties.Add("RdbConnection Pooling", "false");
            lobSystemInstance.Properties.Add("RdbConnection User ID", username);
            lobSystemInstance.Properties.Add("RdbConnection Password", password);
            lobSystemInstance.Properties.Add("RdbConnection Trusted_Connection", "false");

            return lobSystemInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<LobSystem> listAllLobSystems()
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

            LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

            List<LobSystem> result = new List<LobSystem>();

            foreach (var lobSystem in availableLobSystems)
            {
                result.Add(lobSystem);
            }

            return result;
        }

        public void createNewContentType(string name, LobSystem lobSystem)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog = 
                service.GetAdministrationMetadataCatalog(context);

            // Create a new customer model 
            Model model = Model.Create(name + "Model", true, catalog);

            string lobSystemInstanceName;

            LobSystemInstanceCollection collection = lobSystem.LobSystemInstances;
            if(collection.GetEnumerator().Current != null)
            {
                lobSystemInstanceName = collection.GetEnumerator().Current.Name;
            }
            else
            {
                throw new LobGenericException("No LobSystemInstance available in LobSystem.");
            }
                        
            // Create a new Entity 
            Entity entity = Entity.Create(name, lobSystemInstanceName, true, new Version("1.0.0.0"), 10000, CacheUsage.Default, lobSystem, model, catalog);

            // TODO go on here

            // Set the identifier - CustomerID column 
            entity.Identifiers.Create("CustomerId", true, "System.Int32");

            /* Define the Specific Finder Method, Parameters and Type Descriptors
            Next, create the specific finder Method, specify the query it will use, and define the input and output parameters associated with it. 
            The specific finder Method returns exactly one row of data from the data source, given an identifier. */

            // Create the specific finder method 
            Method getCustomerMethod = entity.Methods.Create("GetCustomer", true, false, "GetCustomer");

            // Specify the query 
            getCustomerMethod.Properties.Add("RdbCommandText", "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer] WHERE [CustomerId] = @CustomerId");

            // Set the command type 
            getCustomerMethod.Properties.Add("RdbCommandType", "Text");

            // Create the CustomerID input parameter 
            Parameter customerIDParameter = getCustomerMethod.Parameters.Create("@CustomerId", true, DirectionType.In);

            // Create the TypeDescriptor for the CustomerID parameter 
            customerIDParameter.CreateRootTypeDescriptor("CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null, catalog);

            // Create the Customer return parameter 
            Parameter customerParameter = getCustomerMethod.Parameters.Create("Customer", true, DirectionType.Return);

            // Create the TypeDescriptors for the Customer return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor = customerParameter.CreateRootTypeDescriptor( 
                "Customers", true, "System.Data.IDataReader, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "Customers", null, null, TypeDescriptorFlags.IsCollection, null, catalog);
            TypeDescriptor returnRootElementTypeDescriptor = returnRootCollectionTypeDescriptor.ChildTypeDescriptors.Create( 
                "Customer", true, "System.Data.IDataRecord, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "Customer", null, null, TypeDescriptorFlags.None, null);

            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "FirstName", true, "System.String", "FirstName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "LastName", true, "System.String", "LastName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "Phone", true, "System.String", "Phone", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "EmailAddress", true, "System.String", "EmailAddress", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create( "CompanyName", true, "System.String", "CompanyName", null, null, TypeDescriptorFlags.None, null);

            // Create the specific finder method instance 
            getCustomerMethod.MethodInstances.Create("GetCustomer", true, returnRootElementTypeDescriptor, MethodInstanceType.SpecificFinder, true);
       
            /* Define the Finder Method, Parameters and Type Descriptors
            Next, create the finder Method, specify the query it will use, and define the output parameters associated with it. 
            The finder Method returns all of the rows of data from the data source which its query defines. */

            // Create the Finder method 
            Method getCustomersMethod = entity.Methods.Create("GetCustomers", true, false, "GetCustomers");

            // Specify the query 
            getCustomersMethod.Properties.Add("RdbCommandText", "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer]");

            // Set the command type 
            getCustomersMethod.Properties.Add("RdbCommandType", "Text");

            // Create the Customer return parameter
            Parameter customersParameter = getCustomersMethod.Parameters.Create("Customer", true, DirectionType.Return);

            // Create the TypeDescriptors for the Customer return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor2 = customersParameter.CreateRootTypeDescriptor("Customers", true, "System.Data.IDataReader, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "Customers", null, null, TypeDescriptorFlags.IsCollection, null, catalog);
            TypeDescriptor returnRootElementTypeDescriptor2 = returnRootCollectionTypeDescriptor2.ChildTypeDescriptors.Create( "Customer", true, "System.Data.IDataRecord, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "Customer", null, null, TypeDescriptorFlags.None, null);

            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "FirstName", true, "System.String", "FirstName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "LastName", true, "System.String", "LastName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "Phone", true, "System.String", "Phone", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "EmailAddress", true, "System.String", "EmailAddress", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor2.ChildTypeDescriptors.Create( "CompanyName", true, "System.String", "CompanyName", null, null, TypeDescriptorFlags.None, null);

            getCustomersMethod.MethodInstances.Create("GetCustomers", true, returnRootCollectionTypeDescriptor2, MethodInstanceType.Finder, true);

            // Publish the newly created Entity to the BCS Metadata Store.
            entity.Activate();
        }
    }
 }
  