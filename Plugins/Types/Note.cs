// *********************************************************************
// Created by : Latebound Constants Generator 1.2024.11.4 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://org1ad17421.crm.dynamics.com/
// Filename   : C:\source\repos\NextGenPluginDemo\Types\Note.cs
// Created    : 2024-12-24 07:22:41
// *********************************************************************

namespace NextGenDemo.Plugins.Types
{
    /// <summary>DisplayName: Note, OwnershipType: UserOwned, IntroducedVersion: 5.0.0.0</summary>
    public static class annotation
    {
        public const string EntityName = "annotation";
        public const string EntityCollectionName = "annotations";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "annotationid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 500, Format: Text</summary>
        public const string PrimaryName = "subject";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 100000</summary>
        public const string notetext = "notetext";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1073741823, Format: TextArea</summary>
        public const string documentbody = "documentbody";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 255, Format: Text</summary>
        public const string filename = "filename";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 500, Format: Text</summary>
        public const string dummyfilename = "dummyfilename";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 255, Format: Text</summary>
        public const string filepointer = "filepointer";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 1000000000</summary>
        public const string filesize = "filesize";
        /// <summary>Type: Boolean, RequiredLevel: SystemRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string isdocument = "isdocument";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string isprivate = "isprivate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 2, Format: Text</summary>
        public const string langid = "langid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 256, Format: Text</summary>
        public const string mimetype = "mimetype";
        /// <summary>Type: EntityName, RequiredLevel: None, DisplayName: Object Type , OptionSetType: Picklist</summary>
        public const string objecttypecode = "objecttypecode";
        /// <summary>Type: Owner, RequiredLevel: SystemRequired, Targets: systemuser,team</summary>
        public const string ownerid = "ownerid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 10, Format: Text</summary>
        public const string prefix = "prefix";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: account,adx_invitation,adx_inviteredemption,adx_portalcomment,appointment,calendar,channelaccessprofile,channelaccessprofilerule,channelaccessprofileruleitem,chat,contact,convertrule,duplicaterule,email,emailserverprofile,fax,goal,kbarticle,knowledgearticle,knowledgebaserecord,letter,mailbox,msdyn_aifptrainingdocument,msdyn_aimodel,msdyn_aiodimage,msdyn_flow_approval,msfp_alert,msfp_question,msfp_surveyinvite,msfp_surveyresponse,mspcat_catalogsubmissionfiles,phonecall,recurringappointmentmaster,routingrule,routingruleitem,sharepointdocument,sla,socialactivity,task,workflow</summary>
        public const string objectid = "objectid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 500, Format: Text</summary>
        public const string dummyregarding = "dummyregarding";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 32, Format: Text</summary>
        public const string stepid = "stepid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 10, Format: Text</summary>
        public const string storagepointer = "storagepointer";

        #endregion Attributes

        #region Relationships
        #endregion Relationships

        #region OptionSets

        public enum objecttypecode_OptionSet
        {
            Account = 1,
            Appointment = 4201,
            BulkImport = 4407,
            Calendar = 4003,
            Campaign = 4400,
            CampaignActivity = 4402,
            CampaignResponse = 4401,
            Case = 112,
            CaseResolution = 4206,
            Commitment = 4215,
            Competitor = 123,
            Contact = 2,
            Contract = 1010,
            ContractLine = 1011,
            Email = 4202,
            Facility_Equipment = 4000,
            Fax = 4204,
            Invoice = 1090,
            Lead = 4,
            Letter = 4207,
            MarketingList = 4300,
            Opportunity = 3,
            OpportunityClose = 4208,
            Order = 1088,
            OrderClose = 4209,
            PhoneCall = 4210,
            Product = 1024,
            Quote = 1084,
            QuoteClose = 4211,
            ResourceSpecification = 4006,
            Service = 4001,
            ServiceActivity = 4214,
            Task = 4212,
            RoutingRule = 8181,
            RoutingRuleItem = 8199
        }

        #endregion OptionSets
    }
}
