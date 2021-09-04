using Origine.BT;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

namespace Origine
{
    public class BehaviorTreeEditor : NodeEditor
    {
        public static BehaviorTreeEditor Instance;
        public static Func<BehaviorTree> TreeGetter { get; set; }

        private Debugger debugger;
        private BehaviorTree currentTree;

        public BehaviorTreeEditor()
        {
        }

        public void SetBehaviorTree(BehaviorTree behaviorTree)
        {
            debugger = new Debugger();
            currentTree = behaviorTree;
            CreateNode(debugger, behaviorTree.StartNode);
            CenterView();
        }

        private NodeDesigner CreateNode(Debugger designer, BaseNode node)
        {
            NodeDesigner nodeDesigner = new NodeDesigner();
            nodeDesigner.baseNode = node;
            nodeDesigner.NodeData = node.NodeData;
            nodeDesigner.Rect = new Rect(nodeDesigner.NodeData.X, nodeDesigner.NodeData.Y, BehaviorTreeEditorStyles.StateWidth, BehaviorTreeEditorStyles.StateHeight);
            designer.Nodes.Add(nodeDesigner);

            if (node is CompositeNode)
            {
                CompositeNode compositeNode = node as CompositeNode;
                for (int i = 0; i < compositeNode.Childs.Count; i++)
                {
                    BaseNode childNode = compositeNode.Childs[i];
                    NodeDesigner childNodeDesigner = CreateNode(designer, childNode);
                    Transition transition = new Transition();
                    transition.FromNode = nodeDesigner;
                    transition.ToNode = childNodeDesigner;
                    nodeDesigner.Transitions.Add(transition);
                }
            }

            return nodeDesigner;
        }

        public void SetTransition(NodeDesigner node, NodeData nodeData)
        {
            if (nodeData.Childs != null && nodeData.Childs.Count > 0)
            {
                node.Transitions = new List<Transition>(nodeData.Childs.Count);

                for (int i = 0; i < nodeData.Childs.Count; i++)
                {
                    NodeData tempData = nodeData.Childs[i];
                    Transition transition = new Transition();
                    transition.Set(FindById(tempData.ID), node);
                    node.Transitions.Add(transition);
                }
            }
        }

        public NodeDesigner FindById(Guid id)
        {
            if (debugger == null)
                return null;

            for (int i = 0; i < debugger.Nodes.Count; i++)
            {
                NodeDesigner nodeDesigner = debugger.Nodes[i];
                if (nodeDesigner != null && nodeDesigner.ID == id)
                    return nodeDesigner;
            }

            return null;
        }

        private bool centerView;
        private NodeDesigner fromNode;
        private MainToolbar mainToolbar;

        [MenuItem("Tools/BTDebugger")]
        public static BehaviorTreeEditor ShowWindow()
        {
            BehaviorTreeEditor window = EditorWindow.GetWindow<BehaviorTreeEditor>("BTDebugger");
            return window;
        }

        private void OnDestroy()
        {
            Selection.activeObject = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Instance = this;
            if (mainToolbar == null)
                mainToolbar = new MainToolbar();
            mainToolbar.OnEnable();

            centerView = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
        }

        private void Update()
        {
            if (Application.isPlaying && TreeGetter != null)
            {
                var tree = TreeGetter?.Invoke();
                if (tree != null) SetBehaviorTree(tree);
            }

            if (currentTree != null && currentTree.Status != NodeStatus.Running)
                debugger = null;

            Repaint();
        }

