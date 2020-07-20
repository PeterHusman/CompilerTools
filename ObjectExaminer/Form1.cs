using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ObjectExaminer
{
    public partial class ObjectExaminerForm : Form
    {
        public ObjectExaminerForm()
        {
            InitializeComponent();
        }

        public void LoadObject(object root)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.Nodes.Add(MakeLazyNode(root));
            treeView.EndUpdate();
        }

        private TreeNode AddNode(object node, string prefix = null)
        {
            TreeNode treeNode = new TreeNode();

            if (prefix != null)
            {
                treeNode.Text = $"{prefix}: {node}";
            }
            else
            {
                treeNode.Text = node.ToString();
            }


            if (node is float || node is int || node is bool || node is string)
            {
                return treeNode;
            }

            //if(node is Array arr)
            //{
            //    foreach(object child in arr)
            //    {
            //        treeNode.Nodes.Add(AddNode(child));
            //    }
            //}

            if (node is IEnumerable ienum)
            {
                foreach (object child in ienum)
                {
                    treeNode.Nodes.Add(AddNode(child));
                }
            }


            foreach (PropertyInfo child in node.GetType().GetProperties())
            {
                treeNode.Nodes.Add(AddNode(child.GetValue(node), child.Name));
            }

            foreach (FieldInfo child in node.GetType().GetFields())
            {
                treeNode.Nodes.Add(AddNode(child.GetValue(node), child.Name));
            }

            return treeNode;
        }

        private void ObjectExaminerForm_Load(object sender, EventArgs e)
        {

        }

        private TreeNode MakeLazyNode(object obj, string prefix = null)
        {
            TreeNode treeNode = new TreeNode("");

            if (prefix != null)
            {
                treeNode.Text = $"{prefix}: {obj}";
            }
            else
            {
                treeNode.Text = obj.ToString();
            }


            if (obj is float || obj is int || obj is bool || obj is string || obj is long)
            {
                return treeNode;
            }

            treeNode.Nodes.Add(new TreeNode(""));

            treeNode.Tag = obj;

            return treeNode;
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count != 1 || e.Node.Tag == null || e.Node.Nodes[0].Text != "")
            {
                return;
            }

            treeView.BeginUpdate();

            object obj = e.Node.Tag;

            e.Node.Nodes.Clear();

            if (obj is IEnumerable ienum)
            {
                foreach (object child in ienum)
                {
                    e.Node.Nodes.Add(MakeLazyNode(child));
                }
            }


            foreach (PropertyInfo child in obj.GetType().GetProperties())
            {
                e.Node.Nodes.Add(MakeLazyNode(child.GetValue(obj), child.Name));
            }

            foreach (FieldInfo child in obj.GetType().GetFields())
            {
                e.Node.Nodes.Add(MakeLazyNode(child.GetValue(obj), child.Name));
            }

            treeView.EndUpdate();
        }
    }
}
