using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using EFEXCON.ExternalLookup.Helper;
using System.Collections;
using System.Linq;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="referenceList"></param>
        /// <param name="lobSystem"></param>
        public void createNewContentType(string name, List<ExternalColumnReference> referenceList, LobSystem lobSystem)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog = 
                service.GetAdministrationMetadataCatalog(context);

            // Create a new customer model 
            Model model = Model.Create(name + "Model", true, catalog);

            string lobSystemInstanceName;

            IEnumerator<LobSystemInstance> enumerator = lobSystem.LobSystemInstances.GetEnumerator();
            enumerator.MoveNext();

            if(enumerator.Current != null)
            {
                lobSystemInstanceName = enumerator.Current.Name;
            }
            else
            {
                throw new LobGenericException("No LobSystemInstance available in LobSystem.");
            }
                        
            // Create a new Entity 
            Entity entity = Entity.Create(name, lobSystemInstanceName, true, new Version("1.0.0.0"), 10000, CacheUsage.Default, lobSystem, model, catalog);

            // Set the identifier
            ExternalColumnReference keyReference = referenceList.Where(x => x.IsKey == true).First();
            entity.Identifiers.Create(keyReference.DestinationName, true, keyReference.Type); // e.g. CustomerId //  "System.Int32"

            // Create the specific finder method to return one specific element
            Creator.CreateReadItemMethod(name, catalog, entity);

            // Create the finder method to return all rows
            Creator.CreateReadListMethod(name, catalog, entity);

            // Publish the newly created Entity to the BCS Metadata Store.
            entity.Activate();
        }

        /// <summary>
        /// Creates the finder Method, specify the query it will use, and define the output parameters associated with it.
        /// The finder Method returns all of the rows of data from the data source which its query defines.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadListMethod(string name, string database, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            string listMethodName = String.Format("Get{0}s", name);
            string listMethodEntity = name + "s";
            string itemMethodEntity = name;

            // Create the Finder method 
            Method getListMethod = entity.Methods.Create(listMethodName, true, false, listMethodName);

            // Specify the query 
            getListMethod.Properties.Add("RdbCommandText", "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer]");

            // Set the command type 
            getListMethod.Properties.Add("RdbCommandType", "Text");

            // Create the Customer return parameter
            Parameter customersParameter = getListMethod.Parameters.Create(name, true, DirectionType.Return); // e.g. Customer

            
            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor = 
                customersParameter.CreateRootTypeDescriptor(
                    listMethodEntity, 
                    true, 
                    "System.Data.IDataReader, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    listMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.IsCollection, 
                    null, 
                    catalog); // e.g. Customers

            TypeDescriptor returnRootElementTypeDescriptor = 
                returnRootCollectionTypeDescriptor.ChildTypeDescriptors.Create(
                    itemMethodEntity, 
                    true, 
                    "System.Data.IDataRecord, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    itemMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.None, 
                    null); // e.g. Customer

            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("FirstName", true, "System.String", "FirstName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("LastName", true, "System.String", "LastName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("Phone", true, "System.String", "Phone", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("EmailAddress", true, "System.String", "EmailAddress", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("CompanyName", true, "System.String", "CompanyName", null, null, TypeDescriptorFlags.None, null);

            getListMethod.MethodInstances.Create(listMethodName, true, returnRootCollectionTypeDescriptor, MethodInstanceType.Finder, true);
        }

        /// <summary>
        /// Create the specific finder Method, specify the query it will use, and define the input and output parameters associated with it.
        /// The specific finder Method returns exactly one row of data from the data source, given an identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadItemMethod(string name, string database, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            string itemMethodName = "Get" + name;
            string itemMethodEntity = name;
            string listMethodEntity = name + "s";

            Method getItemMethod = entity.Methods.Create(itemMethodName, true, false, itemMethodName);

            // Specify the query 
            getItemMethod.Properties.Add("RdbCommandText", "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer] WHERE [CustomerId] = @CustomerId");

            // Set the command type 
            getItemMethod.Properties.Add("RdbCommandType", "Text");

            // Create the CustomerID input parameter 
            Parameter entityIDParameter = getItemMethod.Parameters.Create("@CustomerId", true, DirectionType.In);

            // Create the TypeDescriptor for the CustomerID parameter 
            entityIDParameter.CreateRootTypeDescriptor("CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null, catalog);

            // Create the Customer return parameter 
            Parameter customerParameter = getItemMethod.Parameters.Create("Customer", true, DirectionType.Return);

            // Create the TypeDescriptors for the Customer return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor = 
                customerParameter.CreateRootTypeDescriptor(
                    listMethodEntity, 
                    true, 
                    "System.Data.IDataReader, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    listMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.IsCollection, 
                    null, 
                    catalog); // e.g. Customers

            TypeDescriptor returnRootElementTypeDescriptor = 
                returnRootCollectionTypeDescriptor.ChildTypeDescriptors.Create(
                    itemMethodEntity, 
                    true, 
                    "System.Data.IDataRecord, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    itemMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.None, 
                    null); // e.g. Customer

            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("CustomerId", true, "System.Int32", "CustomerId", new IdentifierReference("CustomerId", new EntityReference("AdventureWorks", "Customer", catalog), catalog), null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("FirstName", true, "System.String", "FirstName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("LastName", true, "System.String", "LastName", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("Phone", true, "System.String", "Phone", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("EmailAddress", true, "System.String", "EmailAddress", null, null, TypeDescriptorFlags.None, null);
            returnRootElementTypeDescriptor.ChildTypeDescriptors.Create("CompanyName", true, "System.String", "CompanyName", null, null, TypeDescriptorFlags.None, null);

            // Create the specific finder method instance 
            getItemMethod.MethodInstances.Create(itemMethodName, true, returnRootElementTypeDescriptor, MethodInstanceType.SpecificFinder, true); // getCustomer
        }
    }
 }
  