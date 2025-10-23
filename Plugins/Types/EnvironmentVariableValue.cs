// *********************************************************************
// Created by : Latebound Constants Generator 1.2023.12.1 for XrmToolBox
// Author     : Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://mikefactorialdemo.crm.dynamics.com
// Filename   : C:\source\repos\NextGenPluginDemo\Types\EnvironmentVariableValue.cs
// Created    : 2024-07-03 17:09:03
// *********************************************************************

namespace NextGenDemo.Plugins.Types
{
    /// <summary>DisplayName: Environment Variable Value, OwnershipType: UserOwned, IntroducedVersion: 1.0.0.0</summary>
    public static class environmentvariablevalue
    {
        public const string EntityName = "environmentvariablevalue";
        public const string EntityCollectionName = "environmentvariablevalues";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "environmentvariablevalueid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "schemaname";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: Component State, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string componentstate = "componentstate";
        /// <summary>Type: ManagedProperty, RequiredLevel: SystemRequired</summary>
        public const string iscustomizable = "iscustomizable";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: environmentvariabledefinition</summary>
        public const string environmentvariabledefinitionid = "environmentvariabledefinitionid";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string environmentvariablevalueidunique = "environmentvariablevalueidunique";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 48, Format: VersionNumber</summary>
        public const string introducedversion = "introducedversion";
        /// <summary>Type: Boolean, RequiredLevel: SystemRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string ismanaged = "ismanaged";
        /// <summary>Type: DateTime, RequiredLevel: SystemRequired, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string overwritetime = "overwritetime";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string solutionid = "solutionid";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string supportingsolutionid = "supportingsolutionid";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string statecode = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string statuscode = "statuscode";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string value = "value";

        #endregion Attributes

        #region Relationships

        /// <summary>Parent: "environmentvariabledefinition" Child: "environmentvariablevalue" Lookup: "environmentvariabledefinitionid"</summary>
        public const string RelM1_environmentvariabledefinition_environmentvariablevalue = "environmentvariabledefinition_environmentvariablevalue";
        /// <summary>Parent: "environmentvariablevalue" Child: "principalobjectattributeaccess" Lookup: ""</summary>
        public const string Rel1M_environmentvariablevalue_PrincipalObjectAttributeAccesses = "environmentvariablevalue_PrincipalObjectAttributeAccesses";

        #endregion Relationships

        #region OptionSets

        public enum componentstate_OptionSet
        {
            Published = 0,
            Unpublished = 1,
            Deleted = 2,
            DeletedUnpublished = 3
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

        #endregion OptionSets
    }
}
