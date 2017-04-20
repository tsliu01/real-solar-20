namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_5
    using UnityEngine.SceneManagement;
#endif

    public class rdtMessageGameObjectsHandler
    {
        private List<rdtTcpMessageGameObjects.Gob> m_allGobs = new List<rdtTcpMessageGameObjects.Gob>(0x800);
        private List<rdtTcpMessageComponents.Component> m_components = new List<rdtTcpMessageComponents.Component>(0x10);
        private List<GameObject> m_gameObjects = new List<GameObject>(0x800);
        private RemoteDebugServer m_server;
        private List<UnityEngine.Component> m_unityComponents = new List<UnityEngine.Component>(0x10);

        public rdtMessageGameObjectsHandler(RemoteDebugServer server)
        {
            this.m_server = server;
            this.m_server.AddCallback(typeof(rdtTcpMessageGetGameObjects), new Action<rdtTcpMessage>(this.OnRequestGameObjects));
            this.m_server.AddCallback(typeof(rdtTcpMessageGetComponents), new Action<rdtTcpMessage>(this.OnRequestGameObjectComponents));
            this.m_server.AddCallback(typeof(rdtTcpMessageUpdateComponentProperties), new Action<rdtTcpMessage>(this.OnUpdateComponentProperties));
            this.m_server.AddCallback(typeof(rdtTcpMessageUpdateGameObjectProperties), new Action<rdtTcpMessage>(this.OnUpdateGameObjectProperties));
        }

        private void AddGameObject(GameObject g, List<rdtTcpMessageGameObjects.Gob> list)
        {
            rdtTcpMessageGameObjects.Gob item = new rdtTcpMessageGameObjects.Gob();
            item.m_name = g.name;
            item.m_instanceId = g.GetInstanceID();
            Transform parent = g.transform.parent;
            item.m_hasParent = parent != null;
            if (item.m_hasParent)
            {
                item.m_parentInstanceId = parent.gameObject.GetInstanceID();
            }
            item.m_enabled = g.activeInHierarchy;
            list.Add(item);
            for (int i = 0; i < g.transform.childCount; i++)
            {
                Transform child = g.transform.GetChild(i);
                this.AddGameObject(child.gameObject, list);
            }
        }

        private UnityEngine.Component FindComponent(GameObject gob, int instanceId)
        {
            gob.GetComponents<UnityEngine.Component>(this.m_unityComponents);
            for (int i = 0; i < this.m_unityComponents.Count; i++)
            {
                UnityEngine.Component component = this.m_unityComponents[i];
                if (component.GetInstanceID() == instanceId)
                {
                    return component;
                }
            }
            return null;
        }

        public GameObject FindGameObject(int instanceId)
        {
#if UNITY_5
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.rootCount > this.m_gameObjects.Capacity)
            {
                this.m_gameObjects.Capacity = activeScene.rootCount;
            }
            this.m_gameObjects.Clear();
            activeScene.GetRootGameObjects(this.m_gameObjects);
#else
            m_gameObjects.Clear();
            Transform[] allTrans = GameObject.FindObjectsOfType<Transform>();
            for (int ii = 0; ii < allTrans.Length; ++ii)
            {
                Transform tran = allTrans[ii];
                if (tran.parent != null)
                    continue;
                m_gameObjects.Add(tran.gameObject);
            }
#endif
            int count = this.m_gameObjects.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject parent = this.m_gameObjects[i];
                GameObject obj3 = this.FindGameObject(instanceId, parent);
                if (obj3 != null)
                {
                    return obj3;
                }
            }
            return null;
        }

        private GameObject FindGameObject(int instanceId, GameObject parent)
        {
            if (parent.GetInstanceID() == instanceId)
            {
                return parent;
            }
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject gameObject = parent.transform.GetChild(i).gameObject;
                GameObject obj3 = this.FindGameObject(instanceId, gameObject);
                if (obj3 != null)
                {
                    return obj3;
                }
            }
            return null;
        }

        private void OnRequestGameObjectComponents(rdtTcpMessage message)
        {
            rdtTcpMessageGetComponents components = (rdtTcpMessageGetComponents) message;
            if (components.m_instanceId != 0)
            {
                GameObject obj2 = this.FindGameObject(components.m_instanceId);
                if (obj2 != null)
                {
                    rdtTcpMessageComponents components2 = new rdtTcpMessageComponents();
                    components2.m_instanceId = components.m_instanceId;
                    components2.m_components = new List<rdtTcpMessageComponents.Component>();
                    components2.m_layer = obj2.layer;
                    components2.m_tag = obj2.tag;
                    components2.m_enabled = obj2.activeSelf;
                    this.m_components.Clear();
                    obj2.GetComponents<UnityEngine.Component>(this.m_unityComponents);
                    if (this.m_unityComponents.Count > this.m_components.Capacity)
                    {
                        this.m_components.Capacity = this.m_unityComponents.Count;
                    }
                    for (int i = 0; i < this.m_unityComponents.Count; i++)
                    {
                        UnityEngine.Component owner = this.m_unityComponents[i];
                        if (owner == null)
                        {
                            rdtDebug.Debug(this, "Component is null, skipping", new object[0]);
                        }
                        else
                        {
                            List<rdtTcpMessageComponents.Property> list = this.m_server.SerializerRegistry.ReadAllFields(owner);
                            if (list == null)
                            {
                                rdtDebug.Debug(this, "Properties are null, skipping", new object[0]);
                            }
                            else
                            {
                                rdtTcpMessageComponents.Component item = new rdtTcpMessageComponents.Component();
                                if (owner is Behaviour)
                                {
                                    item.m_canBeDisabled = true;
                                    item.m_enabled = ((Behaviour) owner).enabled;
                                }
                                else if (owner is Renderer)
                                {
                                    item.m_canBeDisabled = true;
                                    item.m_enabled = ((Renderer) owner).enabled;
                                }
                                else if (owner is Collider)
                                {
                                    item.m_canBeDisabled = true;
                                    item.m_enabled = ((Collider) owner).enabled;
                                }
                                else
                                {
                                    item.m_canBeDisabled = false;
                                    item.m_enabled = true;
                                }
                                System.Type type = owner.GetType();
                                item.m_name = type.Name;
                                item.m_assemblyName = type.AssemblyQualifiedName;
                                item.m_instanceId = owner.GetInstanceID();
                                item.m_properties = list;
                                this.m_components.Add(item);
                            }
                        }
                    }
                    components2.m_components = this.m_components;
                    this.m_unityComponents.Clear();
                    this.m_server.EnqueueMessage(components2);
                }
            }
        }

        private void OnRequestGameObjects(rdtTcpMessage message)
        {
            rdtTcpMessageGameObjects objects = new rdtTcpMessageGameObjects();
#if UNITY_5
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.rootCount > this.m_gameObjects.Capacity)
            {
                this.m_gameObjects.Capacity = activeScene.rootCount;
            }
            this.m_gameObjects.Clear();
            activeScene.GetRootGameObjects(this.m_gameObjects);
