using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace SourceMatSwapper
{
    public struct Mat
    {
        public string name;
        public string[] texes;
    }
    public partial class Form1: Form
    {
        string settingsFileName = "settings.xml";
        public List<Mat> MAT = new List<Mat>();
        public string vertexString = "\"VertexLitGeneric\"";
        public string lightmappedString = "\"LightMappedGeneric\"";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            loadCollections();
            refreshListBox();
        }
        void loadCollections()
        {
            if (File.Exists(settingsFileName))
            {
                string[] Mats = File.ReadAllLines(settingsFileName);
                int numTex = Mats.Length;
                if (numTex != 0)
                {
                    Mat mat;
                    string[] tmpCollection;
                    for (int i = 0; i < numTex; i++)
                    {
                        if (!Mats[i].Contains('*')) throw new Exception("Formatting wrong, couldnt find * in " + Mats[i]);
                        tmpCollection = Mats[i].Split('*');
                        mat = new Mat();
                        mat.name = tmpCollection[0];
                        int numTexRefs = tmpCollection.Length - 1;
                        mat.texes = new string[numTexRefs];
                        for (int j = 0; j < numTexRefs; j++)
                        {
                            mat.texes[j] = tmpCollection[j + 1];
                        }
                        MAT.Add(mat);
                    }
                }
            }
            else
            {
                File.Create(settingsFileName);
                loadCollections();
            }
        }
        void refreshListBox()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < MAT.Count(); i++)
            {
                listBox1.Items.Add(MAT[i].name);
            }
        }
        Mat getMatByName(string name)
        {
            for (int i = 0; i < MAT.Count(); i++)
            {
                if (MAT[i].name == name)
                {
                    return MAT[i];
                }
            }
            throw new Exception("Mat name not found for '" + name + "'");
        }
        void removeMatByName(string name)
        {
            Mat matToRemove = getMatByName(name);
            MAT.Remove(matToRemove);
        }
        void saveCollections()
        {
            string saveBuffer = "";
            for (int i = 0; i < MAT.Count(); i++)
            {
                if (i != 0) saveBuffer += '\n';
                saveBuffer += MAT[i].name;
                for (int j = 0; j < MAT[i].texes.Length; j++)
                {
                    saveBuffer += "*" + MAT[i].texes[j];
                }
            }
            File.WriteAllText(settingsFileName, saveBuffer);
        }
        bool checkIfOn(string dir)
        {
            if (File.Exists(dir))
            {
                string[] contents = File.ReadAllLines(dir);
                for (int i = 0; i < contents.Length; i++)
                {
                    if (firstLineHas(contents[i], vertexString)) //ON
                    {
                        return true;
                    }
                    else if (firstLineHas(contents[i], lightmappedString)) //OFF
                    {
                        return false;
                    }
                }
            }
            else
            {
                throw new Exception("File does not exist " + Path.GetFileName(dir));
            }
            throw new Exception("Something wrong with formatting " + Path.GetFileName(dir));
        }
        bool toggle(string dir)
        {
            if (File.Exists(dir))
            {
                string[] contents = File.ReadAllLines(dir);
                for (int i = 0; i < contents.Length; i++)
                {
                    if (firstLineHas(contents[i], vertexString)) //turn off
                    {
                        contents[i] = lightmappedString;
                        File.WriteAllLines(dir, contents);
                        return false;
                    }
                    else if (firstLineHas(contents[i], lightmappedString)) //turn on
                    {
                        contents[i] = vertexString;
                        File.WriteAllLines(dir, contents);
                        return true;
                    }
                }
                MessageBox.Show("Something weird with texture " + Path.GetFileName(dir));
            }
            else
            {
                MessageBox.Show(Path.GetFileName(dir) + " Does not exist to toggle");
                return false;
            }
            throw new Exception("Toggle failed");
        }
        bool firstLineHas(string a, string b)
        {
            if (a.Substring(0, b.Length) == b) return true;
            else return false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Mat matAtIndex = getMatByName(listBox1.SelectedItem.ToString());
            richTextBox1.Text = matAtIndex.name;
            for (int i = 0; i < matAtIndex.texes.Length; i++)
            {
                richTextBox1.Text += "\n" + matAtIndex.texes[i];
            }
            if (checkIfOn(matAtIndex.texes[0]))
            {
                label2.ForeColor = Color.Green;
                label2.Text = "ON";
            }
            else
            {
                label2.ForeColor = Color.Red;
                label2.Text = "OFF";
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            for (int i = 0; i < files.Length; i++)
            {
                if (i != 0) richTextBox1.Text += "\n";
                richTextBox1.Text += files[i];
            }
        }
        void selectLastItem()
        {
            if (listBox1.Items.Count > 0) listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }
        private void button2_Click(object sender, EventArgs e) //Remove
        {
            removeMatByName(listBox1.SelectedItem.ToString());
            refreshListBox();
            saveCollections();
            selectLastItem();
        }
        Mat getTextBoxMatInfo()
        {
            Mat mat = new Mat();
            string[] buffer = richTextBox1.Text.Split('\n');
            mat.name = buffer[0];
            int numTexes = buffer.Length - 1;
            mat.texes = new string[numTexes];
            for (int i = 0; i < numTexes; i++)
            {
                mat.texes[i] = buffer[i + 1];
            }
            return mat;
        }
        bool checkMat(Mat mat)
        {
            for (int i = 0; i < mat.texes.Length; i++)
            {
                if (!File.Exists(mat.texes[i])) return false;
            }
            return true;
        }
        void MatAdd(Mat mat)
        {
            if (checkMat(mat))
            {
                MAT.Add(mat);
            }
            else
            {
                MessageBox.Show("Mat path exist error: " + mat.name);
            }
        }
        private void button4_Click(object sender, EventArgs e) //Add
        {
            if (checkTbFormatting())
            {
                MatAdd(getTextBoxMatInfo());
                refreshListBox();
                saveCollections();
            }
            else
            {
                MessageBox.Show("Formatting wrong");
            }
        }

        private void button1_Click(object sender, EventArgs e) //Save
        {
            if (checkTbFormatting())
            {
                string selectedName = listBox1.SelectedItem.ToString();
                removeMatByName(selectedName);
                MatAdd(getTextBoxMatInfo());
                refreshListBox();
                saveCollections();
            }
        }
        private void button3_Click(object sender, EventArgs e) //Toggle
        {
            if (listBox1.SelectedIndex != -1)
            {
                string selectedName = listBox1.SelectedItem.ToString();
                Mat matbyname = getMatByName(selectedName);
                int numMats = matbyname.texes.Length;
                for (int i = 0; i < numMats; i++)
                {
                    if (toggle(matbyname.texes[i]))
                    {
                        label2.ForeColor = Color.Green;
                        label2.Text = "ON";
                    }
                    else
                    {
                        label2.ForeColor = Color.Red;
                        label2.Text = "OFF";
                    }
                }
            }
            else
            {
                label2.Text = "Mat not selected";
            }
                
        }
        bool checkTbFormatting()
        {
            return !(richTextBox1.Text[richTextBox1.Text.Length - 1] == '\n'
                || !richTextBox1.Text.Contains('\n'));
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
