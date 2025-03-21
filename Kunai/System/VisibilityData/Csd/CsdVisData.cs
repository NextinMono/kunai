﻿using SharpNeedle.Framework.Ninja.Csd;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kunai
{
    public partial class CsdVisData
    {
        public List<Node> Nodes = new List<Node>();

        public CsdVisData(CsdProject in_Proj)
        {
            Nodes.Add(new Node(new KeyValuePair<string, SceneNode>(in_Proj.Name, in_Proj.Project.Root)));
        }
        private Node RecursiveGetNode(Node in_Node, SceneNode in_SceneNode)
        {
            if (in_Node.Value.Value == in_SceneNode)
                return in_Node;
            foreach (var node in in_Node.Children)
            {
                if (node.Value.Value == in_SceneNode)
                    return node;
            }
            return null;
        }
        private Scene RecursiveGetScene(Node in_Node, SharpNeedle.Framework.Ninja.Csd.Scene in_Scene)
        {
            foreach (var s in in_Node.Scene)
            {
                if (s.Value.Value == in_Scene)
                    return s;
            }
            foreach (var node in in_Node.Children)
            {
                var scene = RecursiveGetScene(node, in_Scene);
                if (scene != null)
                    return scene;
            }
            return null;
        }
        public Node GetVisibility(SceneNode in_Scene)
        {
            foreach (var node in Nodes)
            {
                return RecursiveGetNode(node, in_Scene);
            }
            return null;
        }
        public Scene GetScene(SharpNeedle.Framework.Ninja.Csd.Scene in_Scene)
        {
            foreach (var node in Nodes)
            {
                return RecursiveGetScene(node, in_Scene);
            }
            return null;
        }
    }
}
