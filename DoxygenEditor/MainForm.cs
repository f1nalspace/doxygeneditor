using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoxygenEditor
{
    public partial class MainForm : Form
    {
        private readonly DoxygenParser _parser = new DoxygenParser();
        private DoxygenParser.Context _context = null;

        private readonly ScintillaNET.Scintilla _editor;
        private readonly DoxygenLexer _lexer;
        private readonly Timer _editorChangeTimer;
        private readonly BackgroundWorker _parseWorker;


        public MainForm()
        {
            InitializeComponent();
            _editor = new ScintillaNET.Scintilla();
            splitContainer1.Panel2.Controls.Add(_editor);
            _editor.Dock = DockStyle.Fill;
            _editor.WrapMode = ScintillaNET.WrapMode.None;
            _editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            _editor.TextChanged += _editor_TextChanged;
            _editor.UpdateUI += _editor_UpdateUI;
            _editor.Insert += _editor_Insert;
            _editor.Delete += _editor_Delete;
            _editor.StyleNeeded += _editor_StyleNeeded;
            _lexer = new DoxygenLexer(_editor);
            _editorChangeTimer = new Timer();
            _editorChangeTimer.Interval = 1000;
            _editorChangeTimer.Tick += _editorChangeTimer_Tick;
            _editorChangeTimer.Enabled = false;
            _parseWorker = new BackgroundWorker();
            _parseWorker.RunWorkerCompleted += _parseWorker_RunWorkerCompleted;
            _parseWorker.DoWork += _parseWorker_DoWork;
        }

        private void _parseWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string text = (string)e.Argument;
            _context = _parser.Parse(text);
        }

        private void _parseWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Assert(_context != null);
            UpdateTree(_context);
        }

        private void _editor_StyleNeeded(object sender, ScintillaNET.StyleNeededEventArgs e)
        {
            int startPos = _editor.GetEndStyled();
            int endPos = e.Position;
            _lexer.Style(startPos, endPos);
        }

        private void _editor_Delete(object sender, ScintillaNET.ModificationEventArgs e)
        {
        }

        private void _editor_Insert(object sender, ScintillaNET.ModificationEventArgs e)
        {
        }

        private int _lastCaretPos = -1;
        private void _editor_UpdateUI(object sender, ScintillaNET.UpdateUIEventArgs e)
        {
            var caretPos = _editor.CurrentPosition;
            if (caretPos != _lastCaretPos)
            {
                _lastCaretPos = caretPos;
            }
        }

        private void _editorChangeTimer_Tick(object sender, EventArgs e)
        {
            _editorChangeTimer.Stop();
            if (!_parseWorker.IsBusy)
            {
                _parseWorker.RunWorkerAsync(_editor.Text);
            }
            else
                _editorChangeTimer.Start();
        }

        private void _editor_TextChanged(object sender, EventArgs e)
        {
            _editorChangeTimer.Stop();
            _editorChangeTimer.Start();
        }

        private void Clear()
        {
            _editor.ClearAll();
            tvTree.Nodes.Clear();
        }

        private void UpdateTree(DoxygenParser.Context context)
        {
            tvTree.BeginUpdate();
            tvTree.Nodes.Clear();

            // Pages
            foreach (DoxygenParser.PageEntity pageEntity in context.RootEntity.Children.Where(p => typeof(DoxygenParser.PageEntity).IsInstanceOfType(p)))
            {
                string pageCaption = !string.IsNullOrEmpty(pageEntity.PageCaption) ? pageEntity.PageCaption : pageEntity.PageId;
                TreeNode pageNode = tvTree.Nodes.Add(pageEntity.PageId, pageCaption);
                pageNode.Tag = pageEntity;

                // Sections
                foreach (DoxygenParser.SectionEntity sectionEntity in pageEntity.Children.Where(p => typeof(DoxygenParser.SectionEntity).Equals(p.GetType())))
                {
                    string sectionCaption = !string.IsNullOrEmpty(sectionEntity.SectionCaption) ? sectionEntity.SectionCaption : sectionEntity.SectionId;
                    TreeNode sectionNode = pageNode.Nodes.Add(sectionEntity.SectionId, sectionCaption);
                    sectionNode.Tag = sectionEntity;

                    // Subsections
                    foreach (DoxygenParser.SubSectionEntity subSectionEntity in sectionEntity.Children.Where(p => typeof(DoxygenParser.SubSectionEntity).Equals(p.GetType())))
                    {
                        string subSectionCaption = !string.IsNullOrEmpty(subSectionEntity.SectionCaption) ? subSectionEntity.SectionCaption : subSectionEntity.SectionId;
                        TreeNode subSectionNode = sectionNode.Nodes.Add(subSectionEntity.SectionId, subSectionCaption);
                        subSectionNode.Tag = subSectionEntity;
                    }
                }
            }
            tvTree.EndUpdate();
        }

        private void LoadFile(string filePath)
        {
            _editor.TextChanged -= _editor_TextChanged;
            _editor.Text = File.ReadAllText(filePath);
            _editor.TextChanged += _editor_TextChanged;
            Debug.Assert(!_parseWorker.IsBusy);
            _parseWorker.RunWorkerAsync(_editor.Text);
        }

        private void fileOpenMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadFile(openFileDialog1.FileName);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var editorFont = new Font(FontFamily.GenericMonospace, 14.0f, FontStyle.Regular);

            _editor.StyleResetDefault();
            _editor.Styles[ScintillaNET.Style.Default].Font = editorFont.Name;
            _editor.Styles[ScintillaNET.Style.Default].Size = (int)editorFont.SizeInPoints;
            _editor.StyleClearAll();

            _lexer.InitStyles();

            _editor.Lexer = ScintillaNET.Lexer.Container;

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                LoadFile(args[1]);
        }

        private void tvTree_DoubleClick(object sender, EventArgs e)
        {
            if (tvTree.SelectedNode != null)
            {
                TreeNode treeNode = tvTree.SelectedNode;
                DoxygenParser.Entity entity = (DoxygenParser.Entity)treeNode.Tag;
                int lineIndex = entity.LineInfo.LineIndex;
                // @TODO(final): Navigate to line
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _editorChangeTimer.Stop();
            if (_parseWorker.IsBusy)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
