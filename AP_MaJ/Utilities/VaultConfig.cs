using Autodesk.Connectivity.WebServices;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Entities;
using System.Collections.Generic;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace Ch.Hurni.AP_MaJ.Utilities
{

    public class VaultConfig
    {
        public VDF.Vault.Currency.Properties.PropertyDefinitionDictionary VaultFilePropertyDefinitionDictionary
        {
            get
            {
                return _vaultFilePropertyDefinitionDictionary;
            }
            set
            {
                _vaultFilePropertyDefinitionDictionary = value;
            }
        }
        private VDF.Vault.Currency.Properties.PropertyDefinitionDictionary _vaultFilePropertyDefinitionDictionary = null;

        public VDF.Vault.Currency.Properties.PropertyDefinitionDictionary VaultItemPropertyDefinitionDictionary
        {
            get
            {
                return _vaultItemPropertyDefinitionDictionary;
            }
            set
            {
                _vaultItemPropertyDefinitionDictionary = value;
            }
        }
        private VDF.Vault.Currency.Properties.PropertyDefinitionDictionary _vaultItemPropertyDefinitionDictionary = null;

        public List<LfCycDef> VaultLifeCycleDefinitionList
        {
            get
            {
                return _vaultLifeCycleDefinitionList;
            }
            set
            {
                _vaultLifeCycleDefinitionList = value;
            }
        }
        private List<LfCycDef> _vaultLifeCycleDefinitionList = null;

        public List<RevDef> VaultRevisionDefinitionList
        {
            get
            {
                return _vaultRevisionDefinitionList;
            }
            set
            {
                _vaultRevisionDefinitionList = value;
            }
        }
        private List<RevDef> _vaultRevisionDefinitionList = null;

        public List<EntityCategory> VaultFileCategoryList
        {
            get
            {
                return _vaultFileCategoryList;
            }
            set
            {
                _vaultFileCategoryList = value;
            }
        }
        private List<EntityCategory> _vaultFileCategoryList = null;
        
        public List<CatCfg> VaultFileCategoryBehavioursList
        {
            get
            {
                return _vaultFileCategoryBehavioursList;
            }
            set
            {
                _vaultFileCategoryBehavioursList = value;
            }
        }
        private List<CatCfg> _vaultFileCategoryBehavioursList = null;
        public List<CatCfg> VaultItemCategoryBehavioursList
        {
            get
            {
                return _vaultItemCategoryBehavioursList;
            }
            set
            {
                _vaultItemCategoryBehavioursList = value;
            }
        }
        private List<CatCfg> _vaultItemCategoryBehavioursList = null;

        public List<long> AllowedStateTransitionIdsList
        {
            get
            {
                return _allowedStateTransitionIdsList;
            }
            set
            {
                _allowedStateTransitionIdsList = value;
            }
        }
        private List<long> _allowedStateTransitionIdsList = null;

        public RevDefInfo RevDefInfo 
        {
            get
            {
                return _revDefInfo;
            }
            set
            {
                _revDefInfo = value;
            }
        }
        private RevDefInfo _revDefInfo = null;
        public VaultConfig() { }
    }
}