        protected override void OnGUI()
        {
            GetCanvasSize();

            mainToolbar.OnGUI();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            ZoomableArea.Begin(new Rect(0, 0f, scaledCanvasSize.width, scaledCanvasSize.height + 21), scale, IsDocked);
            Begin();

            if (debugger != null)
            {
                DoNodes();
            }
            else
            {
                ZoomableArea.End();
            }
            End();

            if (centerView)
            {
                CenterView();
                centerView = false;
            }
            //GUI.Label(new Rect(5, 20, 300, 200), "Right click to create a node.", BehaviorTreeEditorStyles.instructionLabel);
            Event ev = Event.current;
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        protected override Rect GetCanvasSize()
        {
            return new Rect(0, 17, position.width, position.height);
        }

        private void DoNodes()
        {
            DoTransitions();
            DoChildIndex();

            if (debugger.Nodes.Count > 0)
            {
                for (int i = 0; i < debugger.Nodes.Count; i++)
                {
                    NodeDesigner node = debugger.Nodes[i];
                    DoNode(node, false);
                }
            }

            ZoomableArea.End();
            NodeContextMenu();
        }

        private void DoNode(NodeDesigner node, bool on)
        {
            GUIStyle style = BehaviorTreeEditorStyles.GetNodeStyle(node);
            GUI.Box(node.Rect, node.NodeData.ClassType + ":" + node.NodeData.Label, style);
        }

        private void DoTransitions()
        {
            if (debugger == null)
                return;

            if (fromNode != null)
            {
                DrawConnection(fromNode.Rect.center, mousePosition, Color.green, 1, false);
                Repaint();
            }

            for (int i = 0; i < debugger.Nodes.Count; i++)
            {
                NodeDesigner node = debugger.Nodes[i];
                for (int j = 0; j < node.Transitions.Count; j++)
                {
                    Transition transition = node.Transitions[j];
                    DoTransition(transition);
                }
            }
        }

        private void DoTransition(Transition trnansition)
        {
            NodeDesigner toNode = trnansition.ToNode;
            NodeDesigner fromNode = trnansition.FromNode;
            if (toNode != null && fromNode != null)
            {
                Color color = BehaviorTreeEditorStyles.GetTransition(toNode);
                DrawConnection(fromNode.Rect.center, toNode.Rect.center, color, 1, false);
            }
        }

        private void DoChildIndex()
        {
            if (debugger == null)
                return;

            for (int i = 0; i < debugger.Nodes.Count; i++)
            {
                NodeDesigner node = debugger.Nodes[i];

                if (node.Transitions.Count > 1)
                {
                    for (int j = 0; j < node.Transitions.Count; j++)
                    {
                        Transition transition = node.Transitions[j];
                        Vector3 start = transition.FromNode.Rect.center;
                        Vector3 end = transition.ToNode.Rect.center;

                        Vector3 vector3 = (end + start) * 0.5f;
                        GUI.Label(new Rect(vector3.x, vector3.y, 0, 0), j.ToString(), BehaviorTreeEditorStyles.instructionLabel);
                    }
                }

            }
        }

        protected override void CanvasContextMenu()
        {
            if (currentEvent.type != EventType.MouseDown
                || currentEvent.button != 1
                || currentEvent.clickCount != 1)
                return;

            if (debugger == null)
                return;

            GenericMenu canvasMenu = new GenericMenu();
            canvasMenu.ShowAsContext();
        }

        private void NodeContextMenu()
        {
            if (currentEvent.type != EventType.MouseDown || currentEvent.button != 1 || currentEvent.clickCount != 1)
                return;

            NodeDesigner node = MouseOverNode();
            if (node == null)
                return;
            GenericMenu nodeMenu = new GenericMenu();

            nodeMenu.AddItem(FsmContent.makeTransition, false, () => fromNode = node);

            nodeMenu.AddItem(FsmContent.deleteStr, false, () => { });

            nodeMenu.AddSeparator("");

            nodeMenu.ShowAsContext();
            Event.current.Use();
        }

        private NodeDesigner MouseOverNode()
        {
            for (int i = 0; i < debugger.Nodes.Count; i++)
            {
                NodeDesigner node = debugger.Nodes[i];
                if (node.Rect.Contains(mousePosition))
                {
                    return node;
                }
            }
            return null;
        }

        private void UpdateUnitySelection()
        {
            //Selection.objects = selection1.ToArray();
        }

        public void Fresh()
        {
        }

        public void CenterView()
        {
            if (debugger == null)
                return;

            Vector3 center = Vector3.zero;
            if (debugger.Nodes.Count > 0)
            {
                for (int i = 0; i < debugger.Nodes.Count; i++)
                {
                    NodeDesigner node = debugger.Nodes[i];
                    center += new Vector3(node.Rect.center.x - scaledCanvasSize.width * 0.5f, node.Rect.center.y - scaledCanvasSize.height * 0.5f);
                }
                center /= debugger.Nodes.Count;
            }
            else
            {
                center = NodeEditor.Center;
            }
            UpdateScrollPosition(center);
            Repaint();
        }

        public bool Save()
        {
            return false;
        }

        public void ShowNotification(string showStr)
        {
            ShowNotification(new GUIContent(showStr));
        }

        public static void RepaintAll()
        {
            if (Instance != null)
            {
                Instance.Repaint();
            }
        }

        public bool IsDocked
        {
            get
            {
                BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
                return (bool)isDockedMethod.Invoke(this, null);
            }
        }
    }
}