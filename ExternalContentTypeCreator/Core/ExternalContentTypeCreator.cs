using Microsoft.SharePoint.BusinessData.Administration.Client;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using System;

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


        public void createNewContentType()
        {
            AdministrationMetadataCatalog catalog = AdministrationMetadataCatalog.GetCatalog("http://sharepoint-dev/");

            // Create a new customer model 
            Model customerModel = Model.Create("CustomerModel", true, catalog);

            // Make a new Customer LobSystem
            LobSystem awLobSystem = customerModel.OwnedReferencedLobSystems.Create("Customer", true, SystemType.Database);

            // Make a new AdventureWorks LobSystemInstance 
            LobSystemInstance awLobSystemInstance = awLobSystem.LobSystemInstances.Create("AdventureWorks", true);

            // Set the connection properties 
            awLobSystemInstance.Properties.Add("AuthenticationMode", "PassThrough");
            awLobSystemInstance.Properties.Add("DatabaseAccessProvider", "SqlServer");
            awLobSystemInstance.Properties.Add("RdbConnection Data Source", "DEV1");
            awLobSystemInstance.Properties.Add("RdbConnection Initial Catalog", "Customers");
            awLobSystemInstance.Properties.Add("RdbConnection Integrated Security", "SSPI");
            awLobSystemInstance.Properties.Add("RdbConnection Pooling", "true");

            /* Create the Entity Next, create the Entityto represent the Customers table and define which column(s) make up the identifier for the Entity. */

            // Create a new Customer Entity 
            Entity customerEntity = Entity.Create("Customer", "AdventureWorks", true, new Version("1.0.0.0"), 10000, CacheUsage.Default, awLobSystem, customerModel, catalog);
            // Set the identifier - CustomerID column 
            customerEntity.Identifiers.Create("CustomerId", true, "System.Int32");

            /* Define the Specific Finder Method, Parameters and Type Descriptors
            Next, create the specific finder Method, specify the query it will use, and define the input and output parameters associated with it. 
            The specific finder Method returns exactly one row of data from the data source, given an identifier. */

            // Create the specific finder method 
            Method getCustomerMethod = customerEntity.Methods.Create("GetCustomer", true, false, "GetCustomer");

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
            Method getCustomersMethod = customerEntity.Methods.Create("GetCustomers", true, false, "GetCustomers");

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

            /* Finally, commit the changes to the BCS Metadata Store. */

            // Publish the Customer Entity 
            customerEntity.Activate();
        }
    }
 }
  