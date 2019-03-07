using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;
using System.IO;

namespace LearnEnglish
{
    public partial class frmMain : Form
    {
        private StreamWords sw = null;
        private string path = null;
        private Word Current;
        private List<Word> queue;
        private int PointST = 0;
        private const string Filter = "Learn English|*.learn";

        public frmMain()
        {
            InitializeComponent();
        }

        
        private void frmMain_Load(object sender, EventArgs e)
        {
            sw = new StreamWords();
            Current.Eng = "\0";
            queue = new List<Word>();
        }

        private void listWords_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void listWords_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var gird = sender as DataGridView;
            var rowIndex = (e.RowIndex + 1).ToString();
            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, gird.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIndex, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var current = listWords.CurrentRow;
            if (current != null && current.Index != listWords.NewRowIndex)
                listWords.Rows.Remove(current);
        }

        private void FillToList()
        {
            listWords.Rows.Clear();
            foreach (Word w in sw.listWords)
                listWords.Rows.Add(w.Eng, w.Viet);
        }

        private void OpenFile()
        {
            if (sw != null)
            {
                OpenFileDialog o = new OpenFileDialog()
                {
                    Filter = Filter,
                    Multiselect = false
                };
                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    sw.LoadData(o.FileName);
                    FillToList();
                }
            }
        }

        private void SaveFile()
        {
            if (sw != null)
            {
                SaveFileDialog s = new SaveFileDialog()
                {
                    Filter = Filter,
                    FileName = sw.Path
                };
                if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    sw.listWords.Clear();
                    for (int i = 0; i < listWords.Rows.Count; ++i)
                    {
                        object eng = listWords.Rows[i].Cells[0].Value;
                        object viet = listWords.Rows[i].Cells[1].Value;
                        if (eng == null || viet == null) continue;
                        sw.listWords.Add(new Word(eng.ToString(), viet.ToString()));
                    }
                    sw.WriteData(s.FileName);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (sw == null) return;
            if (sw.Saved == false)
            {
                switch (MessageBox.Show("Bạn có muốn lưu lại không?", "Warn", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        SaveFile();
                        if (sw.Saved == false) return;
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        OpenFile();
                        return;
                    case System.Windows.Forms.DialogResult.Cancel:
                        return;
                }
            }
            if (sw.Saved == true) OpenFile();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (sw == null) return;
            SaveFile();
        }

        private void listWords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (sw == null) return;
            sw.Saved = false;
        }

        private Word GetNextWord()
        {
            if (queue.Count == 0)
            {

                return Current = new Word("\0", "");
            }
            Current = queue[0];
            queue.RemoveAt(0);
            lbWord.Text = Current.Viet;
            return Current;
        }

        private void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (Current.Eng == "\0")
                {
                    MessageBox.Show("Hết từ vựng!");
                    return;
                }
                if (tbInput.Text.ToUpper() == Current.Eng.ToUpper())
                {
                    lbPoint.Text = (++PointST).ToString();
                    Current = GetNextWord();
                }
                else
                {
                    MessageBox.Show(Current.Eng);
                    lbPoint.Text = (--PointST).ToString();
                }
                tbInput.Text = "";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            queue.Clear();
            {
                lbPoint.Text = (PointST = 0).ToString();
                ///New
                Random r = new Random();
                for (int i = 0; i < num.Value; ++i)
                {
                    foreach (Word w in sw.listWords)
                    {
                        Word tmp = w;
                        tmp.id = r.Next();
                        queue.Add(tmp);
                    }
                }
                for (int i = 0; i < queue.Count - 1; ++i)
                    for (int j = i + 1; j < queue.Count; ++j)
                        if (queue[i].id > queue[j].id) {
                            Word tmp = queue[i];
                            queue[i] = queue[j];
                            queue[j] = tmp;
                        }
                Current = GetNextWord();
            }
            tbInput.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("No information!");
        }
    }

    public class StreamWords
    {
        public List<Word> listWords;
        public bool Saved;
        public string Path;

        public StreamWords()
        {
            listWords = new List<Word>();
            Saved = true;
            Path = "";
        }

        public void ResetData()
        {
            listWords = new List<Word>();
            Saved = true;
            Path = "";
        }

        public void LoadData(string path)
        {
            Path = path;
            Saved = true;
            ResetData();
            BinaryReader br = new BinaryReader(new FileStream(path, FileMode.OpenOrCreate));
            int n = br.ReadInt32();
            for (int i = 0; i < n; ++i)
            {
                string en, vi;
                en = br.ReadString();
                vi = br.ReadString();
                listWords.Add(new Word(en, vi));
            }
            br.Close();
        }

        public void Add(string en, string vi)
        {
            Saved = false;
            listWords.Add(new Word(en, vi));
        }

        public void WriteData(string path)
        {
            Path = path;
            Saved = true;
            BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
            bw.Write(listWords.Count);
            foreach (Word w in listWords)
            {
                bw.Write(w.Eng);
                bw.Write(w.Viet);
            }
            bw.Close();
        }
    }

    public struct Word
    {
        public string Eng;
        public string Viet;
        public int id;
        public Word(string _eng, string _viet)
        {
            id = 0;
            Eng = _eng;
            Viet = _viet;
        }
    }
}
