using Microsoft.SharePoint.BusinessData.Administration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Microsoft.SharePoint;

namespace EFEXCON.ExternalLookup.Helper
{
    /// <summary>
    /// Class Creator.
    /// </summary>
    public class SqlHelper
    {   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <returns></returns>
        private static string GetDatabaseConnectionString(LobSystem lobSystem)
        {
            string server = "";
            string database = "";

            foreach (Property prop in SqlHelper.GetLobSystemInstanceProperties(lobSystem))
            {
                if (prop.Name == "RdbConnection Data Source")
                    server = prop.Value.ToString();

                if (prop.Name == "RdbConnection Initial Catalog")
                    database = prop.Value.ToString();
            }

            if (String.IsNullOrEmpty(server))
                throw new NoNullAllowedException("Server string is not defined.");

            if (String.IsNullOrEmpty(database))
                throw new NoNullAllowedException("Database string is not defined.");

            // Good read on connection strings and integrated security:
            // http://stackoverflow.com/questions/1229691/difference-between-integrated-security-true-and-integrated-security-sspi

            return String.Format("Server={0};Database={1};Integrated Security=SSPI;",
                server, database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <returns></returns>
        public static Microsoft.SharePoint.BusinessData.Administration.PropertyCollection GetLobSystemInstanceProperties(LobSystem lobSystem)
        {
            List<LobSystemInstance> list = lobSystem.LobSystemInstances.ToList();

            if (!list.Any())
                throw new Exception("No LobSystemInstance available for LobSystem.");

            LobSystemInstance instance = list[0];

            return instance.Properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <returns></returns>
        public static List<String> GetTablesForLobSystem(LobSystem lobSystem, Credentials credentials)
        {
            var connectionString = GetDatabaseConnectionString(lobSystem);

            string database =
                connectionString.Split(';').Where(x => x.StartsWith("Database")).Select(x => x.Split('=')[1]).ToArray()[
                    0];

            if (String.IsNullOrEmpty(database))
                throw new NoNullAllowedException("Database string is not defined.");

            try
            {
                using (new Impersonator(credentials.User, credentials.Domain, credentials.Password))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        var commandString = String.Format("SELECT TABLE_NAME FROM {0}.INFORMATION_SCHEMA.Tables",
                            database);

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
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobSystem"></param>
        /// <param name="credentials"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<TableColumn> GetTableStructure(LobSystem lobSystem, Credentials credentials, string tableName)
        {
            var connectionString = GetDatabaseConnectionString(lobSystem);

            try
            {
                using (new Impersonator(credentials.User, credentials.Domain, credentials.Password))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        var commandString =
                            String.Format(
                                "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '{0}'",
                                tableName);

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
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
 }
  