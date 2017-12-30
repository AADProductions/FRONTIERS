using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Frontiers.Main
{
    public class Item : MonoBehaviour, IPopupListTarget, IFocusItem, IExaminable, IDamageSource
    {
        [Serializable]
        [XmlType("ItemState")]
        public class State : Mod
        {
            public State() { }

            public State(Item item) {
                if (item == null)
                    throw new NullReferenceException();

                Name = item.name;
                Mode = item.Mode;
                DropPosition = item.transform.position;
                PropsFileName = item.Props.FileName;
                SaveStateKey = item.Key;
                SubItemCount = item.subItemCount;
                SubItemName = item.subItemName;
                Origin = item.origin;
                TimeLastDropped = item.timeLastDropped;
                TimeFirstPickedUp = item.timeFirstPickedUp;
                TimeLastPickedUp = item.timeLastPickedUp;
                ActiveState = item.activeState;
            }

            // Mode of item's existence
            public ItemMode Mode;
            // Drop position in the world
            public SVector3 DropPosition;
            // Used to load the correct props
            public string PropsFileName;
            // Used primarily by projectiles
            public int SubItemCount;
            // Used for crafting
            public ItemOrigin Origin;
            // Used by multi-state objects
            public string ActiveState;
            // Used by single objects that need unique IDs
            public string SubItemName;
            // Time variables
            public float TimeFirstPickedUp;
            public float TimeLastDropped;
            public float TimeLastPickedUp;
        }

        public Action<ItemMode> OnModeChange;
        public Action<Item> OnItemChange;

        public void Initialize(ItemProps newProps, State state) {

            Debug.LogWarning("Initializing " + newProps.FileName + "...");

            initializing = true;

            props = newProps;

            if (state != null) {
                name = state.Name;
                mode = state.Mode;
                transform.position = state.DropPosition;
                key = state.SaveStateKey;
                subItemCount = state.SubItemCount;
                origin = state.Origin;
                activeState = state.ActiveState;
                subItemName = state.SubItemName;
                timeFirstPickedUp = state.TimeFirstPickedUp;
                timeLastDropped = state.TimeLastDropped;
                timeLastPickedUp = state.TimeLastPickedUp;
            }

            //name = props.FileName;
            sprite.sprite = props.BaseSprite;

            // Check our subtypes and see if they require scripts
            ItemProps.SubType[] subTypes = props.SubTypes;
            for (int i = 0; i < subTypes.Length; i++) {
                if (subTypes[i].UsesScript) {
                    Component subTypeScript = gameObject.GetComponent(subTypes[i].ScriptType);
                    if (subTypeScript == null) {
                        //Debug.Log("Adding subtype script " + subTypes[i].ScriptType.Name);
                        subTypeScript = gameObject.AddComponent(subTypes[i].ScriptType);
                    }

                    if (subTypeScript == null) {
                        Debug.LogError("Couldn't add script type " + subTypes[i].ScriptType.Name + " to item " + props.FileName);
                    } else {
                        System.Reflection.MethodInfo method = subTypeScript.GetType().GetMethod("Initialize");
                        if (method != null) {
                            Debug.Log("initializing subtype " + subTypeScript.GetType().Name);
                            method.Invoke(subTypeScript, new object[] { subTypes[i], this });
                        } else {
                            Debug.LogError("Couldn't get initialize method from subtype script " + subTypes[i].ScriptType.Name);
                        }
                    }
                }/* else {
                    Debug.Log("Subtype " + subTypes[i].name + " doesn't use a script");
                }*/
            }

            SetItemMode(mode);

            initializing = false;
        }

        public string Key {
            get {
                return key;
            }
            set {
                if (!string.IsNullOrEmpty(key))
                    return;

                key = value;
            }
        }

        public ItemProps Props {
            get {
                return props;
            }
        }

        #region state values
        public string ActiveState {
            get {
                return activeState;
            }
            set {
                if (activeState != value) {
                    activeState = value;
                    StoreItem();
                    if (OnItemChange != null)
                        OnItemChange(this);
                }
            }
        }

        public string SubItemName {
            get {
                return subItemName;
            }
            set {
                if (subItemName != value) {
                    subItemName = value;
                    StoreItem();
                    if (OnItemChange != null)
                        OnItemChange(this);
                }
            }
        }

        public float FirstPickUpTime {
            get {
                return timeFirstPickedUp;
            }
            set {
                // Only set this once
                if (timeFirstPickedUp != 0) {
                    return;
                }
                timeFirstPickedUp = value;
                StoreItem();
            }
        }

        public int SubItemCount {
            get {
                return subItemCount;
            }
            set {
                if (subItemCount != value) {
                    subItemCount = value;
                    StoreItem();
                    if (OnItemChange != null)
                        OnItemChange(this);
                }
            }
        }
        #endregion

        public bool CanEnterInventory {
            get {
                return
                    props.Weight != ItemWeight.Unliftable &&
                    props.Size != WISize.NoLimit &&
                    Mode != ItemMode.Destroyed;
            }
        }

        private void Awake() {
            GameManager.OnGameLoad += OnGameLoad;
        }

        private void OnDestroy() {
            GameManager.OnGameLoad -= OnGameLoad;
            OnItemChange = null;
            OnModeChange = null;
        }

        private void OnEnable() {
            if (props != null && GameManager.Get.State == ProgramStateEnum.InGame) {
                // If the game is already loaded, load automatically
                OnGameLoad();
            }
        }

        private void OnGameLoad() {
            //Debug.LogError("On game load in " + name + " - " + props.name);
            // Get a key if we don't have one yet
            if (string.IsNullOrEmpty(key)) {
                key = GameState.GetKeyFromUniqueString(name);
            }

            // Register ourselves with Items so other items can look us up
            Items.Get.Register(this);

            if (!GameState.Get.LoadItem(Key, this)) {
                // If we don't load ourselves, initialize manually without a state
                Debug.LogWarning("No load state found, initializing ourselves in " + props.FileName);
                Initialize(props, null);
            }
        }

        #region damage source
        public WIMaterialType Material { get { return props.Material; } }
        #endregion

        #region popup list
        public void PopulateOptionsList(List<PopupListOption> options, List<string> message) {
            if (Mode != ItemMode.ExistsInInventory && CanEnterInventory) {
                options.Add(new PopupListOption(OnSelectPopupListOption, "Take"));
            }
            options.Add(new PopupListOption(OnSelectPopupListOption, "Examine"));
        }

        public void OnSelectPopupListOption(string result, int subResultIndex) {
            switch (result) {
                case "Take":
                    LocalPlayer.Get.Inventory.TryToAddItem(this);
                    break;

                case "Examine":
                    ExamineDisplay.Get.ExamineItem(gameObject);
                    break;
            }
        }

        public bool OpenListOnEquippedUse { get { return false; } }
        #endregion

        #region focus
        public ItemMode Mode {
            get {
                return mode;
            }
            set {
                if (mode != value) {
                    SetItemMode(value);
                    StoreItem();
                }
            }
        }

        public Vector3 Position {
            get {
                return transform.position;
            }
        }

        public FocusItemType ItemType {
            get {
                return FocusItemType.Item;
            }
        }

        public bool HasPlayerFocus {
            get {
                return hasPlayerFocus;
            }
            set {
                hasPlayerFocus = value;
                sprite.sharedMaterial = hasPlayerFocus ? highlightMat : standardMat;
                outline.enabled = hasPlayerFocus;
            }
        }

        public bool HasPlayerAttention {
            get { return hasPlayerAttention; }
            set { hasPlayerAttention = value; }
        }
        #endregion

        #region examine
        public string DisplayName {
            get {
                return string.IsNullOrEmpty (overrideDisplayName) ? props.DisplayName : overrideDisplayName;
            }
            set {
                overrideDisplayName = value;
            }
        }

        public Sprite ItemSprite {
            get {
                return overrideSprite == null ? props.BaseSprite : overrideSprite;
            }
            set {
                overrideSprite = value;
                sprite.sprite = overrideSprite == null ? props.BaseSprite : overrideSprite;
            }
        }

        public Material ItemMaterial {
            get {
                return standardMat;
            }
            set {
                standardMat = value;
                if (!hasPlayerFocus) {
                    sprite.sharedMaterial = standardMat;
                }
            }
        }

        public int ExamineOrder { get { return 0; } }

        public void Examine(List<ExamineInfo> examineInfo) {
            examineInfo.Add(new ExamineInfo(props.StaticDescription));
        }
        #endregion

        public float TimeLastDropped {
            get {
                return timeLastDropped;
            }
        }

        public float TimeLastPickedUp {
            get {
                return timeLastPickedUp;
            }
        }

        private void SetItemMode(ItemMode newMode) {

            if (newMode == ItemMode.Destroyed) {
                Debug.Log("Setting " + props.FileName + " item mode to " + newMode);
            }

            mode = newMode;
            switch (mode) {
                case ItemMode.Destroyed:
                    gameObject.SetActive(false);
                    break;

                case ItemMode.ExistsInInventory:
                    gameObject.SetActive(false);
                    break;

                case ItemMode.ExistsInWorldPerm:
                    break;

                case ItemMode.ExistsInWorldTemp:
                    bob.enabled = true;
                    break;

                case ItemMode.HiddenInWorld:
                    gameObject.SetActive(false);
                    break;
            }

            if (OnModeChange != null)
                OnModeChange(mode);

            if (OnItemChange != null)
                OnItemChange(this);
        }

        private void StoreItem() {
            // We shouldn't store changes while initializing
            if (initializing)
                return;

            // We shouldn't store changes made while loading or saving
            if (GameManager.Get.State == ProgramStateEnum.GameLoading || GameManager.Get.State == ProgramStateEnum.GameSaving)
                return;

            if (props != null && Application.isPlaying) {
                Debug.Log("Storing item " + name + " with mode " + mode);
                GameState.Get.StoreItem(Key, this);
            }
        }

        #region state info

        [SerializeField]
        private string key;
        [SerializeField]
        private string activeState;
        [SerializeField]
        private string subItemName;
        [SerializeField]
        private int subItemCount;
        [SerializeField]
        private ItemMode mode = ItemMode.ExistsInWorldPerm;
        [SerializeField]
        private ItemOrigin origin = ItemOrigin.Default;
        [SerializeField]
        private float timeFirstPickedUp;
        [SerializeField]
        private float timeLastDropped;
        [SerializeField]
        private float timeLastPickedUp;

        #endregion

        [SerializeField]
        private bool hasPlayerAttention = false;
        [SerializeField]
        private bool hasPlayerFocus = false;
        [SerializeField]
        private ItemProps props;
        [SerializeField]
        private Container container;
        [SerializeField]
        private SpriteRenderer sprite;
        [SerializeField]
        private ItemBillboard billboard;
        [SerializeField]
        private SpriteOutline outline;
        [SerializeField]
        private ItemBob bob;
        [SerializeField]
        private Material standardMat;
        [SerializeField]
        private Material highlightMat;
        
        private Sprite overrideSprite;
        private string overrideDisplayName;
        private bool initializing;

        #if UNITY_EDITOR
        void OnDrawGizmos() {
            if (Application.isPlaying)
                return;

            if (props != null && sprite != null) {
                sprite.sprite = props.BaseSprite;
            }
            if (string.IsNullOrEmpty (key)) {
                key = GameState.GetKeyFromUniqueString(name);
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
        }
        #endif
    }
}
