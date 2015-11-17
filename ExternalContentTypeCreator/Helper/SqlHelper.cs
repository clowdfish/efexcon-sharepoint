using Microsoft.SharePoint.BusinessData.Administration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Linq;

namespace EFEXCON.ExternalLookup.Helper
{
    /// <summary>
    /// Class Creator.
    /// </summary>
    public class SqlHelper
    {

        public void Initialize()
        {

        }

        private static string getDatabaseConnectionString(LobSystem lobSystem)
        {
            string server = "";
            string database = "";
            string username = "";
            string password = "";

            foreach (Property prop in SqlHelper.getLobSystemInstanceProperties(lobSystem))
            {
                if (prop.Name == "RdbConnection Data Source")
                    server = prop.Value.ToString();

                if (prop.Name == "RdbConnection Initial Catalog")
                    database = prop.Value.ToString();

                if (prop.Name == "RdbConnection User ID")
                    username = prop.Value.ToString();

                if (prop.Name == "RdbConnection Password")
                    password = prop.Value.ToString();
            }

            if (String.IsNullOrEmpty(server))
                throw new ArgumentNullException("Server string is not defined.");

            if (String.IsNullOrEmpty(database))
                throw new ArgumentNullException("Database string is not defined.");

            if (String.IsNullOrEmpty(username))
                throw new ArgumentNullException("Username string is not defined.");

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException("Password string is not defined.");


            return String.Format("Server={0};Database={1};User Id={2};Password={3};",
                    server, database, username, password);
        }

        public static Microsoft.SharePoint.BusinessData.Administration.PropertyCollection getLobSystemInstanceProperties(LobSystem lobSystem)
        {
            List<LobSystemInstance> list = lobSystem.LobSystemInstances.ToList<LobSystemInstance>();

            if (list.Count() == 0)
                throw new Exception("No LobSystemInstance available for LobSystem.");

            LobSystemInstance instance = list[0];

            return instance.Properties;
        }

        public static List<String> getTablesForLobSystem(LobSystem lobSystem)
        {
            var connectionString = SqlHelper.getDatabaseConnectionString(lobSystem);

            string database = connectionString.Split(';').Where(x => x.StartsWith("Database")).Select(x => x.Split('=')[1]).ToArray()[0];
      
            if (String.IsNullOrEmpty(database))
                throw new ArgumentNullException("Database string is not defined.");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var commandString = String.Format("SELECT TABLE_NAME FROM {0}.INFORMATION_SCHEMA.Tables", database);

                    SqlCommand cmd = new SqlCommand(commandString, connection);
                    connection.Open();

                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    var result = new List<String>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<TableColumn> getTableStructure(LobSystem lobSystem, string tableName)
        {
            var connectionString = SqlHelper.getDatabaseConnectionString(lobSystem);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var commandString = String.Format("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '{0}'", tableName);

                    SqlCommand cmd = new SqlCommand(commandString, connection);
                    connection.Open();

                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    var result = new List<TableColumn>();
                    while (reader.Read())
                    {
                        result.Add(new TableColumn()
                        {
                            Name = reader.GetString(0),
                            Type = reader.GetString(1)
                        });
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    public class TableColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
 }
  