// *********************************************************************
// Created by : Latebound Constants Generator 1.2023.12.1 for XrmToolBox
// Author     : Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://mikefactorialdemo.crm.dynamics.com
// Filename   : C:\source\repos\NextGenPluginDemo\Types\EnvironmentVariableDefinition.cs
// Created    : 2024-07-03 17:09:03
// *********************************************************************

namespace NextGenDemo.Plugins.Types
{
    /// <summary>DisplayName: Environment Variable Definition, OwnershipType: UserOwned, IntroducedVersion: 1.0.0.0</summary>
    public static class environmentvariabledefinition
    {
        public const string EntityName = "environmentvariabledefinition";
        public const string EntityCollectionName = "environmentvariabledefinitions";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "environmentvariabledefinitionid";
        /// <summary>Type: String, RequiredLevel: SystemRequired, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "schemaname";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 150, Format: Text</summary>
        public const string apiid = "apiid";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: Component State, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string componentstate = "componentstate";
        /// <summary>Type: Lookup, RequiredLevel: None</summary>
        public const string connectionreferenceid = "connectionreferenceid";
        /// <summary>Type: ManagedProperty, RequiredLevel: SystemRequired</summary>
        public const string iscustomizable = "iscustomizable";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string defaultvalue = "defaultvalue";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string description = "description";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 100, Format: Text</summary>
        public const string displayname = "displayname";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string environmentvariabledefinitionidunique = "environmentvariabledefinitionidunique";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string hint = "hint";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 48, Format: VersionNumber</summary>
        public const string introducedversion = "introducedversion";
        /// <summary>Type: Boolean, RequiredLevel: SystemRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string ismanaged = "ismanaged";
        /// <summary>Type: Boolean, RequiredLevel: ApplicationRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string isrequired = "isrequired";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 150, Format: Text</summary>
        public const string parameterkey = "parameterkey";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: environmentvariabledefinition</summary>
        public const string parentdefinitionid = "parentdefinitionid";
        /// <summary>Type: DateTime, RequiredLevel: SystemRequired, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string overwritetime = "overwritetime";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Secret Store, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string secretstore = "secretstore";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string solutionid = "solutionid";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string supportingsolutionid = "supportingsolutionid";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string statecode = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string statuscode = "statuscode";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Type, OptionSetType: Picklist, DefaultFormValue: 100000000</summary>
        public const string type = "type";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string valueschema = "valueschema";

        #endregion Attributes

        #region Relationships

        /// <summary>Entity 1: "bot" Entity 2: "environmentvariabledefinition"</summary>
        public const string RelMM_bot_environmentvariabledefinition = "bot_environmentvariabledefinition";
        /// <summary>Entity 1: "botcomponent" Entity 2: "environmentvariabledefinition"</summary>
        public const string RelMM_botcomponent_environmentvariabledefinition = "botcomponent_environmentvariabledefinition";
        /// <summary>Entity 1: "msdyn_connectordatasource" Entity 2: "environmentvariabledefinition"</summary>
        public const string RelMM_msdyn_connectordatasource_environmentvariable = "msdyn_connectordatasource_environmentvariable";
        /// <summary>Parent: "environmentvariabledefinition" Child: "environmentvariabledefinition" Lookup: "parentdefinitionid"</summary>
        public const string Rel1M_envdefinition_envdefinition = "envdefinition_envdefinition";
        /// <summary>Parent: "environmentvariabledefinition" Child: "principalobjectattributeaccess" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_PrincipalObjectAttributeAccesses = "environmentvariabledefinition_PrincipalObjectAttributeAccesses";
        /// <summary>Parent: "environmentvariabledefinition" Child: "environmentvariablevalue" Lookup: "environmentvariabledefinitionid"</summary>
        public const string Rel1M_environmentvariabledefinition_environmentvariablevalue = "environmentvariabledefinition_environmentvariablevalue";
        /// <summary>Parent: "environmentvariabledefinition" Child: "powerbimashupparameter" Lookup: ""</summary>
        public const string Rel1M_envvardefinition_powerbimashupparameter = "envvardefinition_powerbimashupparameter";
        /// <summary>Parent: "environmentvariabledefinition" Child: "credential" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_credential_password = "environmentvariabledefinition_credential_password";
        /// <summary>Parent: "environmentvariabledefinition" Child: "credential" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_credential_username = "environmentvariabledefinition_credential_username";
        /// <summary>Parent: "environmentvariabledefinition" Child: "credential" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_credential_cyberarksafe = "environmentvariabledefinition_credential_cyberarksafe";
        /// <summary>Parent: "environmentvariabledefinition" Child: "credential" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_credential_cyberarkobject = "environmentvariabledefinition_credential_cyberarkobject";
        /// <summary>Parent: "environmentvariabledefinition" Child: "credential" Lookup: ""</summary>
        public const string Rel1M_environmentvariabledefinition_credential_cyberarkusername = "environmentvariabledefinition_credential_cyberarkusername";

        #endregion Relationships

        #region OptionSets

        public enum componentstate_OptionSet
        {
            Published = 0,
            Unpublished = 1,
            Deleted = 2,
            DeletedUnpublished = 3
        }
        public enum secretstore_OptionSet
        {
            AzureKeyVault = 0,
            MicrosoftDataverse = 1
        }
        public enum statecode_OptionSet
        {
            Active = 0,
            Inactive = 1
        }
        public enum statuscode_OptionSet
        {
            Active = 1,
            Inactive = 2
        }
        public enum type_OptionSet
        {
            String = 100000000,
            Number = 100000001,
            Boolean = 100000002,
            JSON = 100000003,
            DataSource = 100000004,
            Secret = 100000005
        }

        #endregion OptionSets
    }
}
