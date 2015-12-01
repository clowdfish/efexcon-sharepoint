using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFEXCON.ExternalLookup.Core
{
    using Helper;

    /// <summary>
    /// Class Creator.
    /// </summary>
    public class Creator
    {
        /// <summary>
        /// Get list of all available external content types.
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
        /// Creates a new LobSystem object and adds it to the Business Data 
        /// Connectivity Service.
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
                if (lobSystem.Name == name && lobSystem.SystemType == type)
                {
                    return lobSystem;
                }
            }

            // if no LobSystem was found, create a new one and return it
            return availableLobSystems.Create(name, true, type);
        }

        /// <summary>
        /// Return a LobSystem object or null, if no LobSystem with the given 
        /// name exists.
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
        /// Delete LobSystem object with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Boolean DeleteLobSystem(string name)
        {
            SPWeb web = SPContext.Current.Web;
            BdcService service = SPFarm.Local.Services.GetValue<BdcService>(String.Empty);
            SPServiceContext context = SPServiceContext.GetContext(web.Site);

            LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

            foreach (var lobSystem in availableLobSystems)
            {
                if (lobSystem.Name == name)
                {            
                    try
                    {
                        lobSystem.Delete();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Create a new LobSystemInstance for the given LobSystem object.
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="sssId"></param>
        /// <param name="providerImplementation"></param>
        /// <returns></returns>
        public static LobSystemInstance CreateLobSystemInstance(LobSystem lobSystem, string server, string database, string sssId, string providerImplementation)
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

            lobSystemInstance.Properties.Add("AuthenticationMode", "WindowsCredentials");
            lobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            lobSystemInstance.Properties.Add("RdbConnection Data Source", server);
            lobSystemInstance.Properties.Add("RdbConnection Initial Catalog", database);
            lobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            lobSystemInstance.Properties.Add("RdbConnection Pooling", "false");
            lobSystemInstance.Properties.Add("SsoApplicationId", sssId);
            lobSystemInstance.Properties.Add("SsoProviderImplementation", providerImplementation);

            return lobSystemInstance;
        }

        /// <summary>
        /// Get a list of all available LobSystem objects.
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
        /// Create a new external content type in the Business Data Connectivity Service.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        /// <param name="referenceList"></param>
        /// <param name="lobSystem"></param>
        public static void CreateNewContentType(string name, string table, List<ExternalColumnReference> referenceList, LobSystem lobSystem)
        {
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;

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
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Helper_LobSystem", "Resources", language);
                throw new LobGenericException(message);
            }
                        
            // Create a new Entity 
            Entity entity = Entity.Create(name, "EFEXCON.ExternalLookup", true, new Version("1.0.0.0"), 10000, CacheUsage.Default, lobSystem, model, catalog);

            // Set the identifier
            ExternalColumnReference keyReference = referenceList.First(x => x.IsKey == true);
            entity.Identifiers.Create(keyReference.DestinationName, true, keyReference.Type);

            // Create the specific finder method to return one specific element
            Creator.CreateReadItemMethod(name, table, lobSystem.Name, referenceList, catalog, entity);

            // Create the finder method to return all rows
            Creator.CreateReadListMethod(name, table, lobSystem.Name, referenceList, catalog, entity);

            // Validate the entity before activating it
            entity.Validate();

            // Publish the newly created Entity to the BCS Metadata Store.
            entity.Activate();
        }

        /// <summary>
        /// Creates the finder Method, specify the query it will use, and define the output parameters associated with it.
        /// The finder Method returns all of the rows of data from the data source which its query defines.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        /// <param name="lobSystemName"></param>
        /// <param name="referenceList"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadListMethod(string name, string table, string lobSystemName, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            string listMethodName = String.Format("Get{0}List", name);
            string listMethodEntity = name + "List";
            string itemMethodEntity = name;

            var identifierField = referenceList.Where(x => x.IsKey).ToList().First();
            if (identifierField == null)
                throw new NullReferenceException("Could not get identifier field.");

            // Create the Finder method 
            Method getListMethod = entity.Methods.Create(listMethodName, true, false, table); // itemMethodEntity

            // Specify the query
            // "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer]"             
            string queryAllItemsString = "SELECT TOP(@" + identifierField.DestinationName + ") ";

            foreach(ExternalColumnReference reference in referenceList)
            {
                queryAllItemsString += "[" + reference.SourceName + "], ";
            }
            queryAllItemsString = queryAllItemsString.Substring(0, queryAllItemsString.Length - 2);
            queryAllItemsString += " FROM [" + table + "]";

            var whereClause = " WHERE";
            foreach (ExternalColumnReference reference in referenceList)
            {
                if (reference.IsSearchField)
                {
                    whereClause += String.Format(" ((@{1} IS NULL) OR ((@{1} IS NULL AND [{0}] IS NULL) OR [{0}] LIKE @{1})) AND", reference.SourceName, reference.DestinationName);
                }
            }

            if (whereClause.Length == 7)
                whereClause = "";
            else
            {
                whereClause = whereClause.Substring(0, whereClause.Length - 4);
            }    

            queryAllItemsString += whereClause;

            // Set method properties
            getListMethod.Properties.Add("RdbCommandText", queryAllItemsString);                       
            getListMethod.Properties.Add("RdbCommandType", "Text");
            getListMethod.Properties.Add("BackEndObjectType", "SqlServerTable");
            getListMethod.Properties.Add("BackEndObject", table);
            getListMethod.Properties.Add("Schema", "dbo");

            // Create the Entity return parameter
            Parameter modelParameter = getListMethod.Parameters.Create(name, true, DirectionType.Return);

            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor =
                modelParameter.CreateRootTypeDescriptor(
                    listMethodEntity, 
                    true, 
                    "System.Data.IDataReader, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    listMethodEntity, 
                    null, 
                    null, // filter descriptor
                    TypeDescriptorFlags.IsCollection, 
                    null, 
                    catalog);

            TypeDescriptor returnRootElementTypeDescriptor = 
                returnRootCollectionTypeDescriptor.ChildTypeDescriptors.Create(
                    itemMethodEntity, 
                    true, 
                    "System.Data.IDataRecord, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    itemMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.None, 
                    null);

            // Create a Filter so that we can limit the number 
            // of rows returned;          
            // otherwise we may exceed the list query size threshold.
            FilterDescriptor limitRowsReturnedFilter =
                getListMethod.FilterDescriptors.Create(
                    "RowsReturnedLimit", true, FilterType.Limit, identifierField.SourceName);

            limitRowsReturnedFilter.Properties.Add("IsDefault", false);
            limitRowsReturnedFilter.Properties.Add("UsedForDisambiguation", false);

            // Create the RowsToRetrieve input parameter.
            Parameter identifierParameter =
                getListMethod.Parameters.Create(
                "@" + identifierField.DestinationName, true, DirectionType.In);

            // Create the TypeDescriptor for the MaxRowsReturned parameter.
            // using the Filter we have created.
            TypeDescriptor maxRowsReturnedTypeDescriptor =
                identifierParameter.CreateRootTypeDescriptor(
                    identifierField.DestinationName,
                    true,
                    "System.Int64",
                    identifierField.DestinationName, //"MaxRowsReturned"
                    null,
                    limitRowsReturnedFilter,
                    TypeDescriptorFlags.None,
                    null,
                    catalog);

            var typeDescriptorList = new List<TypeDescriptor>();
            var counter = 0;
            foreach (ExternalColumnReference reference in referenceList)
            {
                IdentifierReference identityReference = null;
                if(reference.IsKey) 
                    identityReference = new IdentifierReference(reference.DestinationName, new EntityReference("EFEXCON.ExternalLookup", itemMethodEntity, catalog), catalog);

                if (reference.IsSearchField)
                {
                    FilterType filterType = reference.Type == "System.String" ? FilterType.Wildcard : FilterType.Comparison;
                    FilterDescriptor filter = getListMethod.FilterDescriptors.Create(
                        reference.DestinationName + "Filter", true, filterType, reference.SourceName);

                    filter.Properties.Add("CaseSensitive", false);
                    filter.Properties.Add("IsDefault", false);
                    filter.Properties.Add("UsedForDisambiguation", false);                    
                    filter.Properties.Add("UseValueAsDontCare", true);
                    filter.Properties.Add("DontCareValue", "");

                    // Create the filter input parameter.
                    Parameter filterParameter = reference.IsKey ?
                        identifierParameter : getListMethod.Parameters.Create(
                            "@" + reference.DestinationName, true, DirectionType.In);  

                    // Create the TypeDescriptor for the filter parameter.
                    TypeDescriptor filterParamTypeDescriptor =
                        filterParameter.CreateRootTypeDescriptor(
                        reference.SourceName,
                        true,
                        reference.Type,
                        reference.SourceName,
                        null,
                        filter,
                        TypeDescriptorFlags.None,
                        null,
                        catalog);

                    if(reference.Type == "System.String")
                        typeDescriptorList.Add(filterParamTypeDescriptor);

                    if (counter > 0)
                        filterParamTypeDescriptor.Properties.Add("LogicalOperatorWithPrevious", "And");

                    counter++;
                }
             
                var childTypeDescriptor = returnRootElementTypeDescriptor.ChildTypeDescriptors.Create(
                    reference.SourceName,
                    true,
                    reference.Type,
                    reference.SourceName,
                    identityReference,
                    null,
                    TypeDescriptorFlags.None,
                    null
                );
              
                childTypeDescriptor.Properties.Add("ShowInPicker", true);                
            }        

            // Create the finder method instance
            MethodInstance readListMethodInstance =
                getListMethod.MethodInstances.Create(
                    listMethodName, 
                    true, 
                    returnRootCollectionTypeDescriptor, 
                    MethodInstanceType.Finder, 
                    true);

            readListMethodInstance.Properties.Add("RootFinder", "");

            // Set the default value for the number of rows 
            // to be returned filter.
            // NOTE: The method instance needs to be created first 
            // before we can set the default value.
            maxRowsReturnedTypeDescriptor.SetDefaultValue(
                readListMethodInstance.Id, Int64.Parse("30"));

            foreach(var typeDescriptor in typeDescriptorList)
            {
                typeDescriptor.SetDefaultValue(
                    readListMethodInstance.Id, "");
            }
        }

        /// <summary>
        /// Create the specific finder Method, specify the query it will use, and define the input and output parameters associated with it.
        /// The specific finder Method returns exactly one row of data from the data source, given an identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        /// <param name="lobSystemName"></param>
        /// <param name="catalog"></param>
        /// <param name="entity"></param>
        private static void CreateReadItemMethod(string name, string table, string lobSystemName, List<ExternalColumnReference> referenceList, AdministrationMetadataCatalog catalog, Entity entity)
        {
            uint language = SPContext.Current.Web != null ? SPContext.Current.Web.Language : 1033;

            string itemMethodName = "Get" + name;
            string listMethodEntity = name + "List";
            string itemMethodEntity = name;

            ExternalColumnReference keyColumn = null;

            Method getItemMethod = entity.Methods.Create(itemMethodName, true, false, table);

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
            querySingleItemString += " FROM [" + table + "] WHERE " + whereClause;

            // Set the method properties
            getItemMethod.Properties.Add("RdbCommandText", querySingleItemString);            
            getItemMethod.Properties.Add("RdbCommandType", "Text");
            getItemMethod.Properties.Add("BackEndObjectType", "SqlServerTable");
            getItemMethod.Properties.Add("BackEndObject", table);
            getItemMethod.Properties.Add("Schema", "dbo");

            // Create the EntityID input parameter 
            if (keyColumn == null)
            {
                var message = SPUtility.GetLocalizedString("$Resources:ExternalLookup_Creator_KeyColumn", "Resources", language);
                throw new NullReferenceException(message);
            }

            string idParameter = "@" + keyColumn.DestinationName;
            Parameter entityIdParameter = getItemMethod.Parameters.Create(idParameter, true, DirectionType.In);

            // Create the TypeDescriptor for the EntityID parameter 
            entityIdParameter.CreateRootTypeDescriptor(
                keyColumn.DestinationName, 
                true,
                keyColumn.Type,
                keyColumn.DestinationName, 
                new IdentifierReference(keyColumn.DestinationName, new EntityReference("EFEXCON.ExternalLookup", itemMethodEntity, catalog), catalog),
                null, 
                TypeDescriptorFlags.None, 
                null, 
                catalog);

            // Create the Entity return parameter 
            Parameter modelParameter = getItemMethod.Parameters.Create(itemMethodEntity, true, DirectionType.Return);

            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor =
                modelParameter.CreateRootTypeDescriptor(
                    listMethodEntity, 
                    true, 
                    "System.Data.IDataReader, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    listMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.IsCollection, 
                    null, 
                    catalog);

            TypeDescriptor returnRootElementTypeDescriptor = 
                returnRootCollectionTypeDescriptor.ChildTypeDescriptors.Create(
                    itemMethodEntity, 
                    true, 
                    "System.Data.IDataRecord, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                    itemMethodEntity, 
                    null, 
                    null, 
                    TypeDescriptorFlags.None, 
                    null);


            foreach (ExternalColumnReference reference in referenceList)
            {
                IdentifierReference identityReference = null;

                if (reference.IsKey)
                {
                    identityReference = new IdentifierReference(reference.DestinationName, new EntityReference("EFEXCON.ExternalLookup", itemMethodEntity, catalog), catalog);
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
            getItemMethod.MethodInstances.Create(itemMethodName, true, returnRootElementTypeDescriptor, MethodInstanceType.SpecificFinder, true);
        }
  
        /// <summary>
        /// Delete the external content type with the given name from the 
        /// Business Data Connectivity Service.
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
                    LobSystem lobSystemReference = entity.LobSystem;
                    LobSystemInstance instanceReference = lobSystemReference.LobSystemInstances.First();

                    var lobSystemName = "";
                    var lobSystemType = SystemType.Custom;

                    var instanceName = "";
                    var instanceIsCached = true;
                    var instanceProperties = new Dictionary<string, string>();

                    if (lobSystemReference != null && instanceReference != null)
                    {
                        lobSystemName = lobSystemReference.Name;
                        lobSystemType = lobSystemReference.SystemType;

                        instanceName = instanceReference.Name;
                        instanceIsCached = instanceReference.IsCached;                       

                        foreach (var prop in instanceReference.Properties)
                        {
                            instanceProperties.Add(prop.Name, prop.Value.ToString());
                        }
                    }

                    entity.Delete();

                    // check if LobSytem must be recreated
                    if (!string.IsNullOrEmpty(lobSystemName) && !string.IsNullOrEmpty(instanceName))
                    {
                        LobSystemCollection availableLobSystems = service.GetAdministrationMetadataCatalog(context).GetLobSystems("*");

                        var lobSystemStillThere = availableLobSystems.Where(x => x.Name == lobSystemName).Any();

                        if(!lobSystemStillThere)
                        {
                            // re-create LobSystem
                            var newLobSystem = availableLobSystems.Create(lobSystemName, true, lobSystemType);
                            var newInstance = newLobSystem.LobSystemInstances.Create(instanceName, instanceIsCached);

                            foreach(var item in instanceProperties)
                            {
                                newInstance.Properties.Add(item.Key, item.Value);
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
 }