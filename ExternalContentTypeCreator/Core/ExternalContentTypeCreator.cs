using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using EFEXCON.ExternalLookup.Helper;
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
        public static List<Entity> ListAllExternalContentTypes()
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog = 
                service.GetAdministrationMetadataCatalog(context);

            EntityCollection ects = catalog.GetEntities("*", "*", true);
            return ects.ToList<Entity>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LobSystem CreateLobSystem(string name, SystemType type)
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
        public static LobSystem GetLobSystem(string name)
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
        public static Boolean DeleteLobSystem(string name, SystemType type)
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
        public static LobSystemInstance CreateLobSystemInstance(LobSystem lobSystem, string server, string database, string username, string password)
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
            // The following url helps to understand the different authentication modes and the relation to the ones
            // given in SharePoint Designer: https://technet.microsoft.com/en-us/library/ee661743.aspx#Section3

            //lobSystemInstance.Properties.Add("AuthenticationMode", "Credentials");
            //lobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            //lobSystemInstance.Properties.Add("RdbConnection Data Source", server);
            //lobSystemInstance.Properties.Add("RdbConnection Initial Catalog", database);
            //lobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            //lobSystemInstance.Properties.Add("RdbConnection Pooling", "false");
            //lobSystemInstance.Properties.Add("RdbConnection User ID", username);
            //lobSystemInstance.Properties.Add("RdbConnection Password", password);
            //lobSystemInstance.Properties.Add("RdbConnection Trusted_Connection", "false");

            //lobSystemInstance.Properties.Add("AuthenticationMode", "PassThrough");
            //lobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            //lobSystemInstance.Properties.Add("RdbConnection Data Source", server);
            //lobSystemInstance.Properties.Add("RdbConnection Initial Catalog", database);
            //lobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            //lobSystemInstance.Properties.Add("RdbConnection Pooling", "false");

            lobSystemInstance.Properties.Add("AuthenticationMode", "WindowsCredentials");
            lobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            lobSystemInstance.Properties.Add("RdbConnection Data Source", server);
            lobSystemInstance.Properties.Add("RdbConnection Initial Catalog", database);
            lobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            lobSystemInstance.Properties.Add("RdbConnection Pooling", "false");
            lobSystemInstance.Properties.Add("SsoApplicationId", "SQLServer");
            lobSystemInstance.Properties.Add("SsoProviderImplementation", "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");

            return lobSystemInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<LobSystem> ListAllLobSystems()
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

           return service.GetAdministrationMetadataCatalog(context).GetLobSystems("*").ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        /// <param name="referenceList"></param>
        /// <param name="lobSystem"></param>
        public static void CreateNewContentType(string name, string table, List<ExternalColumnReference> referenceList, LobSystem lobSystem)
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
            ExternalColumnReference keyReference = referenceList.First(x => x.IsKey == true);
            entity.Identifiers.Create(keyReference.DestinationName, true, keyReference.Type); // e.g. CustomerId // "System.Int32"

            var database = "";
            foreach (Property prop in SqlHelper.getLobSystemInstanceProperties(lobSystem))
            {
                if (prop.Name == "RdbConnection Initial Catalog")
                    database = prop.Value.ToString();
            }

            if (String.IsNullOrEmpty(database))
                throw new Exception("Database name can not be set.");

            // Create the specific finder method to return one specific element
            Creator.CreateReadItemMethod(name, database, table, lobSystem.Name, referenceList, catalog, entity);

            // Create the finder method to return all rows
            Creator.CreateReadListMethod(name, database, table, lobSystem.Name,referenceList, catalog, entity);

            // Publish the newly created Entity to the BCS Metadata Store.
            entity.Activate();
        }

        /// <summary>
        /// Creates the finder Method, specify the query it will use, and define the output parameters associated with it.
        /// The finder Method returns all of the rows of data from the data source which its query defines.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="lobSystemName"></param>
        /// <param name="referenceList"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadListMethod(string name, string database, string table, string lobSystemName, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            string listMethodName = String.Format("Get{0}s", name);
            string listMethodEntity = name + "s";
            string itemMethodEntity = name;

            // Create the Finder method 
            Method getListMethod = entity.Methods.Create(listMethodName, true, false, listMethodName);

            // Specify the query
            // "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer]"             
            string queryAllItemsString = "SELECT ";

            foreach(ExternalColumnReference reference in referenceList)
            {
                queryAllItemsString += "[" + reference.SourceName + "], ";
            }
            queryAllItemsString = queryAllItemsString.Substring(0, queryAllItemsString.Length - 2);
            queryAllItemsString += " FROM [" + database + "][dbo][" + table + "]";

            getListMethod.Properties.Add("RdbCommandText", queryAllItemsString);

            // Set the command type 
            getListMethod.Properties.Add("RdbCommandType", "Text");

            // Create the Entity return parameter
            Parameter modelParameter = getListMethod.Parameters.Create(name, true, DirectionType.Return); // e.g. Customer

            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor =
                modelParameter.CreateRootTypeDescriptor(
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

            foreach (ExternalColumnReference reference in referenceList)
            {
                IdentifierReference identityReference = null;

                if (reference.IsKey)
                {
                    identityReference = new IdentifierReference(reference.SourceName, new EntityReference(lobSystemName, itemMethodEntity, catalog), catalog); // "AdventureWorks" // "Customer"
                }

                returnRootElementTypeDescriptor.ChildTypeDescriptors.Create(
                    reference.SourceName, 
                    true, 
                    reference.Type,
                    reference.SourceName, 
                    identityReference, 
                    null, 
                    TypeDescriptorFlags.None, 
                    null
                );
            }

            getListMethod.MethodInstances.Create(listMethodName, true, returnRootCollectionTypeDescriptor, MethodInstanceType.Finder, true);
        }

        /// <summary>
        /// Create the specific finder Method, specify the query it will use, and define the input and output parameters associated with it.
        /// The specific finder Method returns exactly one row of data from the data source, given an identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="lobSystemName"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadItemMethod(string name, string database, string table, string lobSystemName, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            string itemMethodName = "Get" + name;
            string itemMethodEntity = name;
            string listMethodEntity = name + "s";

            ExternalColumnReference keyColumn = null;

            Method getItemMethod = entity.Methods.Create(itemMethodName, true, false, itemMethodName);

            // Specify the query 
            // "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer] WHERE [CustomerId] = @CustomerId"
            string querySingleItemString = "SELECT ";
            string whereClause = "";

            foreach (ExternalColumnReference reference in referenceList)
            {
                querySingleItemString += "[" + reference.SourceName + "], ";

                if(reference.IsKey)
                {
                    keyColumn = reference;
                    whereClause = "[" + reference.SourceName + "] = @" + reference.DestinationName;
                }
            }
            querySingleItemString = querySingleItemString.Substring(0, querySingleItemString.Length - 2);
            querySingleItemString += " FROM [" + database + "][dbo][" + table + "] WHERE " + whereClause;

            getItemMethod.Properties.Add("RdbCommandText", querySingleItemString);

            // Set the command type 
            getItemMethod.Properties.Add("RdbCommandType", "Text");

            // Create the EntityID input parameter 
            if (keyColumn == null)
                throw new NullReferenceException("keyColumn is not set.");

            string idParameter = "@" + keyColumn.DestinationName;
            Parameter entityIdParameter = getItemMethod.Parameters.Create(idParameter, true, DirectionType.In);

            // Create the TypeDescriptor for the EntityID parameter 
            entityIdParameter.CreateRootTypeDescriptor(
                keyColumn.SourceName, 
                true,
                keyColumn.Type,
                keyColumn.SourceName, 
                new IdentifierReference(keyColumn.SourceName, new EntityReference(lobSystemName, itemMethodEntity, catalog), catalog), // "AdventureWorks" // "Customer"
                null, 
                TypeDescriptorFlags.None, 
                null, 
                catalog); // "CustomerId"

            // Create the Entity return parameter 
            Parameter modelParameter = getItemMethod.Parameters.Create(itemMethodEntity, true, DirectionType.Return); // "Customer"

            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor =
                modelParameter.CreateRootTypeDescriptor(
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


            foreach (ExternalColumnReference reference in referenceList)
            {
                IdentifierReference identityReference = null;

                if (reference.IsKey)
                {
                    identityReference = new IdentifierReference(reference.SourceName, new EntityReference(lobSystemName, itemMethodEntity, catalog), catalog); // "AdventureWorks" // "Customer"
                }

                returnRootElementTypeDescriptor.ChildTypeDescriptors.Create(
                    reference.SourceName,
                    true,
                    reference.Type,
                    reference.SourceName,
                    identityReference,
                    null,
                    TypeDescriptorFlags.None,
                    null
                );
            }

            // Create the specific finder method instance 
            getItemMethod.MethodInstances.Create(itemMethodName, true, returnRootElementTypeDescriptor, MethodInstanceType.SpecificFinder, true); // getCustomer
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Boolean DeleteContentType(string name)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);

            SPServiceContext context = SPServiceContext.GetContext(web.Site);
            AdministrationMetadataCatalog catalog =
                service.GetAdministrationMetadataCatalog(context);

            EntityCollection availableEcts = catalog.GetEntities("*", "*", true);

            foreach (var entity in availableEcts)
            {
                if (entity.Name == name)
                {
                    entity.Delete();
                    return true;
                }
            }

            return false;
        }
    }
 }
  