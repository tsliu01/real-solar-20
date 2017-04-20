namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class rdtSerializerRegistry
    {
        private Dictionary<System.Type, ConvertObjectDelegate> m_converters = new Dictionary<System.Type, ConvertObjectDelegate>();
        private HashSet<System.Type> m_dontReadProperties = new HashSet<System.Type>();
        private HashSet<System.Type> m_failures = new HashSet<System.Type>();
        private Dictionary<string, HashSet<string>> m_includePropertiesPerType = new Dictionary<string, HashSet<string>>();
        private HashSet<System.Type> m_referenceFailures = new HashSet<System.Type>();
        private HashSet<string> m_skipProperties = new HashSet<string>();
        private Dictionary<string, HashSet<string>> m_skipPropertiesPerType = new Dictionary<string, HashSet<string>>();
        private HashSet<System.Type> m_skipTypes = new HashSet<System.Type>();

        public rdtSerializerRegistry()
        {
            this.m_converters.Add(typeof(Vector2), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerVector2((Vector2) objIn);
            });
            this.m_converters.Add(typeof(Vector3), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerVector3((Vector3) objIn);
            });
            this.m_converters.Add(typeof(Vector4), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerVector4((Vector4) objIn);
            });
            this.m_converters.Add(typeof(Quaternion), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerQuaternion((Quaternion) objIn);
            });
            this.m_converters.Add(typeof(Color), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerColor((Color) objIn);
            });
            this.m_converters.Add(typeof(Color32), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerColor32((Color32) objIn);
            });
            this.m_converters.Add(typeof(Rect), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerRect((Rect) objIn);
            });
            this.m_converters.Add(typeof(Bounds), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerBounds((Bounds) objIn);
            });
            this.m_converters.Add(typeof(Matrix4x4), delegate (object objIn, rdtSerializerRegistry r) {
                return new rdtSerializerMatrix4x4((Matrix4x4) objIn);
            });
            this.m_converters.Add(typeof(List<>), new ConvertObjectDelegate(rdtSerializerContainerList.Serialize));
            this.m_converters.Add(typeof(Dictionary<,>), new ConvertObjectDelegate(this.NotHandledConversion));
            this.m_converters.Add(typeof(Array), new ConvertObjectDelegate(rdtSerializerContainerArray.Serialize));
            this.InitSkipProperties();
        }

        private void AddDontReadProperties(System.Type type)
        {
            this.m_dontReadProperties.Add(type);
        }

        private void AddField(List<rdtTcpMessageComponents.Property> allFields, string name, object value, rdtTcpMessageComponents.Property.Type type, RangeAttribute rangeAttribute)
        {
            object obj2;
            if ((rangeAttribute != null) && (value is float))
            {
                obj2 = new rdtSerializerSlider((float) value, rangeAttribute.min, rangeAttribute.max);
            }
            else
            {
                obj2 = this.Serialize(value);
                if ((value != null) && (obj2 == null))
                {
                    return;
                }
            }
            rdtTcpMessageComponents.Property item = new rdtTcpMessageComponents.Property();
            item.m_name = name;
            item.m_value = obj2;
            item.m_type = type;
            allFields.Add(item);
        }

        private void AddIncludeForType(string typeName, params string[] properties)
        {
            HashSet<string> set;
            if (!this.m_includePropertiesPerType.TryGetValue(typeName, out set))
            {
                set = new HashSet<string>();
                this.m_includePropertiesPerType.Add(typeName, set);
            }
            set.UnionWith(properties);
        }

        private void AddIncludeForType(System.Type type, params string[] properties)
        {
            this.AddIncludeForType(type.Name, properties);
        }

        private void AddSkipForType(string typeName, params string[] properties)
        {
            HashSet<string> set;
            if (!this.m_skipPropertiesPerType.TryGetValue(typeName, out set))
            {
                set = new HashSet<string>();
                this.m_skipPropertiesPerType.Add(typeName, set);
            }
            set.UnionWith(properties);
        }

        private void AddSkipForType(System.Type type, params string[] properties)
        {
            this.AddSkipForType(type.Name, properties);
        }

        private bool CanAddMember(object owner, MemberInfo memberInfo, System.Type memberType)
        {
            bool flag = memberInfo.IsDefined(typeof(ObsoleteAttribute), false);
            bool flag2 = memberInfo.IsDefined(typeof(HideInInspector), false);
            if (!flag && !flag2)
            {
                if (!rdtTypeExtensions.IsReference(memberType))
                {
                    return true;
                }
                if (!this.m_referenceFailures.Contains(memberType))
                {
                    UnityEngine.Object obj2 = owner as UnityEngine.Object;
                    rdtDebug.Log(rdtDebug.LogLevel.Warning, "Remote Debug: Member '{0}' (type {1}) on '{2}', component {3} is a reference and is not serializable!", new object[] { memberInfo.Name, memberType.Name, (obj2 != null) ? obj2.name : "<unknown>", (obj2 != null) ? obj2.GetType().Name : "<unknown>" });
                    this.m_referenceFailures.Add(memberType);
                }
            }
            return false;
        }

        public object Deserialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            object obj2 = obj;
            rdtSerializerInterface interface2 = obj as rdtSerializerInterface;
            if (interface2 != null)
            {
                obj2 = interface2.Deserialize(this);
            }
            return obj2;
        }

        private bool HasIncludePerType(string ownerTypeName)
        {
            return this.m_includePropertiesPerType.ContainsKey(ownerTypeName);
        }

        private bool IncludeMember(string ownerTypeName, string memberInfoName)
        {
            HashSet<string> set = null;
            return (!this.m_includePropertiesPerType.TryGetValue(ownerTypeName, out set) || set.Contains(memberInfoName));
        }

        private void InitSkipProperties()
        {
            this.m_skipTypes.Add(typeof(ParticleSystemRenderer));
            this.m_skipProperties.Add("hideFlags");
            this.m_skipProperties.Add("useGUILayout");
            this.m_skipProperties.Add("tag");
            this.m_skipProperties.Add("name");
            this.m_skipProperties.Add("enabled");
            this.AddSkipForType(typeof(Rigidbody2D), new string[] { "position", "rotation", "freezeRotation" });
            string[] properties = new string[] { "position", "rotation", "freezeRotation", "useConeFriction" };
            this.AddSkipForType(typeof(Rigidbody), properties);
            string[] strArray2 = new string[] { "material", "sharedMaterial", "density", "sharedMesh" };
            this.AddSkipForType(typeof(BoxCollider), strArray2);
            this.AddSkipForType(typeof(BoxCollider2D), strArray2);
            this.AddSkipForType(typeof(CircleCollider2D), strArray2);
            this.AddSkipForType(typeof(SphereCollider), strArray2);
            this.AddSkipForType(typeof(PolygonCollider2D), strArray2);
            this.AddSkipForType(typeof(MeshCollider), strArray2);
            this.AddSkipForType(typeof(CapsuleCollider), strArray2);
            this.AddSkipForType(typeof(EdgeCollider2D), strArray2);
            this.AddSkipForType(typeof(WheelCollider), strArray2);
            this.AddSkipForType(typeof(TerrainCollider), strArray2);
            this.AddSkipForType(typeof(TerrainCollider), new string[] { "isTrigger", "terrainData" });
            this.AddSkipForType(typeof(CharacterController), strArray2);
            this.AddSkipForType(typeof(CharacterController), new string[] { "isTrigger", "contactOffset" });
            string[] strArray3 = new string[] { "capsuleColliders", "sphereColliders", "solverFrequency", "useContinuousCollision", "useVirtualParticles" };
            this.AddSkipForType(typeof(Cloth), strArray3);
            string[] strArray4 = new string[] { "breakForce", "breakTorque", "connectedBody" };
            this.AddSkipForType(typeof(HingeJoint2D), strArray4);
            this.AddSkipForType("FixedJoint2D", strArray4);
            this.AddSkipForType(typeof(SpringJoint2D), strArray4);
            this.AddSkipForType(typeof(DistanceJoint2D), strArray4);
            this.AddSkipForType("FrictionJoint2D", strArray4);
            this.AddSkipForType("RelativeJoint2D", strArray4);
            this.AddSkipForType(typeof(SliderJoint2D), strArray4);
            this.AddSkipForType(typeof(WheelJoint2D), strArray4);
            this.AddSkipForType("TargetJoint2D", new string[] { "enableCollision" });
            this.AddSkipForType("TargetJoint2D", strArray4);
            string[] strArray5 = new string[] { "connectedBody" };
            this.AddSkipForType(typeof(CharacterJoint), strArray5);
            this.AddSkipForType(typeof(ConfigurableJoint), strArray5);
            this.AddSkipForType(typeof(FixedJoint), strArray5);
            this.AddSkipForType(typeof(HingeJoint), strArray5);
            this.AddSkipForType(typeof(SpringJoint), strArray5);
            this.AddSkipForType("ReflectionProbe", new string[] { "bakedTexture", "customBakedTexture" });
            this.AddSkipForType(typeof(Skybox), new string[] { "material" });
            this.AddSkipForType(typeof(UnityEngine.AI.NavMeshAgent), new string[] { "velocity", "nextPosition" });
            this.AddSkipForType(typeof(AudioSource), new string[] { "clip", "outputAudioMixerGroup" });
            this.AddSkipForType(typeof(AudioLowPassFilter), new string[] { "customCutoffCurve" });
            this.AddSkipForType(typeof(AudioReverbZone), new string[] { "reverbDelay", "reflectionsDelay" });
            this.AddSkipForType(typeof(LensFlare), new string[] { "flare" });
            this.AddSkipForType(typeof(Projector), new string[] { "material" });
            this.AddSkipForType(typeof(EventSystem), new string[] { "m_FirstSelected", "firstSelectedGameObject" });
            this.AddSkipForType(typeof(EventTrigger), new string[] { "m_Delegates" });
            this.AddSkipForType(typeof(Canvas), new string[] { "worldCamera" });
            this.AddSkipForType("TouchInputModule", new string[] { "forceModuleActive" });
            this.AddSkipForType(typeof(Light), new string[] { "flare", "cookie" });
            string[] strArray6 = new string[] { "m_Padding", "padding" };
            this.AddSkipForType(typeof(GridLayoutGroup), strArray6);
            this.AddSkipForType(typeof(HorizontalLayoutGroup), strArray6);
            this.AddSkipForType(typeof(VerticalLayoutGroup), strArray6);
            this.AddSkipForType(typeof(TextMesh), new string[] { "font" });
            this.AddSkipForType(typeof(Animation), new string[] { "clip" });
            this.AddSkipForType(typeof(Animator), new string[] { "runtimeAnimatorController", "avatar", "bodyPosition", "bodyRotation", "playbackTime" });
            this.AddSkipForType(typeof(NetworkView), new string[] { "observed", "viewID" });
            this.AddSkipForType(typeof(Terrain), new string[] { "terrainData", "materialTemplate" });
            this.AddSkipForType(typeof(UnityEngine.AI.NavMeshAgent), new string[] { "path" });
            this.AddSkipForType(typeof(UnityEngine.AI.OffMeshLink), new string[] { "startTransform", "endTransform" });
            string[] strArray7 = new string[] { "m_SpawnPrefabs", "m_ConnectionConfig", "m_GlobalConfig", "m_Channels", "m_PlayerPrefab", "client", "matchInfo", "matchMaker", "matches" };
            this.AddSkipForType("NetworkManager", strArray7);
            this.AddSkipForType("NetworkLobbyManager", strArray7);
            this.AddSkipForType("NetworkLobbyManager", new string[] { "m_LobbyPlayerPrefab", "m_GamePlayerPrefab" });
            this.AddSkipForType("NetworkTransform", new string[] { "m_ClientMoveCallback3D", "m_ClientMoveCallback2D" });
            this.AddSkipForType("NetworkTransformVisualizer", new string[] { "m_VisualizerPrefab" });
            this.AddSkipForType(typeof(GUIText), new string[] { "material", "font" });
            this.AddSkipForType(typeof(GUITexture), new string[] { "texture", "border" });
            string[] strArray8 = new string[] { 
                "m_OnClick", "m_TargetGraphic", "m_AnimationTriggers", "m_SpriteState", "m_OnCullStateChanged", "m_Template", "m_CaptionText", "m_CaptionImage", "m_Options", "m_OnValueChanged", "m_ItemText", "m_ItemImage", "m_Sprite", "m_Material", "m_TextComponent", "m_Placeholder",
                "m_OnEndEdit", "m_OnValidateInput", "m_Texture", "m_HandleRect", "m_FontData", "m_Group", "m_AsteriskChar", "m_FillRect", "onValueChanged", "graphic", "m_HorizontalScrollbar", "m_Content", "m_VerticalScrollbar", "m_Viewport"
            };
            this.AddSkipForType(typeof(Navigation), new string[] { "m_SelectOnUp", "m_SelectOnDown", "m_SelectOnLeft", "m_SelectOnRight" });
            this.AddSkipForType(typeof(Button), strArray8);
            this.AddSkipForType("Dropdown", strArray8);
            this.AddSkipForType(typeof(Image), strArray8);
            this.AddSkipForType(typeof(InputField), strArray8);
            this.AddSkipForType(typeof(RawImage), strArray8);
            this.AddSkipForType(typeof(Scrollbar), strArray8);
            this.AddSkipForType(typeof(ScrollRect), strArray8);
            this.AddSkipForType(typeof(Selectable), strArray8);
            this.AddSkipForType(typeof(Slider), strArray8);
            this.AddSkipForType(typeof(Text), strArray8);
            this.AddSkipForType(typeof(Toggle), strArray8);
            this.AddDontReadProperties(typeof(ColorBlock));
            this.AddDontReadProperties(typeof(Navigation));
            string[] strArray9 = new string[] { "localPosition", "localEulerAngles", "localScale" };
            this.AddIncludeForType(typeof(Transform), strArray9);
            string[] strArray10 = new string[] { "anchoredPosition", "anchorMax", "anchorMin", "offsetMax", "offsetMin", "pivot" };
            this.AddIncludeForType(typeof(RectTransform), strArray9);
            this.AddIncludeForType(typeof(RectTransform), strArray10);
            string[] strArray11 = new string[] { "shadowCastingMode", "receiveShadows", "useLightProbes", "reflectionProbeUsage" };
            this.AddIncludeForType(typeof(MeshRenderer), strArray11);
            this.AddIncludeForType(typeof(SpriteRenderer), strArray11);
            this.AddIncludeForType(typeof(SpriteRenderer), new string[] { "color", "flipX", "flipY" });
            string[] strArray12 = new string[] { "alignment", "cameraVelocityScale", "lengthScale", "maxParticleSize", "minParticleSize", "normalDirection", "pivot", "renderMode", "sortingFudge", "sortMode", "velocityScale" };
            this.AddIncludeForType(typeof(ParticleSystemRenderer), strArray11);
            this.AddIncludeForType(typeof(ParticleSystemRenderer), strArray12);
            this.AddIncludeForType(typeof(TrailRenderer), strArray11);
            this.AddIncludeForType(typeof(TrailRenderer), new string[] { "autodestruct", "endWidth", "startWidth", "time" });
            this.AddIncludeForType(typeof(SkinnedMeshRenderer), strArray11);
            this.AddIncludeForType(typeof(SkinnedMeshRenderer), new string[] { "quality", "updateWhenOffscreen", "localBounds" });
            this.AddIncludeForType(typeof(LineRenderer), strArray11);
            this.AddIncludeForType(typeof(LineRenderer), new string[] { "useWorldSpace" });
            this.AddIncludeForType("BillboardRenderer", strArray11);
            string[] strArray13 = new string[] { "clearFlags", "backgroundColor", "cullingMask", "orthographic", "orthographicSize", "fov", "nearClipPlane", "farClipPlane", "rect", "depth", "renderingPath", "useOcclusionCulling", "hdr", "targetDisplay" };
            this.AddIncludeForType(typeof(Camera), strArray13);
            this.AddIncludeForType(typeof(MeshFilter), new string[0]);
            this.AddIncludeForType("NetworkAnimator", new string[0]);
            this.AddIncludeForType("NetworkIdentity", new string[] { "m_ServerOnly", "m_LocalPlayerAuthority" });
            this.AddIncludeForType(typeof(CanvasRenderer), new string[0]);
        }

        private object NotHandledConversion(object objIn, rdtSerializerRegistry r)
        {
            return null;
        }

        public List<rdtTcpMessageComponents.Property> ReadAllFields(object owner)
        {
            System.Type item = owner.GetType();
            if (this.m_skipTypes.Contains(item))
            {
                return null;
            }
            List<rdtTcpMessageComponents.Property> allFields = new List<rdtTcpMessageComponents.Property>();
            PropertyInfo[] properties = item.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            bool flag = owner is MonoBehaviour;
            string name = item.Name;
            if (!flag && !this.m_dontReadProperties.Contains(item))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo memberInfo = properties[i];
                    if ((memberInfo.CanRead && memberInfo.CanWrite) && (memberInfo.GetIndexParameters().Length <= 0))
                    {
                        string memberInfoName = memberInfo.Name;
                        bool flag2 = !this.HasIncludePerType(name);
                        if ((flag2 || this.IncludeMember(name, memberInfoName)) && (!flag2 || !this.SkipMember(name, memberInfoName)))
                        {
                            MethodInfo getMethod = memberInfo.GetGetMethod();
                            if ((getMethod != null) && getMethod.IsPublic)
                            {
                                MethodInfo setMethod = memberInfo.GetSetMethod();
                                if (((setMethod != null) && setMethod.IsPublic) && this.CanAddMember(owner, memberInfo, memberInfo.PropertyType))
                                {
                                    RangeAttribute rangeAttribute = null;
                                    object obj2 = memberInfo.GetValue(owner, null);
                                    this.AddField(allFields, memberInfoName, obj2, rdtTcpMessageComponents.Property.Type.Property, rangeAttribute);
                                }
                            }
                        }
                    }
                }
            }
            foreach (FieldInfo info4 in rdtTypeExtensions.GetAllFields(item).ToArray())
            {
                bool flag3 = info4.IsDefined(typeof(SerializeField), false);
                RangeAttribute attribute2 = null;
                if (info4.IsPublic || flag3)
                {
                    string str3 = info4.Name;
                    bool flag4 = !this.HasIncludePerType(name);
                    if (((flag4 || this.IncludeMember(name, str3)) && (!flag4 || !this.SkipMember(name, str3))) && this.CanAddMember(owner, info4, info4.FieldType))
                    {
                        object obj3 = info4.GetValue(owner);
                        this.AddField(allFields, str3, obj3, rdtTcpMessageComponents.Property.Type.Field, attribute2);
                    }
                }
            }
            foreach (MethodInfo info5 in item.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (info5.IsDefined(typeof(ButtonAttribute), false))
                {
                    rdtTcpMessageComponents.Property property = new rdtTcpMessageComponents.Property();
                    property.m_name = info5.Name;
                    property.m_value = new rdtSerializerButton(false);
                    property.m_type = rdtTcpMessageComponents.Property.Type.Method;
                    allFields.Add(property);
                }
            }
            return allFields;
        }

        public object Serialize(object obj)
        {
            if (obj != null)
            {
                ConvertObjectDelegate delegate2;
                object obj2 = obj;
                System.Type key = obj.GetType();
                if (key.IsArray)
                {
                    key = typeof(Array);
                }
                else if (key.IsGenericType)
                {
                    key = key.GetGenericTypeDefinition();
                }
                if (this.m_converters.TryGetValue(key, out delegate2))
                {
                    obj2 = delegate2(obj, this);
                }
                else if (rdtTypeExtensions.IsUserStruct(key))
                {
                    obj2 = this.ReadAllFields(obj);
                }
                if ((obj2 == null) || obj2.GetType().IsSerializable)
                {
                    return obj2;
                }
                System.Type type = obj2.GetType();
                if (!this.m_failures.Contains(type))
                {
                    rdtDebug.Warning("Remote Debug: Object '{0}' (type {1}) is not serializable!", new object[] { obj2, obj2.GetType().Name });
                    this.m_failures.Add(type);
                }
            }
            return null;
        }

        private bool SkipMember(string ownerTypeName, string memberInfoName)
        {
            if (this.m_skipProperties.Contains(memberInfoName))
            {
                return true;
            }
            HashSet<string> set = null;
            return (this.m_skipPropertiesPerType.TryGetValue(ownerTypeName, out set) && set.Contains(memberInfoName));
        }

        public void WriteAllFields(object owner, List<rdtTcpMessageComponents.Property> allFields)
        {
            rdtDebug.Debug(this, "WriteAllFields", new object[0]);
            for (int i = 0; i < allFields.Count; i++)
            {
                rdtTcpMessageComponents.Property property = allFields[i];
                object obj2 = property.m_value;
                object obj3 = this.Deserialize(obj2);
                if (obj3 != null)
                {
                    if (obj3 is rdtSerializerSlider)
                    {
                        obj3 = ((rdtSerializerSlider) obj3).Value;
                    }
                    if (property.m_type == rdtTcpMessageComponents.Property.Type.Property)
                    {
                        PropertyInfo info = owner.GetType().GetProperty(property.m_name);
                        if (info != null)
                        {
                            List<rdtTcpMessageComponents.Property> list = property.m_value as List<rdtTcpMessageComponents.Property>;
                            if (list != null)
                            {
                                object obj4 = info.GetValue(owner, null);
                                this.WriteAllFields(obj4, list);
                                info.SetValue(owner, obj4, null);
                            }
                            else
                            {
                                try
                                {
                                    rdtDebug.Debug(this, "Setting property {0} to {1}", new object[] { property.m_name, obj3.ToString() });
                                    info.SetValue(owner, obj3, null);
                                }
                                catch (Exception exception)
                                {
                                    rdtDebug.Warning(this, "Property '{0}' could not be set: {1}!", new object[] { property.m_name, exception.Message });
                                }
                            }
                        }
                    }
                    else if (property.m_type == rdtTcpMessageComponents.Property.Type.Field)
                    {
                        FieldInfo fieldInHierarchy = rdtTypeExtensions.GetFieldInHierarchy(owner.GetType(), property.m_name);
                        if (fieldInHierarchy != null)
                        {
                            List<rdtTcpMessageComponents.Property> list2 = property.m_value as List<rdtTcpMessageComponents.Property>;
                            if (list2 != null)
                            {
                                object obj5 = fieldInHierarchy.GetValue(owner);
                                this.WriteAllFields(obj5, list2);
                                fieldInHierarchy.SetValue(owner, obj5);
                            }
                            else
                            {
                                try
                                {
                                    rdtDebug.Debug(this, "Setting field {0} to {1}", new object[] { property.m_name, obj3.ToString() });
                                    fieldInHierarchy.SetValue(owner, obj3);
                                }
                                catch (ArgumentException exception2)
                                {
                                    rdtDebug.Error(this, "'{0}' could not be assigned: {1}!", new object[] { property.m_name, exception2.Message });
                                }
                            }
                        }
                    }
                    else
                    {
                        MethodInfo method = owner.GetType().GetMethod(property.m_name);
                        if (method != null)
                        {
                            try
                            {
                                if (((rdtSerializerButton) property.m_value).Pressed)
                                {
                                    method.Invoke(owner, null);
                                }
                            }
                            catch (Exception exception3)
                            {
                                rdtDebug.Error(this, "'{0}' could not be invoked: {1}!", new object[] { property.m_name, exception3.Message });
                            }
                        }
                    }
                }
            }
        }

        private delegate object ConvertObjectDelegate(object objIn, rdtSerializerRegistry registry);
    }
}

