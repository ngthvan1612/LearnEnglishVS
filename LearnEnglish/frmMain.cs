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
        private StreamWords stream = null;
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
            var row = listWords.CurrentRow;
            if (row == null) return;
            if (row.IsNewRow == true) return;
            listWords.Rows.Remove(row);
            stream.Saved = false;
        }

        private void FillToList()
        {
            if (stream == null)
            {
                MessageBox.Show("Error [FILL-TO-LIST]");
                return;
            }
            listWords.Rows.Clear();
            List<Word> lw = stream.Get();
            foreach (Word w in lw)
                listWords.Rows.Add(w.Eng, w.Viet);
        }

        private void FillToStream()
        {
            if (stream == null) return;
            stream.ResetWords();
            foreach (DataGridViewRow row in listWords.Rows)
            {
                object en = row.Cells[0].Value;
                object vi = row.Cells[1].Value;
                if (en == null || vi == null) continue;
                stream.Add(en.ToString(), vi.ToString());
            }
        }

        private bool OpenFileWithoutSave()
        {
            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = Filter,
                Multiselect = false
            };
            if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                stream = new StreamWords();
                GC.Collect();
                stream.LoadData(o.FileName);
                FillToList();
                return true;
            }
            return false;
        }

        private bool SaveFile()
        {
            if (stream == null) return true;
            if (stream.Path != "")
            {
                FillToStream();
                stream.WriteData(stream.Path);
                return true;
            }
            SaveFileDialog s = new SaveFileDialog()
            {
                Filter = Filter
            };
            if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                stream.WriteData(s.FileName);
                return true;
            }
            return false;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (stream == null) OpenFileWithoutSave();
            else
            {
                if (stream.Saved == false)
                {
                    var result = MessageBox.Show("Bạn có muốn lưu lại không?", "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    switch (result)
                    {
                        case System.Windows.Forms.DialogResult.Yes:
                            if (SaveFile() == false) return;
                            break;
                        case System.Windows.Forms.DialogResult.No:
                            ///No thing
                            break;
                        case System.Windows.Forms.DialogResult.Cancel:
                            return;
                    }                    
                }
                OpenFileWithoutSave();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (stream == null) stream = new StreamWords();
            FillToStream();
            if (stream.Path != "") stream.WriteData(stream.Path);
            else SaveFile();
        }

        private void listWords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (stream == null) return;
            stream.Saved = false;
        }

        private void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (queue == null) return;
                if (Current.Eng == "\0")
                {
                    MessageBox.Show("Đã hết từ vựng!");
                    return;
                }
                if (StreamWords.Repair(tbInput.Text).ToUpper() == StreamWords.Repair(Current.Eng).ToUpper())
                {
                    lbPoint.Text = "Point: " + (++PointST).ToString();
                    tbInput.Text = "";
                    Current = GetNextWord();
                }
                else
                {
                    MessageBox.Show(Current.Eng);
                    lbPoint.Text = "Point: " + (--PointST).ToString();
                }
                tbInput.Text = "";
            }
        }

        private Word GetNextWord()
        {
            if (queue.Count == 0)
            {
                lbWord.Text = "Đã hết từ vựng!";
                return new Word("\0", "");
            }
            Word tmp = queue[0];
            lbWord.Text = tmp.Viet;
            queue.RemoveAt(0);
            tbInput.Focus();
            return tmp;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (stream == null) return;
            if (queue == null) queue = new List<Word>();
            if (queue.Count != 0)
            {
                var result = MessageBox.Show("Đang học, bạn có muốn bắt đầu lại?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.No) return;
            }
            PointST = 0;
            queue.Clear();
            Random r = new Random();
            List<Word> lw = stream.Get();
            for (int i = 0; i < num.Value; ++i)
            {
                foreach (Word w in lw.ToList())
                {
                    Word tmp = w;
                    tmp.id = r.Next();
                    queue.Add(tmp);
                }
            }
            for (int i = 0; i < queue.Count - 1; ++i)
                for (int j = i + 1; j < queue.Count; ++j)
                    if (queue[i].id > queue[j].id)
                    {
                        Word tmp = queue[i];
                        queue[i] = queue[j];
                        queue[j] = tmp;
                    }
            Current = GetNextWord();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("No information!");
        }

        private void listWords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            e.CellStyle = listWords.DefaultCellStyle;
        }

        private void lbPoint_TextChanged(object sender, EventArgs e)
        {
            if (PointST < 0) lbPoint.ForeColor = Color.Red;
            else lbPoint.ForeColor = Color.Blue;
        }
    }

    public class StreamWords
    {
        private List<Word> listWords;
        public bool Saved;
        public string Path;

        public StreamWords()
        {
            listWords = new List<Word>();
            Saved = true;
            Path = "";
        }

        public void ResetWords()
        {
            listWords = new List<Word>();
        }

        public void LoadData(string path)
        {
            Path = path;
            Saved = true;
            ResetWords();
            BinaryReader br = new BinaryReader(new FileStream(path, FileMode.OpenOrCreate));
            int n = br.ReadInt32();
            for (int i = 0; i < n; ++i)
            {
                string en, vi;
                en = Enco(br.ReadString());
                vi = Enco(br.ReadString());
                listWords.Add(new Word(en, vi));
            }
            br.Close();
        }

        public void Add(string en, string vi)
        {
            Saved = false;
            listWords.Add(new Word(en, vi));
        }

        public List<Word> Get()
        {
            return listWords;
        }

        public static string Repair(string p)
        {
            string tmp = "";
            string res = "";
            p += " ";
            for (int i = 0; i < (int)p.Length; ++i)
                if (p[i] == ' ')
                {
                    if (tmp == "") continue;
                    res = res + tmp + " ";
                    tmp = "";
                }
                else tmp += p[i];
            if (res.Length > 0 && res[res.Length - 1] == ' ')
                res = res.Remove(res.Length - 1);
            return res;
        }

        private string Enco(string p)
        {
            string res = "";
            foreach (char c in p)
                res = res + (char)((int)c ^ 161);
            res = Repair(res);
            return res;
        }

        public void WriteData(string path)
        {
            Path = path;
            Saved = true;
            BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
            bw.Write(listWords.Count);
            foreach (Word w in listWords)
            {
                bw.Write(Enco(w.Eng));
                bw.Write(Enco(w.Viet));
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
