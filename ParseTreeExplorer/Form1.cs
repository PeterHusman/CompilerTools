using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParseTreeExplorer
{
    public partial class ParseTreeExplorer : Form
    {
        public ParseTreeExplorer()
        {
            InitializeComponent();
        }

        public void LoadParseTree(NonterminalNode<ThingType> root)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.Nodes.Add(AddNode(root));
            treeView.EndUpdate();
        }

        private TreeNode AddNode(Node node)
        {
            TreeNode treeNode = new TreeNode();
            if(node is Terminal<ThingType> term)
            {
                treeNode.Text = $"{term.TokenType}: {term.TokenValue}";
                return treeNode;
            }

            NonterminalNode<ThingType> nonterm = node as NonterminalNode<ThingType>;

            treeNode.Text = nonterm.Name;

            foreach(Node child in nonterm.Children)
            {
                treeNode.Nodes.Add(AddNode(child));
            }

            return treeNode;
        }

        private void ParseTreeExplorer_Load(object sender, EventArgs e)
        {
            
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
