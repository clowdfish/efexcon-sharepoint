﻿//using Microsoft.SharePoint.BusinessData.Administration.Client;
using Microsoft.SharePoint.BusinessData.Administration;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using System;
using Microsoft.SharePoint.BusinessData.SharedService;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
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

        public static List<String> getTablesForLobSystem(LobSystem lobSystem)
        {
            List<LobSystemInstance> list = lobSystem.LobSystemInstances.ToList<LobSystemInstance>();

            if (list.Count() == 0)
                throw new Exception("No LobSystemInstance available for LobSystem.");

            LobSystemInstance instance = list[0];

            string server = "";
            string database = "";
            string username = "";
            string password = "";

            foreach (Property prop in instance.Properties)
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


            var connectionString =
                String.Format("Server={0};Database={1};User Id={2};Password={3};",
                    server, database, username, password);

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
    }
 }
  