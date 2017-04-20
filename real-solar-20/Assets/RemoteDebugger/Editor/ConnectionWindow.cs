namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using UnityEngine;

    public class ConnectionWindow : EditorWindow
    {
        private const float LABEL_ADJUST_SIZE_THRESHOLD = 350f;
        [NonSerialized]
        private bool m_clearFocus;
        private rdtClient m_client;
        private double m_componentRefreshTimer;
        private rdtTcpMessageComponents? m_components;
        private Vector2 m_componentsScrollPos;
        private rdtServerAddress m_currentServer;
        private bool m_debug;
        private rdtExpandedCache m_expandedCache = new rdtExpandedCache();
        private double m_gameObjectRefreshTimer;
        [NonSerialized]
        private List<rdtTcpMessageGameObjects.Gob> m_gameObjects;
        private bool m_isProSkin;
        private double m_lastTime;
        private GUIStyle m_normalFoldoutStyle;
        [NonSerialized]
        private rdtTcpMessageComponents.Component? m_pendingExpandComponent;
        [NonSerialized]
        private bool m_running;
        [NonSerialized]
        private rdtTcpMessageGameObjects.Gob? m_selected;
        private rdtSerializerRegistry m_serializerRegistry = new rdtSerializerRegistry();
        private rdtClientEnumerateServers m_serverEnum = new rdtClientEnumerateServers();
        private rdtServersMenu m_serversMenu;
        private rdtGuiSplit m_split;
        private GUIStyle m_toggleStyle;
        private rdtGuiTree<rdtTcpMessageGameObjects.Gob> m_tree;
        private const float WIDE_MODE_SIZE_THRESHOLD = 330f;

        public ConnectionWindow()
        {
            base.minSize = new Vector2(400f, 100f);
            this.m_split = new rdtGuiSplit(200f, 100f, this);
            this.m_tree = new rdtGuiTree<rdtTcpMessageGameObjects.Gob>();
            this.m_tree.SelectionChanged += new Action<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node, rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>(this.OnTreeSelectionChanged);
            this.m_serversMenu = new rdtServersMenu(new Action<rdtServerAddress>(this.OnServerSelected));
            EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction) Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(this.OnPlaymodeStateChanged));
        }

        private void AddChildren(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node parentNode, List<rdtTcpMessageGameObjects.Gob> nonRoots)
        {
            for (int i = 0; i < nonRoots.Count; i++)
            {
                rdtTcpMessageGameObjects.Gob data = nonRoots[i];
                if (data.m_parentInstanceId == parentNode.Data.m_instanceId)
                {
                    rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = parentNode.AddNode(data, data.m_enabled);
                    this.AddChildren(node, nonRoots);
                }
            }
        }

        private void BuildTree()
        {
            List<rdtTcpMessageGameObjects.Gob> list = Enumerable.ToList<rdtTcpMessageGameObjects.Gob>(Enumerable.Where<rdtTcpMessageGameObjects.Gob>(this.m_gameObjects, delegate (rdtTcpMessageGameObjects.Gob x) {
                return !x.m_hasParent;
            }));
            List<rdtTcpMessageGameObjects.Gob> nonRoots = Enumerable.ToList<rdtTcpMessageGameObjects.Gob>(Enumerable.Where<rdtTcpMessageGameObjects.Gob>(this.m_gameObjects, delegate (rdtTcpMessageGameObjects.Gob x) {
                return x.m_hasParent;
            }));
            for (int i = 0; i < list.Count; i++)
            {
                rdtTcpMessageGameObjects.Gob data = list[i];
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node parentNode = this.m_tree.AddNode(data, data.m_enabled);
                this.AddChildren(parentNode, nonRoots);
            }
        }

        private void CheckValueChanged(object oldValue, object newValue, Stack<rdtTcpMessageComponents.Property> hierarchy, rdtTcpMessageComponents.Component component)
        {
            if (newValue != null)
            {
                bool flag = false;
                if (oldValue.GetType().IsArray)
                {
                    Array array = (Array) oldValue;
                    Array array2 = (Array) newValue;
                    flag = array.Length != array2.Length;
                    if (!flag)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            object obj2 = array.GetValue(i);
                            object obj3 = array2.GetValue(i);
                            flag = ((obj2 == null) && (obj3 != null)) || ((obj2 != null) && (obj3 == null));
                            if (!flag && (obj2 != null))
                            {
                                flag = !obj2.Equals(obj3);
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    flag = !newValue.Equals(oldValue);
                }
                if (flag)
                {
                    rdtTcpMessageComponents.Property[] collection = hierarchy.ToArray();
                    collection[0].m_value = newValue;
                    hierarchy = new Stack<rdtTcpMessageComponents.Property>(collection);
                    this.OnPropertyChanged(new rdtTcpMessageComponents.Component?(component), hierarchy, null);
                }
            }
        }

        private List<rdtTcpMessageComponents.Property> CloneAndSerialize(Stack<rdtTcpMessageComponents.Property> hierarchy)
        {
            List<rdtTcpMessageComponents.Property> list = new List<rdtTcpMessageComponents.Property>();
            rdtTcpMessageComponents.Property property = hierarchy.Peek();
            rdtTcpMessageComponents.Property item = property.Clone();
            item.m_value = this.m_serializerRegistry.Serialize(property.m_value);
            bool flag = true;
            foreach (rdtTcpMessageComponents.Property property3 in hierarchy)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    rdtTcpMessageComponents.Property property4 = property3.Clone();
                    List<rdtTcpMessageComponents.Property> list2 = new List<rdtTcpMessageComponents.Property>();
                    list2.Add(item);
                    property4.m_value = list2;
                    item = property4;
                }
            }
            list.Add(item);
            return list;
        }

        public void Connect(rdtServerAddress address)
        {
            this.Disconnect(true);
            this.m_expandedCache.Clear();
            this.m_pendingExpandComponent = null;
            this.m_components = null;
            this.m_selected = null;
            this.m_currentServer = address;
            this.m_client = new rdtClient();
            this.m_client.Connect(address.IPAddress);
            this.m_client.AddCallback(typeof(rdtTcpMessageGameObjects), new Action<rdtTcpMessage>(this.OnMessageGameObjects));
            this.m_client.AddCallback(typeof(rdtTcpMessageComponents), new Action<rdtTcpMessage>(this.OnMessageGameObjectComponents));
        }

        private void Disconnect([Optional, DefaultParameterValue(true)] bool resetServer)
        {
            if (resetServer)
            {
                this.m_currentServer = null;
            }
            if (this.m_client != null)
            {
                this.m_client.Stop();
            }
            this.m_tree.Clear();
            this.m_selected = null;
            this.m_components = null;
        }

        private void Draw()
        {
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            bool windowHasFocus = EditorWindow.focusedWindow == this;
            this.m_tree.Draw(this.m_split.SeparatorPosition, windowHasFocus);
            this.m_split.Draw();
            if (this.m_selected.HasValue && this.m_components.HasValue)
            {
                this.m_componentsScrollPos = EditorGUILayout.BeginScrollView(this.m_componentsScrollPos, new GUILayoutOption[0]);
                float num = base.position.width - this.m_split.SeparatorPosition;
                EditorGUIUtility.wideMode = num >= 330f;
                EditorGUIUtility.labelWidth = 0f;
                EditorGUIUtility.fieldWidth = 0f;
                if (num > 350f)
                {
                    float num2 = num - 350f;
                    EditorGUIUtility.labelWidth = (num2 * 0.5f) + EditorGUIUtility.labelWidth;
                }
                this.DrawComponents();
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawComponent(rdtTcpMessageComponents.Component component, Stack<rdtTcpMessageComponents.Property> hierarchy, List<rdtTcpMessageComponents.Property> properties)
        {
            if ((properties != null) && (properties.Count != 0))
            {
                foreach (rdtTcpMessageComponents.Property property in properties)
                {
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                    string content = ObjectNames.NicifyVariableName(property.m_name);
                    object propValue = property.m_value;
                    bool flag = rdtGuiProperty.HasFoldout(propValue);
                    if (!flag)
                    {
                        GUILayout.Space((float) ((EditorStyles.foldout.padding.left + EditorStyles.foldout.margin.left) - EditorStyles.label.padding.left));
                    }
                    bool foldout = flag ? this.m_expandedCache.IsExpanded(component, property) : false;
                    System.Type type = propValue.GetType();
                    hierarchy.Push(property);
                    if (type == typeof(List<rdtTcpMessageComponents.Property>))
                    {
                        EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
                        GUILayout.Label("", new GUILayoutOption[0]);
                        foldout = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), foldout, content, true);
                        if (foldout)
                        {
                            EditorGUI.indentLevel++;
                            List<rdtTcpMessageComponents.Property> list = (List<rdtTcpMessageComponents.Property>) propValue;
                            this.DrawComponent(component, hierarchy, list);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        object newValue = rdtGuiProperty.Draw(content, propValue, ref foldout);
                        this.CheckValueChanged(propValue, newValue, hierarchy, component);
                    }
                    hierarchy.Pop();
                    if (flag)
                    {
                        this.m_expandedCache.SetExpanded(foldout, component, property);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawComponents()
        {
            this.DrawGameObjectTitle();
            rdtGuiLine.DrawHorizontalLine();
            foreach (rdtTcpMessageComponents.Component component in this.m_components.Value.m_components)
            {
                if (!this.DrawComponentTitle(component, null))
                {
                    rdtGuiLine.DrawHorizontalLine();
                }
                else
                {
                    Stack<rdtTcpMessageComponents.Property> hierarchy = new Stack<rdtTcpMessageComponents.Property>();
                    this.DrawComponent(component, hierarchy, component.m_properties);
                    EditorGUILayout.Space();
                    rdtGuiLine.DrawHorizontalLine();
                }
            }
            EditorGUILayout.Space();
        }

        private bool DrawComponentTitle(rdtTcpMessageComponents.Component component, [Optional, DefaultParameterValue(null)] UnityEngine.Component unityComponent)
        {
            bool foldout = this.m_expandedCache.IsExpanded(component);
            bool expanded = foldout;
            Rect rect = EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Height(18f) });
            Rect position = GUILayoutUtility.GetRect((float) 13f, (float) 16f, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
            if ((component.m_properties != null) && (component.m_properties.Count > 0))
            {
                expanded = EditorGUI.Foldout(position, foldout, GUIContent.none, this.m_normalFoldoutStyle);
                if (expanded != foldout)
                {
                    this.m_expandedCache.SetExpanded(expanded, component);
                }
            }
            bool flag3 = true;
            int width = this.m_toggleStyle.normal.background.width;
            if (component.m_canBeDisabled)
            {
                flag3 = EditorGUILayout.Toggle(component.m_enabled, this.m_toggleStyle, new GUILayoutOption[] { GUILayout.Width((float) width) });
            }
            else
            {
                EditorGUILayout.LabelField("", new GUILayoutOption[] { GUILayout.Width((float) width) });
            }
            string label = ObjectNames.NicifyVariableName(component.m_name);
            if (this.m_debug)
            {
                label = label + ":" + component.m_instanceId;
            }
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, new GUILayoutOption[0]);
            if (component.m_canBeDisabled && (flag3 != component.m_enabled))
            {
                component.m_enabled = flag3;
                this.OnPropertyChanged(new rdtTcpMessageComponents.Component?(component), null, null);
            }
            EditorGUILayout.EndHorizontal();
            if ((component.m_properties != null) && (component.m_properties.Count > 0))
            {
                Event current = Event.current;
                if (!rect.Contains(current.mousePosition) || !current.isMouse)
                {
                    return expanded;
                }
                if (current.type == EventType.MouseDown)
                {
                    this.m_pendingExpandComponent = new rdtTcpMessageComponents.Component?(component);
                    current.Use();
                    return expanded;
                }
                if (((current.type == EventType.MouseUp) && this.m_pendingExpandComponent.HasValue) && (this.m_pendingExpandComponent.Value.m_instanceId != component.m_instanceId))
                {
                    this.m_pendingExpandComponent = null;
                }
            }
            return expanded;
        }

        private void DrawGameObjectTitle()
        {
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            bool flag = EditorGUILayout.ToggleLeft("Enabled", this.m_components.Value.m_enabled, new GUILayoutOption[0]);
            if (flag != this.m_components.Value.m_enabled)
            {
                rdtTcpMessageComponents components = this.m_components.Value;
                components.m_enabled = flag;
                this.m_components = new rdtTcpMessageComponents?(components);
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = this.m_tree.FindNode(this.m_selected.Value);
                if (node != null)
                {
                    node.Enabled = flag;
                }
                this.OnGameObjectChanged();
            }
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
            EditorGUILayout.LabelField("Tag", new GUILayoutOption[] { GUILayout.Width(30f) });
            string str = EditorGUILayout.TagField(GUIContent.none, this.m_components.Value.m_tag, new GUILayoutOption[] { GUILayout.MinWidth(50f) });
            if (str != this.m_components.Value.m_tag)
            {
                rdtTcpMessageComponents components2 = this.m_components.Value;
                components2.m_tag = str;
                this.m_components = new rdtTcpMessageComponents?(components2);
                this.OnGameObjectChanged();
            }
            EditorGUILayout.LabelField("Layer", new GUILayoutOption[] { GUILayout.Width(40f) });
            int num = EditorGUILayout.LayerField(GUIContent.none, this.m_components.Value.m_layer, new GUILayoutOption[] { GUILayout.MinWidth(50f) });
            if (num != this.m_components.Value.m_layer)
            {
                rdtTcpMessageComponents components3 = this.m_components.Value;
                components3.m_layer = num;
                this.m_components = new rdtTcpMessageComponents?(components3);
                this.OnGameObjectChanged();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[0]);
            this.m_serversMenu.Show(this.m_currentServer);
            bool flag = (this.m_client != null) && this.m_client.IsConnecting;
            bool flag2 = (this.m_client != null) && this.m_client.IsConnected;
            string text = flag ? "Connecting" : ((flag2 && (this.m_currentServer != null)) ? this.m_currentServer.ToString() : "Not connected");
            if ((flag2 && (this.m_currentServer != null)) && this.m_debug)
            {
                text = text + " - Server Version " + this.m_currentServer.m_serverVersion;
            }
            GUILayout.Label(text, EditorStyles.toolbarButton, new GUILayoutOption[0]);
            GUILayout.FlexibleSpace();
            bool flag3 = GUILayout.Toggle(this.m_debug, "Debug", EditorStyles.toolbarButton, new GUILayoutOption[0]);
            if (flag3 != this.m_debug)
            {
                this.m_debug = flag3;
                rdtDebug.s_logLevel = flag3 ? rdtDebug.LogLevel.Debug : rdtDebug.LogLevel.Info;
            }
            if (GUILayout.Button("About", EditorStyles.toolbarButton, new GUILayoutOption[0]))
            {
                this.ShowAbout();
            }
            GUILayout.EndHorizontal();
        }

        private void InitStyles()
        {
            if (((this.m_isProSkin != EditorGUIUtility.isProSkin) || (this.m_toggleStyle == null)) || ((this.m_normalFoldoutStyle == null) || (this.m_toggleStyle.normal.background == null)))
            {
                this.m_isProSkin = EditorGUIUtility.isProSkin;
                this.m_toggleStyle = new GUIStyle(EditorStyles.toggle);
                this.m_toggleStyle.overflow.top = -2;
                this.m_normalFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                this.m_normalFoldoutStyle.overflow.top = -2;
                this.m_normalFoldoutStyle.active.textColor = EditorStyles.foldout.normal.textColor;
                this.m_normalFoldoutStyle.onActive.textColor = EditorStyles.foldout.normal.textColor;
                this.m_normalFoldoutStyle.onFocused.textColor = EditorStyles.foldout.normal.textColor;
                this.m_normalFoldoutStyle.onFocused.background = EditorStyles.foldout.onNormal.background;
                this.m_normalFoldoutStyle.focused.textColor = EditorStyles.foldout.normal.textColor;
                this.m_normalFoldoutStyle.focused.background = EditorStyles.foldout.normal.background;
            }
        }

        private void OnConnectionStatusChanged()
        {
            if ((this.m_client == null) || !this.m_client.IsConnected)
            {
                this.m_currentServer = null;
            }
            this.m_pendingExpandComponent = null;
            this.m_components = null;
            this.m_expandedCache.Clear();
            this.m_tree.Clear();
            base.Repaint();
        }

        private void OnDestroy()
        {
            this.Disconnect(true);
            if (this.m_serverEnum != null)
            {
                this.m_serverEnum.Stop();
            }
        }

        private void OnEnable()
        {
            this.m_isProSkin = EditorGUIUtility.isProSkin;
            this.m_expandedCache.Clear();
            this.m_tree.OnEnable();
        }

        private void OnGameObjectChanged()
        {
            if (this.m_client != null)
            {
                rdtTcpMessageUpdateGameObjectProperties message = new rdtTcpMessageUpdateGameObjectProperties();
                message.m_instanceId = this.m_selected.Value.m_instanceId;
                message.m_enabled = this.m_components.Value.m_enabled;
                message.m_layer = this.m_components.Value.m_layer;
                message.m_tag = this.m_components.Value.m_tag;
                this.m_client.EnqueueMessage(message);
            }
        }

        private void OnGUI()
        {
            if (this.m_clearFocus)
            {
                this.m_clearFocus = false;
                GUI.FocusControl(null);
            }
            this.InitStyles();
            this.DrawToolbar();
            this.Draw();
            this.ProcessInput();
        }

        private void OnMessageGameObjectComponents(rdtTcpMessage message)
        {
            this.m_components = new rdtTcpMessageComponents?((rdtTcpMessageComponents) message);
            List<rdtTcpMessageComponents.Component> components = this.m_components.Value.m_components;
            for (int i = 0; i < components.Count; i++)
            {
                rdtTcpMessageComponents.Component component = components[i];
                if (component.m_properties == null)
                {
                    rdtDebug.Debug(this, "Component '{0}' has no properties", new object[] { component.m_name });
                }
                else
                {
                    for (int j = 0; j < component.m_properties.Count; j++)
                    {
                        rdtTcpMessageComponents.Property property = component.m_properties[j];
                        property.Deserialise(this.m_serializerRegistry);
                        component.m_properties[j] = property;
                    }
                    components[i] = component;
                }
            }
            base.Repaint();
        }

        private void OnMessageGameObjects(rdtTcpMessage message)
        {
            rdtTcpMessageGameObjects objects = (rdtTcpMessageGameObjects) message;
            this.m_gameObjects = objects.m_allGobs;
            this.m_tree.Clear();
            this.BuildTree();
            if (this.m_selected.HasValue)
            {
                this.m_tree.SetSelected(this.m_selected.Value, true, false, false);
            }
            else
            {
                this.m_tree.ClearSelected(true, false, false);
            }
            base.Repaint();
        }

        private void OnPlaymodeStateChanged()
        {
            this.Disconnect(true);
        }

        private void OnPropertyChanged(rdtTcpMessageComponents.Component? component, [Optional, DefaultParameterValue(null)] Stack<rdtTcpMessageComponents.Property> hierarchy, [Optional, DefaultParameterValue(null)] UnityEngine.Component unityComponent)
        {
            if ((this.m_client != null) && component.HasValue)
            {
                rdtTcpMessageUpdateComponentProperties message = new rdtTcpMessageUpdateComponentProperties();
                message.m_gameObjectInstanceId = this.m_selected.Value.m_instanceId;
                message.m_componentName = component.Value.m_name;
                message.m_componentInstanceId = component.Value.m_instanceId;
                message.m_enabled = component.Value.m_enabled;
                if ((hierarchy != null) && (hierarchy.Count > 0))
                {
                    message.m_properties = this.CloneAndSerialize(hierarchy);
                }
                this.m_client.EnqueueMessage(message);
            }
        }

        private void OnServerSelected(rdtServerAddress server)
        {
            if (server != null)
            {
                this.Connect(server);
            }
            else
            {
                this.Disconnect(true);
            }
        }

        private void OnTreeSelectionChanged(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node prevSelection, rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node newSelection)
        {
            this.m_clearFocus = true;
            if (this.m_client != null)
            {
                rdtTcpMessageGetComponents message = new rdtTcpMessageGetComponents();
                message.m_instanceId = 0;
                this.m_client.EnqueueMessage(message);
            }
            this.m_selected = null;
            if (newSelection != null)
            {
                this.m_selected = new rdtTcpMessageGameObjects.Gob?(newSelection.Data);
            }
            if (!this.m_selected.HasValue)
            {
                this.m_pendingExpandComponent = null;
                this.m_components = null;
            }
            else if (this.m_client != null)
            {
                rdtTcpMessageGetComponents components2 = new rdtTcpMessageGetComponents();
                components2.m_instanceId = this.m_selected.Value.m_instanceId;
                this.m_client.EnqueueMessage(components2);
            }
            base.Repaint();
        }

        private void OnUnityReloadedAssemblies()
        {
            rdtDebug.Debug(this, "OnUnityReloadedAssemblies", new object[0]);
            rdtDebug.s_logLevel = this.m_debug ? rdtDebug.LogLevel.Debug : rdtDebug.LogLevel.Info;
            this.m_running = true;
            if (this.m_serverEnum != null)
            {
                this.m_serverEnum.Stop();
            }
            this.m_serverEnum = new rdtClientEnumerateServers();
            if (this.m_currentServer != null)
            {
                if (this.m_currentServer.IPAddress == null)
                {
                    this.m_currentServer = null;
                }
                else
                {
                    this.Connect(this.m_currentServer);
                }
            }
        }

        private void ProcessInput()
        {
            Event current = Event.current;
            if ((current.isMouse && (current.type == EventType.MouseUp)) && this.m_pendingExpandComponent.HasValue)
            {
                GUI.FocusControl(null);
                bool flag = this.m_expandedCache.IsExpanded(this.m_pendingExpandComponent.Value);
                this.m_expandedCache.SetExpanded(!flag, this.m_pendingExpandComponent.Value);
                this.m_pendingExpandComponent = null;
                base.Repaint();
            }
        }

        private void ShowAbout()
        {
            string format = "\r\nHdg Remote Debug\r\nVersion {0}.{1}.{2} {3} {4}\r\n\r\nhttp://www.horsedrawngames.com\r\ninfo@horsedrawngames.com\r\n\r\n(c) 2016 Horse Drawn Games Pty Ltd";
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Version version = executingAssembly.GetName().Version;
            string productVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).ProductVersion;
            object[] customAttributes = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            string configuration = "";
            if (customAttributes.Length > 0)
            {
                configuration = ((AssemblyConfigurationAttribute) customAttributes[0]).Configuration;
            }
            EditorUtility.DisplayDialog("About", string.Format(format, new object[] { version.Major, version.Minor, version.Build, productVersion, configuration }), "Ok");
        }

        [UnityEditor.MenuItem("Window/网虫虫 Remote Debug")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ConnectionWindow>(false, "Remote Debug", true);
        }

        private void Update()
        {
            if ((EditorApplication.isCompiling && (this.m_client != null)) && this.m_client.IsConnected)
            {
                this.Disconnect(false);
            }
            if (!this.m_running)
            {
                this.OnUnityReloadedAssemblies();
            }
            double timeSinceStartup = EditorApplication.timeSinceStartup;
            double delta = timeSinceStartup - this.m_lastTime;
            this.m_lastTime = timeSinceStartup;
            this.UpdateServers(delta);
            if (this.m_serverEnum.Stopped)
            {
                this.m_serverEnum = new rdtClientEnumerateServers();
            }
            this.m_tree.Update();
            if (this.m_client != null)
            {
                bool isConnected = this.m_client.IsConnected;
                bool isConnecting = this.m_client.IsConnecting;
                this.m_client.Update(delta);
                if ((this.m_client.IsConnected != isConnected) || ((!this.m_client.IsConnected && !this.m_client.IsConnecting) && isConnecting))
                {
                    this.OnConnectionStatusChanged();
                }
                this.m_gameObjectRefreshTimer -= delta;
                if (this.m_gameObjectRefreshTimer <= 0.0)
                {
                    this.m_client.EnqueueMessage(new rdtTcpMessageGetGameObjects());
                    this.m_gameObjectRefreshTimer = rdtSettings.GAMEOBJECT_UPDATE_TIME;
                }
                if (this.m_selected.HasValue)
                {
                    this.m_componentRefreshTimer -= delta;
                    if (this.m_componentRefreshTimer <= 0.0)
                    {
                        rdtTcpMessageGetComponents message = new rdtTcpMessageGetComponents();
                        message.m_instanceId = this.m_selected.Value.m_instanceId;
                        this.m_client.EnqueueMessage(message);
                        this.m_componentRefreshTimer = rdtSettings.COMPONENT_UPDATE_TIME;
                    }
                }
            }
        }

        private void UpdateServers(double delta)
        {
            this.m_serverEnum.Update(delta);
            this.m_serversMenu.Servers = this.m_serverEnum.Servers;
        }
    }
}