#else
            m_gameObjects.Clear();
            Transform[] allTrans = GameObject.FindObjectsOfType<Transform>();
            for (int ii = 0; ii < allTrans.Length; ++ii)
            {
                Transform tran = allTrans[ii];
                if (tran.parent != null)
                    continue;
                m_gameObjects.Add(tran.gameObject);
            }
#endif
            int count = this.m_gameObjects.Count;
            List<GameObject> gameObjects = this.m_gameObjects;
            if (count > this.m_allGobs.Capacity)
            {
                this.m_allGobs.Capacity = count;
            }
            this.m_allGobs.Clear();
            for (int i = 0; i < count; i++)
            {
                GameObject g = gameObjects[i];
                if ((g.hideFlags == HideFlags.None) && (g.transform.hideFlags == HideFlags.None))
                {
                    this.AddGameObject(g, this.m_allGobs);
                }
            }
            objects.m_allGobs = this.m_allGobs;
            this.m_server.EnqueueMessage(objects);
            this.m_gameObjects.Clear();
        }

        private void OnUpdateComponentProperties(rdtTcpMessage message)
        {
            rdtDebug.Debug(this, "OnUpdateComponentProperties", new object[0]);
            rdtTcpMessageUpdateComponentProperties properties = (rdtTcpMessageUpdateComponentProperties) message;
            GameObject gob = this.FindGameObject(properties.m_gameObjectInstanceId);
            if (gob != null)
            {
                UnityEngine.Component owner = this.FindComponent(gob, properties.m_componentInstanceId);
                if (owner == null)
                {
                    rdtDebug.Error(this, "Tried to update component with id {0} (name={1}) but couldn't find it!", new object[] { properties.m_componentInstanceId, properties.m_componentName });
                }
                else
                {
                    if (owner is Behaviour)
                    {
                        ((Behaviour) owner).enabled = properties.m_enabled;
                    }
                    else if (owner is Renderer)
                    {
                        ((Renderer) owner).enabled = properties.m_enabled;
                    }
                    else if (owner is Collider)
                    {
                        ((Collider) owner).enabled = properties.m_enabled;
                    }
                    if (properties.m_properties != null)
                    {
                        this.m_server.SerializerRegistry.WriteAllFields(owner, properties.m_properties);
                    }
                }
            }
        }

        private void OnUpdateGameObjectProperties(rdtTcpMessage message)
        {
            rdtTcpMessageUpdateGameObjectProperties properties = (rdtTcpMessageUpdateGameObjectProperties) message;
            GameObject obj2 = this.FindGameObject(properties.m_instanceId);
            if (obj2 != null)
            {
                obj2.SetActive(properties.m_enabled);
                obj2.layer = properties.m_layer;
                obj2.tag = properties.m_tag;
            }
        }
    }
}

