namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    public class rdtGuiTree<T>
    {
        private GUIContent m_content;
        private GUIStyle m_disabledLineStyle;
        private rdtExpandedCache m_expandedCache;
        private GUIStyle m_foldoutStyle;
        private bool m_hasFocus;
        private bool m_hasPendingSelection;
        private int m_indent;
        private bool m_isProSkin;
        private GUIStyle m_lineStyle;
        private float m_maxWidth;
        private bool m_mouseWasDown;
        private Node m_pendingSelection;
        private Node m_root;
        private Vector2 m_scrollPosition;
        private Node m_selected;
        private Rect m_treeRect;
        private bool m_triggerCallbacks;
        private const float ROW_HEIGHT = 18f;

        public event Action<Node, Node> SelectionChanged;

        public rdtGuiTree()
        {
            this.m_content = new GUIContent();
            this.m_root = new Node();
            this.m_expandedCache = new rdtExpandedCache();
        }

        public Node AddNode(T data, [Optional, DefaultParameterValue(true)] bool enabled)
        {
            Node item = new Node(data, this.m_root);
            item.Enabled = enabled;
            this.m_root.Children.Add(item);
            return item;
        }

        private List<Node> BuildFlatList(List<Node> nodes, [Optional, DefaultParameterValue(false)] bool allNodes)
        {
            List<Node> list = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                Node item = nodes[i];
                list.Add(item);
                if (allNodes || this.m_expandedCache.IsExpanded(item.Data.GetHashCode(), null))
                {
                    list.AddRange(this.BuildFlatList(item.Children, false));
                }
            }
            return list;
        }

        public void Clear()
        {
            this.m_root.Children.Clear();
        }

        public void ClearSelected(bool immediate, bool autoScroll, bool triggerCallbacks)
        {
            this.SetSelected((Node) null, immediate, autoScroll, triggerCallbacks);
        }

        public void Draw(float width, bool windowHasFocus)
        {
            this.m_hasFocus &= windowHasFocus;
            this.InitStyles();
            this.ProcessKeyboardInput();
            this.m_scrollPosition = EditorGUILayout.BeginScrollView(this.m_scrollPosition, new GUILayoutOption[] { GUILayout.Width(width) });
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            if (Event.current.type == EventType.Repaint)
            {
                this.m_maxWidth = 0f;
            }
            this.m_indent = 0;
            this.DrawNodes(this.m_root.Children);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    this.m_mouseWasDown = this.m_treeRect.Contains(Event.current.mousePosition);
                    return;

                case EventType.MouseUp:
                    if (!this.m_mouseWasDown)
                    {
                        this.m_hasFocus = false;
                        break;
                    }
                    this.m_mouseWasDown = false;
                    this.m_hasFocus = true;
                    if (this.m_pendingSelection != null)
                    {
                        break;
                    }
                    this.SetSelected((Node) null, false, true, true);
                    return;

                case EventType.Repaint:
                {
                    this.m_treeRect = GUILayoutUtility.GetLastRect();
                    if (this.m_maxWidth <= width)
                    {
                        break;
                    }
                    GUIStyle style = GUI.skin.GetStyle("horizontalscrollbar");
                    this.m_treeRect.height -= style.fixedHeight;
                    return;
                }
                default:
                    return;
            }
        }

        private void DrawNode(Node node)
        {
            bool on = object.ReferenceEquals(node, this.m_selected);
            bool foldout = this.m_expandedCache.IsExpanded(node.Data.GetHashCode(), null);
            bool expanded = foldout;
            this.m_content.text = node.Name;
            Vector2 vector = this.m_lineStyle.CalcSize(this.m_content);
            if (node.Children.Count == 0)
            {
                this.m_indent++;
                vector.x += (this.Indent + 2f) + 2f;
                this.m_indent--;
            }
            else
            {
                vector.x += ((this.Indent + 2f) + 13f) + 2f;
            }
            Rect position = GUILayoutUtility.GetRect(vector.x, (float) 18f, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if ((Event.current.type == EventType.Repaint) && on)
            {
                this.m_lineStyle.Draw(position, false, false, on, this.m_hasFocus);
            }
            if (node.Children.Count > 0)
            {
                Rect rect2 = position;
                rect2.x += this.Indent + 2f;
                Rect rect3 = rect2;
                rect3.width = 13f;
                expanded = EditorGUI.Foldout(rect3, foldout, GUIContent.none, this.m_foldoutStyle);
                if (expanded != foldout)
                {
                    this.m_expandedCache.SetExpanded(expanded, node.Data.GetHashCode(), null);
                    this.m_hasFocus = true;
                }
                if (Event.current.type == EventType.Repaint)
                {
                    rect2.x += rect3.width;
                    if (node.Enabled)
                    {
                        this.m_lineStyle.Draw(rect2, this.m_content, false, false, on, this.m_hasFocus && on);
                    }
                    else
                    {
                        this.m_disabledLineStyle.Draw(rect2, this.m_content, false, false, on, this.m_hasFocus && on);
                    }
                }
                if (expanded)
                {
                    this.m_indent++;
                    this.DrawNodes(node.Children);
                    this.m_indent--;
                }
            }
            else if (Event.current.type == EventType.Repaint)
            {
                this.m_indent++;
                Rect rect4 = position;
                rect4.x += this.Indent + 2f;
                if (node.Enabled)
                {
                    this.m_lineStyle.Draw(rect4, this.m_content, false, false, on, this.m_hasFocus && on);
                }
                else
                {
                    this.m_disabledLineStyle.Draw(rect4, this.m_content, false, false, on, this.m_hasFocus && on);
                }
                this.m_indent--;
            }
            Event current = Event.current;
            if (current.type == EventType.Repaint)
            {
                this.m_maxWidth = Mathf.Max(this.m_maxWidth, position.width);
            }
            bool flag4 = ((current.isMouse && (current.type == EventType.MouseUp)) && (current.button == 0)) && this.m_mouseWasDown;
            if ((position.Contains(current.mousePosition) && flag4) && (expanded == foldout))
            {
                this.SetSelected(node, false, true, true);
            }
        }

        private void DrawNodes(List<Node> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                this.DrawNode(nodes[i]);
            }
        }

        public Node FindNode(T data)
        {
            return Enumerable.FirstOrDefault<Node>(this.BuildFlatList(this.m_root.Children, true), delegate (Node x) {
                return x.Data.Equals(data);
            });
        }

        private void InitStyles()
        {
            if (((this.m_isProSkin != EditorGUIUtility.isProSkin) || (this.m_foldoutStyle == null)) || (this.m_lineStyle == null))
            {
                this.m_isProSkin = EditorGUIUtility.isProSkin;
                this.m_lineStyle = new GUIStyle("PR Label");
                this.SetLabelStyle(this.m_lineStyle);
                this.m_disabledLineStyle = new GUIStyle("PR DisabledLabel");
                this.SetLabelStyle(this.m_disabledLineStyle);
                this.m_foldoutStyle = new GUIStyle(EditorStyles.foldout);
                this.m_foldoutStyle.overflow.top = -2;
                this.m_foldoutStyle.active.textColor = EditorStyles.foldout.normal.textColor;
                this.m_foldoutStyle.onActive.textColor = EditorStyles.foldout.normal.textColor;
                this.m_foldoutStyle.onFocused.textColor = EditorStyles.foldout.normal.textColor;
                this.m_foldoutStyle.onFocused.background = EditorStyles.foldout.onNormal.background;
                this.m_foldoutStyle.focused.textColor = EditorStyles.foldout.normal.textColor;
                this.m_foldoutStyle.focused.background = EditorStyles.foldout.normal.background;
            }
        }

        public void OnEnable()
        {
            this.m_isProSkin = EditorGUIUtility.isProSkin;
        }

        private void OnSelectionChanged(Node prevSelection, Node newSelection)
        {
            this.m_pendingSelection = null;
            this.m_hasPendingSelection = false;
            this.m_selected = newSelection;
            if ((this.SelectionChanged != null) && this.m_triggerCallbacks)
            {
                this.SelectionChanged(prevSelection, newSelection);
            }
        }

        private void ProcessKeyboardInput()
        {
            Event current = Event.current;
            if ((current.isKey && (this.m_selected != null)) && ((current.type == EventType.KeyDown) && (GUIUtility.keyboardControl == 0)))
            {
                List<Node> list = this.BuildFlatList(this.m_root.Children, false);
                int index = list.IndexOf(this.m_selected);
                if ((index == -1) || (list.Count == 0))
                {
                    Node node = (list.Count > 0) ? list[0] : null;
                    this.SetSelected(node, false, true, true);
                }
                else
                {
                    switch (current.keyCode)
                    {
                        case KeyCode.UpArrow:
                            if (index > 0)
                            {
                                this.SetSelected(list[index - 1], false, true, true);
                            }
                            current.Use();
                            return;

                        case KeyCode.DownArrow:
                            if (index < (list.Count - 1))
                            {
                                this.SetSelected(list[index + 1], false, true, true);
                            }
                            current.Use();
                            return;

                        case KeyCode.RightArrow:
                        {
                            current.Use();
                            bool flag2 = this.m_expandedCache.IsExpanded(this.m_selected.Data.GetHashCode(), null);
                            if ((this.m_selected.Children.Count == 0) || flag2)
                            {
                                for (int i = index + 1; i < list.Count; i++)
                                {
                                    Node node3 = list[i];
                                    if (node3.Children.Count > 0)
                                    {
                                        this.SetSelected(node3, false, true, true);
                                        return;
                                    }
                                }
                            }
                            this.m_expandedCache.SetExpanded(true, this.m_selected.Data.GetHashCode(), null);
                            return;
                        }
                        case KeyCode.LeftArrow:
                        {
                            current.Use();
                            bool flag = this.m_expandedCache.IsExpanded(this.m_selected.Data.GetHashCode(), null);
                            if ((this.m_selected.Children.Count == 0) || !flag)
                            {
                                for (int j = index - 1; j >= 0; j--)
                                {
                                    Node node2 = list[j];
                                    if (node2.Children.Count > 0)
                                    {
                                        this.SetSelected(node2, false, true, true);
                                        return;
                                    }
                                }
                            }
                            this.m_expandedCache.SetExpanded(false, this.m_selected.Data.GetHashCode(), null);
                            return;
                        }
                        case KeyCode.Insert:
                            return;

                        case KeyCode.Home:
                            this.SetSelected(list[0], false, true, true);
                            current.Use();
                            return;

                        case KeyCode.End:
                            this.SetSelected(list[list.Count - 1], false, true, true);
                            current.Use();
                            return;

                        case KeyCode.PageUp:
                        {
                            int num4 = (int) (this.m_treeRect.height / 18f);
                            int num5 = Mathf.Max(index - num4, 0);
                            this.SetSelected(list[num5], false, true, true);
                            current.Use();
                            return;
                        }
                        case KeyCode.PageDown:
                        {
                            int num6 = (int) (this.m_treeRect.height / 18f);
                            int num7 = Mathf.Min((int) (index + num6), (int) (list.Count - 1));
                            this.SetSelected(list[num7], false, true, true);
                            current.Use();
                            return;
                        }
                    }
                }
            }
        }

        public void SetExpanded(T data)
        {
            this.m_expandedCache.SetExpanded(true, data.GetHashCode(), null);
        }

        private void SetLabelStyle(GUIStyle style)
        {
            style.margin.left = 0;
            style.margin.right = 0;
            style.margin.top = 0;
            style.margin.bottom = 0;
            style.padding.left = 1;
            style.padding.right = 0;
            style.padding.top = 2;
            style.padding.bottom = 0;
            style.normal.textColor = style.focused.textColor;
        }

        public void SetSelected(T data, [Optional, DefaultParameterValue(false)] bool immediate, [Optional, DefaultParameterValue(true)] bool autoScroll, [Optional, DefaultParameterValue(true)] bool triggerCallbacks)
        {
            Node node = Enumerable.FirstOrDefault<Node>(this.BuildFlatList(this.m_root.Children, true), delegate (Node x) {
                return x.Data.Equals(data);
            });
            this.SetSelected(node, immediate, autoScroll, triggerCallbacks);
        }

        public void SetSelected(Node node, [Optional, DefaultParameterValue(false)] bool immediate, [Optional, DefaultParameterValue(true)] bool autoScroll, [Optional, DefaultParameterValue(true)] bool triggerCallbacks)
        {
            this.m_pendingSelection = node;
            this.m_hasPendingSelection = true;
            this.m_triggerCallbacks = triggerCallbacks;
            if (immediate)
            {
                this.UpdatePendingSelection(autoScroll);
            }
        }

        public void Update()
        {
            this.UpdatePendingSelection(true);
        }

        private void UpdatePendingSelection([Optional, DefaultParameterValue(true)] bool autoScroll)
        {
            if (this.m_hasPendingSelection)
            {
                if (autoScroll)
                {
                    int index = this.BuildFlatList(this.m_root.Children, false).IndexOf(this.m_pendingSelection);
                    float num2 = (index * 18f) + 18f;
                    if (num2 >= (this.m_scrollPosition.y + this.m_treeRect.height))
                    {
                        this.m_scrollPosition.y = num2 - this.m_treeRect.height;
                    }
                    else if ((index * 18f) < this.m_scrollPosition.y)
                    {
                        this.m_scrollPosition.y = index * 18f;
                    }
                }
                this.OnSelectionChanged(this.m_selected, this.m_pendingSelection);
            }
        }

        private float Indent
        {
            get
            {
                return (this.m_indent * 13f);
            }
        }

        public class Node
        {
            [CompilerGenerated]
            private List<rdtGuiTree<T>.Node> _children;
            [CompilerGenerated]
            private T _data;
            [CompilerGenerated]
            private bool _bEnabled;
            [CompilerGenerated]
            private rdtGuiTree<T>.Node _parent;

            public Node()
            {
                this.Enabled = true;
                this.Children = new List<rdtGuiTree<T>.Node>();
            }

            public Node(T data, rdtGuiTree<T>.Node parent)
            {
                this.Enabled = true;
                this.Parent = parent;
                this.Data = data;
                this.Children = new List<rdtGuiTree<T>.Node>();
            }

            public rdtGuiTree<T>.Node AddNode(T data, [Optional, DefaultParameterValue(true)] bool enabled)
            {
                rdtGuiTree<T>.Node item = new rdtGuiTree<T>.Node(data, (rdtGuiTree<T>.Node) this);
                item.Enabled = enabled;
                this.Children.Add(item);
                return item;
            }

            public List<rdtGuiTree<T>.Node> Children
            {
                [CompilerGenerated]
                get
                {
                    return _children;
                }
                [CompilerGenerated]
                private set
                {
                    _children = value;
                }
            }

            public T Data
            {
                [CompilerGenerated]
                get
                {
                    return _data;
                }
                [CompilerGenerated]
                private set
                {
                    _data = value;
                }
            }

            public bool Enabled
            {
                [CompilerGenerated]
                get
                {
                    return _bEnabled;
                }
                [CompilerGenerated]
                set
                {
                    _bEnabled = value;
                }
            }

            public string Name
            {
                get
                {
                    return this.Data.ToString();
                }
            }

            public rdtGuiTree<T>.Node Parent
            {
                [CompilerGenerated]
                get
                {
                    return _parent;
                }
                [CompilerGenerated]
                private set
                {
                    _parent = value;
                }
            }
        }
    }
}

