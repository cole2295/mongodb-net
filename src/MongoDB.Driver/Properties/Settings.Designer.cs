﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.21006.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MongoDB.Driver.Properties
{


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        /// <summary>
        /// Gets the default host.
        /// </summary>
        /// <value>The default host.</value>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("127.0.0.1")]
        public string DefaultHost
        {
            get
            {
                return ((string)(this["DefaultHost"]));
            }
        }

        /// <summary>
        /// Gets the default port.
        /// </summary>
        /// <value>The default port.</value>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("27017")]
        public int DefaultPort
        {
            get
            {
                return ((int)(this["DefaultPort"]));
            }
        }

        /// <summary>
        /// Gets the default name of the database.
        /// </summary>
        /// <value>The default name of the database.</value>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Test")]
        public string DefaultDatabaseName
        {
            get
            {
                return ((string)(this["DefaultDatabaseName"]));
            }
        }
    }
}
