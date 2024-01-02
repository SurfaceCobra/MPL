using MPLLib;
using MPLLib.Beauty;
using System.Windows.Forms;
namespace FormTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = richTextBox1.Text;
            ScriptReader reader = new ScriptReader(text);
            foreach(var v in reader)
            {

                switch(v.o)
                {
                    case "hello":
                        richTextBox1.Select(v.range.Start.Value, v.range.Length());
                        richTextBox1.SelectionColor = Color.Red;
                        break;
                    default:
                        richTextBox1.Select(v.range.Start.Value, v.range.Length());
                        richTextBox1.SelectionColor = Color.Black;
                        break;
                }

            }
        }


        
    }
}
