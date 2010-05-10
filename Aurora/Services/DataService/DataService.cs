﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Aurora.DataManager.Migration;
using log4net;
using Nini.Config;
using Aurora.Framework;
using Aurora.DataManager;
using Aurora.DataManager.MySQL;
using Aurora.DataManager.SQLite;
using OpenSim.Framework;

namespace Aurora.Services.DataService
{
    public class LocalDataService: IDataService
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal IConfig m_config;
        string PluginModule = "";
        string ConnectionString = "";

        public void Initialise(IConfigSource source)
        {
            m_config = source.Configs["AuroraData"];
            if (null == m_config)
            {
                m_log.Error("[AuroraData]: no data plugin found!");
                return;
            }
            
            PluginModule = m_config.GetString("PluginModule", "");
            ConnectionString = m_config.GetString("ConnectionString", "");
            if (PluginModule == "MySQL")
            {
                DataManager.DataManager.DataSessionProvider = new DataSessionProvider(DataManagerTechnology.MySql, ConnectionString);
                MySQLDataLoader GenericData = new MySQLDataLoader();
                GenericData.ConnectToDatabase(ConnectionString);

                var migrationManager = new MigrationManager(DataManager.DataManager.DataSessionProvider, GenericData);
                migrationManager.DetermineOperation();
                migrationManager.ExecuteOperation();

                MySQLProfile ProfileData = new MySQLProfile();
                ProfileData.ConnectToDatabase(ConnectionString);
                MySQLRegion RegionData = new MySQLRegion();
                RegionData.ConnectToDatabase(ConnectionString);
                MySQLEstate EstateData = new MySQLEstate();
                EstateData.ConnectToDatabase(ConnectionString);
                Aurora.DataManager.DataManager.SetDefaultGenericDataPlugin(GenericData);
                Aurora.DataManager.DataManager.SetDefaultProfilePlugin((IProfileData)ProfileData);
                Aurora.DataManager.DataManager.SetDefaultRegionPlugin((IRegionData)RegionData);
                Aurora.DataManager.DataManager.SetDefaultEstatePlugin(EstateData);
            }
            else if (PluginModule == "SQLite")
            {
                DataManager.DataManager.DataSessionProvider = new DataSessionProvider(DataManagerTechnology.SQLite, ConnectionString);
                SQLiteLoader GenericData = new SQLiteLoader();
                GenericData.ConnectToDatabase(ConnectionString);

                //SQLite needs a second database for ScriptSaves, otherwise it locks it up...
                string ScriptConnectionString = m_config.GetString("ScriptConnectionString", "");
                DataManager.DataManager.StateSaveDataSessionProvider = new DataSessionProvider(DataManagerTechnology.SQLite, ScriptConnectionString);
                SQLiteStateSaver ScriptSaverData = new SQLiteStateSaver();
                ScriptSaverData.ConnectToDatabase(ScriptConnectionString);

                var migrationManager = new MigrationManager(DataManager.DataManager.DataSessionProvider, GenericData);
                migrationManager.DetermineOperation();
                migrationManager.ExecuteOperation();

                var statesavemigrationManager = new MigrationManager(DataManager.DataManager.StateSaveDataSessionProvider, ScriptSaverData);
                statesavemigrationManager.DetermineOperation();
                statesavemigrationManager.ExecuteOperation();

                SQLiteProfile ProfileData = new SQLiteProfile();
                ProfileData.ConnectToDatabase(ConnectionString);
                SQLiteRegion RegionData = new SQLiteRegion();
                RegionData.ConnectToDatabase(ConnectionString);
                SQLiteEstate EstateData = new SQLiteEstate();
                EstateData.ConnectToDatabase(ConnectionString);

                Aurora.DataManager.DataManager.SetDefaultGenericDataPlugin(GenericData);
                Aurora.DataManager.DataManager.SetDefaultProfilePlugin((IProfileData)ProfileData);
                Aurora.DataManager.DataManager.SetDefaultRegionPlugin((IRegionData)RegionData);
                Aurora.DataManager.DataManager.SetDefaultEstatePlugin(EstateData);
            }

            string Connector = m_config.GetString("Connector", "LocalConnector");
            if (Connector == "LocalConnector")
            {
                DataManager.DataManager.IGridConnector = new LocalGridConnector();
                DataManager.DataManager.IProfileConnector = new LocalProfileConnector();
                DataManager.DataManager.IAgentConnector = new LocalAgentConnector();
                return;
            }
            if (Connector != "RemoteConnector")
            {
                m_log.Error("[AuroraDataService]: No Connector found with that name!");
                return;
            }
            string RemoteConnectionString = m_config.GetString("ConnectionString", "");
            
        }

        public string Name
        {
            get { return "DataService"; }
        }
        public IGenericData GetGenericPlugin()
        {
            return Aurora.DataManager.DataManager.DefaultGenericPlugin;
        }
        public void SetGenericDataPlugin(IGenericData Plugin)
        {
            Aurora.DataManager.DataManager.DefaultGenericPlugin = Plugin;
        }
        public IEstateData GetEstatePlugin()
        {
            return Aurora.DataManager.DataManager.DefaultEstatePlugin;
        }
        public void SetEstatePlugin(IEstateData Plugin)
        {
            Aurora.DataManager.DataManager.DefaultEstatePlugin = Plugin;
        }
        public IProfileData GetProfilePlugin()
        {
            return Aurora.DataManager.DataManager.DefaultProfilePlugin;
        }
        public void SetProfilePlugin(IProfileData Plugin)
        {
            Aurora.DataManager.DataManager.DefaultProfilePlugin = Plugin;
        }
        public IRegionData GetRegionPlugin()
        {
            return Aurora.DataManager.DataManager.DefaultRegionPlugin;
        }
        public void SetRegionPlugin(IRegionData Plugin)
        {
            Aurora.DataManager.DataManager.DefaultRegionPlugin = Plugin;
        }
    }
}
