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
using Microsoft.SharePoint.Utilities;

namespace EFEXCON.ExternalLookup.Core
{
    /// <summary>
    /// Class Creator.
    /// </summary>
    public class Creator
    {
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
        /// 
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
            string listMethodName = String.Format("Get{0}s", name);
            string listMethodEntity = name + "s";
            string itemMethodEntity = name;

            // Create the Finder method 
            Method getListMethod = entity.Methods.Create(listMethodName, true, false, itemMethodEntity);

            // Specify the query
            // "SELECT [CustomerId] , [FirstName] , [LastName] , [Phone] , [EmailAddress] , [CompanyName] FROM [Customers].[SalesLT].[Customer]"             
            string queryAllItemsString = "SELECT TOP(@MaxRowsReturned) ";

            foreach(ExternalColumnReference reference in referenceList)
            {
                queryAllItemsString += "[" + reference.SourceName + "], ";
            }
            queryAllItemsString = queryAllItemsString.Substring(0, queryAllItemsString.Length - 2);
            queryAllItemsString += " FROM [" + table + "]";

            getListMethod.Properties.Add("RdbCommandText", queryAllItemsString);

            // Set the command type 
            getListMethod.Properties.Add("RdbCommandType", "Text");

            // Create a Filter so that we can limit the number 
            // of rows returned;
            // otherwise we may exceed the list query size threshold.
            FilterDescriptor limitRowsReturnedFilter =
                getListMethod.FilterDescriptors.Create(
                    "RowsReturnedLimit", true, FilterType.Limit, null);

            limitRowsReturnedFilter.Properties.Add(
                "IsDefault", true);

            // Create the RowsToRetrieve input parameter.
            Parameter maxRowsReturnedParameter =
                getListMethod.Parameters.Create(
                "@MaxRowsReturned", true, DirectionType.In);

            // Create the TypeDescriptor for the MaxRowsReturned parameter.
            // using the Filter we have created.
            TypeDescriptor maxRowsReturnedTypeDescriptor =
                maxRowsReturnedParameter.CreateRootTypeDescriptor(
                "MaxRowsReturned",
                true,
                "System.Int64",
                "MaxRowsReturned",
                null,
                limitRowsReturnedFilter,
                TypeDescriptorFlags.None,
                null,
                catalog);

            // Create the Entity return parameter
            Parameter modelParameter = getListMethod.Parameters.Create(name, true, DirectionType.Return);

            // Create the TypeDescriptors for the Entity return parameter 
            TypeDescriptor returnRootCollectionTypeDescriptor =
                modelParameter.CreateRootTypeDescriptor(
                    listMethodEntity, 
                    true, 
                    "System.Data.IDataReader, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
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
                    "System.Data.IDataRecord, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
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

                FilterDescriptor filter = null;

                if (reference.IsSearchField)
                {
                    FilterType filterType = reference.Type == "System.String" ? FilterType.Wildcard : FilterType.Comparison; 
                    filter = getListMethod.FilterDescriptors.Create(
                        reference.DestinationName + "Filter", true, filterType, reference.DestinationName);

                    //filter.Properties.Add("IgnoreFilterIfValueIs", null); // leads to NPE
                    //filter.Properties.Add("DefaultValue", null);  // leads to NPE
                    filter.Properties.Add("LogicalOperatorWithPrevious", "and");
                    filter.Properties.Add("IsDefault", false);
                    
                    // Create the RowsToRetrieve input parameter.
                    Parameter filterParamter =
                        getListMethod.Parameters.Create(
                        "@" + reference.DestinationName + "Filter", true, DirectionType.In);

                    // Create the TypeDescriptor for the MaxRowsReturned parameter.
                    // using the Filter we have created.
                    TypeDescriptor filterTypeDescriptor =
                        filterParamter.CreateRootTypeDescriptor(
                        reference.DestinationName + "Filter",
                        true,
                        reference.Type,
                        reference.DestinationName + "Filter",
                        null,
                        filter,
                        TypeDescriptorFlags.None,
                        null,
                        catalog);
                        
                }                

                var childTypeDescriptor = returnRootElementTypeDescriptor.ChildTypeDescriptors.Create(
                    reference.DestinationName, 
                    true, 
                    reference.Type,
                    reference.DestinationName, 
                    identityReference, 
                    null, // filter
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

            // Set the default value for the number of rows 
            // to be returned filter.
            // NOTE: The method instance needs to be created first 
            // before we can set the default value.
            maxRowsReturnedTypeDescriptor.SetDefaultValue(
                readListMethodInstance.Id, Int64.Parse("30"));
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
            querySingleItemString += " FROM [" + table + "] WHERE " + whereClause;

            getItemMethod.Properties.Add("RdbCommandText", querySingleItemString);

            // Set the command type 
            getItemMethod.Properties.Add("RdbCommandType", "Text");

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
                new IdentifierReference(keyColumn.DestinationName, new EntityReference("EFEXCON.ExternalLookup", itemMethodEntity, catalog), catalog), // "AdventureWorks" // "Customer"
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
                    identityReference = new IdentifierReference(reference.DestinationName, new EntityReference("EFEXCON.ExternalLookup", itemMethodEntity, catalog), catalog); // "AdventureWorks" // "Customer"
                }

                returnRootElementTypeDescriptor.ChildTypeDescriptors.Create(
                    reference.DestinationName,
                    true,
                    reference.Type,
                    reference.DestinationName,
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