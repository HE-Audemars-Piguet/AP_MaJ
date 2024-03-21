using Autodesk.Connectivity.WebServices;
using Autodesk.DataManagement.Client.Framework.Currency;
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

        public Dictionary<string, Dictionary<string, IList<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping>>> VaultFilePropertyMapping
        {
            get 
            { 
                return _vaultFilePropertyMapping; 
            }
            set 
            { 
                _vaultFilePropertyMapping = value; 
            }
        }
        private Dictionary<string, Dictionary<string, IList<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping>>> _vaultFilePropertyMapping = null;

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

        public Dictionary<string, Dictionary<string, IList<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping>>> VaultItemPropertyMapping
        {
            get
            {
                return _vaultItemPropertyMapping;
            }
            set
            {
                _vaultItemPropertyMapping = value;
            }
        }
        private Dictionary<string, Dictionary<string, IList<VDF.Vault.Currency.Properties.ContentSourcePropertyMapping>>> _vaultItemPropertyMapping = null;

        public long ProviderPropId 
        {
            get
            {
                return _providerPropId;
            }
            set
            {
                _providerPropId = value;
            }
        }
        private long _providerPropId = -1;

        public long ItemAssignablePropId
        {
            get
            {
                return _itemAssignablePropId;
            }
            set
            {
                _itemAssignablePropId = value;
            }
        }
        private long _itemAssignablePropId = -1;

        public long CompliancePropId
        {
            get
            {
                return _compliancePropId;
            }
            set
            {
                _compliancePropId = value;
            }
        }
        private long _compliancePropId = -1;

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

        public Dictionary<FolderPathAbsolute, VDF.Vault.Currency.Entities.Folder> FolderPathToFolderDico
        {
            get
            {
                return _folderPathToFolderDico;
            }
            set
            {
                _folderPathToFolderDico = value;
            }
        }
        private Dictionary<FolderPathAbsolute, VDF.Vault.Currency.Entities.Folder> _folderPathToFolderDico = null;

        public List<NumSchm> VaultFileNumberingSchemes
        {
            get
            {
                return _vaultFileNumberingSchemes;
            }
            set
            {
                _vaultFileNumberingSchemes = value;
            }
        }
        private List<NumSchm> _vaultFileNumberingSchemes = null;

        public List<NumSchm> VaultItemNumberingSchemes
        {
            get
            {
                return _vaultItemNumberingSchemes;
            }
            set
            {
                _vaultItemNumberingSchemes = value;
            }
        }
        private List<NumSchm> _vaultItemNumberingSchemes = null;

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

        public List<string> InventorMaterials
        {
            get
            {
                return _inventorMaterials;
            }
            set
            {
                _inventorMaterials = value;
            }
        }
        private List<string> _inventorMaterials = null;
        public VaultConfig() { }
    }
}