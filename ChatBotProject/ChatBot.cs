using System;
using System.Windows.Forms;
using System.Media;
using System.IO; // needed for filing
using System.Speech.Synthesis;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using System.Drawing;
using System.Data;


namespace ChatBotProject
{
    public partial class Form1 : Form
    {
        SqlConnection Con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\ChatBotProject\ChatBotProject\DB\LDB.mdf;Integrated Security=True;Connect Timeout=30;Encrypt=False");
        SqlDataAdapter da;
        DataTable dt = new DataTable();
        SqlDataAdapter da_Ac;
        DataTable dt_Ac = new DataTable();
        public Form1()
        {
            InitializeComponent();

        }

        /*/////////////////////////////////////////////////////////////////////////////*/

        private string GetAnswerFromDatabase(string question)
        {
            string answer = "";

            try
            {
                if (Con.State != ConnectionState.Open)
                    Con.Open();

                // تقسيم السؤال إلى كلمات
                string[] keywords = question.Split(new char[] { ' ', ',', '.', '?', '!', ';', ':', '-', '_', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                if (keywords.Length == 0)
                    return "الرجاء إدخال سؤال واضح.";

                // بناء الاستعلام الديناميكي
                string sql = "SELECT TOP 1 answer FROM Chat WHERE ";
                for (int i = 0; i < keywords.Length; i++)
                {
                    sql += $"question LIKE @q{i}";
                    if (i < keywords.Length - 1)
                        sql += " OR ";
                }
                sql += " ORDER BY LEN(question) DESC"; // الأفضلية للأسئلة الأطول (الأكثر تحديدًا)

                SqlCommand cmd = new SqlCommand(sql, Con);

                // إضافة البراميترز
                for (int i = 0; i < keywords.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@q{i}", "%" + keywords[i] + "%");
                }

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    answer = reader["answer"].ToString();
                }
                else
                {
                    answer = "عذرًا، لم أتمكن من العثور على إجابة مناسبة.";
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                answer = "خطأ في الاتصال بقاعدة البيانات: " + ex.Message;
            }
            finally
            {
                if (Con.State == ConnectionState.Open)
                    Con.Close();
            }

            return answer;
        }

        /*/////////////////////////////////////////////////////////////////////////////*/

        static ChatBot bot;
        SpeechSynthesizer reader = new SpeechSynthesizer();
        bool textToSpeech = false;

        /*private void Form1_Load(object sender, EventArgs e)
        {
            bot = new ChatBot();
            //LoadAutoCompleteData();


            // Sets Position for the first bubble on the top
            bbl_old.Top = 0 - bbl_old.Height;

            // Load Chat from the log file
            if (File.Exists("chat.log"))
            {
                using (StreamReader sr = File.OpenText("chat.log"))
                {
                    int i = 0; // to count lines
                    while (sr.Peek() >= 0) // loop till the file ends
                    {
                        if (i % 2 == 0) // check if line is even
                        {
                            addInMessage(sr.ReadLine());
                        }
                        else
                        {
                            addOutMessage(sr.ReadLine());
                        }
                        i++;
                    }
                    // scroll to the bottom once finished loading.
                    panel2.VerticalScroll.Value = panel2.VerticalScroll.Maximum;
                    panel2.PerformLayout();
                }
            }

            /**********************************************************************************************/

        /*try
        {
            SqlConnection Conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\ChatBotProject\ChatBotProject\DB\LDB.mdf;Integrated Security=True;Connect Timeout=30;");

            Conn.Open();
            SqlCommand Command = new SqlCommand("select * from Chat", Conn);
            SqlDataReader reader = Command.ExecuteReader();

            while (reader.Read())
                comboBox1.Items.Add(reader["question"].ToString());

            Conn.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

    }*/
        /****************************************************************/


        private void Form1_Load(object sender, EventArgs e)
        {
            bot = new ChatBot();
            bbl_old.Top = 0 - bbl_old.Height;

            // تحميل سجل المحادثة من الملف
            if (File.Exists("chat.log"))
            {
                using (StreamReader sr = File.OpenText("chat.log"))
                {
                    int i = 0;
                    while (sr.Peek() >= 0)
                    {
                        if (i % 2 == 0)
                            addInMessage(sr.ReadLine());
                        else
                            addOutMessage(sr.ReadLine());
                        i++;
                    }
                    panel2.VerticalScroll.Value = panel2.VerticalScroll.Maximum;
                    panel2.PerformLayout();
                }
            }

            // تفعيل الاقتراح التلقائي من قاعدة البيانات
            try
            {
                SqlConnection Conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\ChatBotProject\ChatBotProject\DB\LDB.mdf;Integrated Security=True;Connect Timeout=30;Encrypt=False");

                Conn.Open();
                SqlCommand Command = new SqlCommand("SELECT question FROM Chat", Conn);
                SqlDataReader reader = Command.ExecuteReader();

                AutoCompleteStringCollection autoSource = new AutoCompleteStringCollection();

                while (reader.Read())
                {
                    string q = reader["question"].ToString();
                    comboBox1.Items.Add(q); // عرضها داخل القائمة
                    autoSource.Add(q);      // إضافتها للاقتراحات
                }

                comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboBox1.AutoCompleteCustomSource = autoSource;

                reader.Close();
                Conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأسئلة: " + ex.Message);
            }
        }
        /*******************************/

        /*private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                dt_Ac.Clear();
                da_Ac = new SqlDataAdapter("SELECT question FROM Chat WHERE question LIKE @text + '%'", Con);
                da_Ac.SelectCommand.Parameters.AddWithValue("@text", comboBox1.Text);
                da_Ac.Fill(dt_Ac);

                comboBox1.Items.Clear();
                foreach (DataRow row in dt_Ac.Rows)
                {
                    comboBox1.Items.Add(row["question"].ToString());
                }

                comboBox1.DroppedDown = true; // عرض القائمة
                comboBox1.SelectionStart = comboBox1.Text.Length;
                comboBox1.SelectionLength = 0;
            }
            catch
            {
                // تجاهل الأخطاء في حال عدم الاتصال المؤقت
            }
        }*/


        private void showOutput()
        {
            if (!(string.IsNullOrWhiteSpace(comboBox1.Text))) // Make sure the textbox isnt empty
            {
                SoundPlayer Send = new SoundPlayer("SOUND1.wav"); // Send Sound Effect
                SoundPlayer Rcv = new SoundPlayer("SOUND2.wav"); // Recieve Sound Effect

                // Show the user message and play the sound
                addInMessage(comboBox1.Text);
                Send.Play();

                // Store the Bot's Output by giving it our input.
                string outtt = GetAnswerFromDatabase(comboBox1.Text.Trim());


                if (string.IsNullOrEmpty(outtt))
                {
                    outtt = "لم أفهم ذلك. حاول بطريقة أخرى او تواصل مع خدمة الجمهور على الرقم التالي 777777777 .";
                }


                //=========== Creates backup of chat from user and bot to the given location ============
                FileStream fs = new FileStream(@"chat.log", FileMode.Append, FileAccess.Write);
                if (fs.CanWrite)
                {
                    byte[] write = System.Text.Encoding.ASCII.GetBytes(comboBox1.Text + Environment.NewLine + outtt + Environment.NewLine);
                    fs.Write(write, 0, write.Length);
                }
                fs.Flush();
                fs.Close();
                //=======================================================================================

                // Make a Dynamic Timer to delay the bot's response to make it feel humanlike.
                var t = new Timer();

                // Time in milseconds - minimum delay of 1s plus 0.1s per character.
                t.Interval = 1000 + (outtt.Length * 100);

                // Show the "Bot is typing.." text
                txtTyping.Show();

                // disable the chat box white the bot is typing to prevent user spam.
                comboBox1.Enabled = false;

                t.Tick += (s, d) =>
                {
                    // Once the timer ends

                    comboBox1.Enabled = true; // Enable Chat box

                    // Hide the "Bot is typing.." text
                    txtTyping.Hide();

                    // Show the bot message and play the sound
                    addOutMessage(outtt);
                    Rcv.Play();

                    // Text to Speech if enabled
                    if (textToSpeech)
                    {
                        reader.SpeakAsync(outtt);
                    }

                    comboBox1.Focus(); // Put the cursor back on the textbox
                    t.Stop();
                };
                t.Start(); // Start Timer

                comboBox1.Text = ""; // Reset textbox
            }
        }

        // Call the Output method when the send button is clicked.
        private void button1_Click(object sender, EventArgs e)
        {
            showOutput();
        }

        // Call the Output method when the enter key is pressed.
        
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                showOutput();
                e.SuppressKeyPress = true; // Disable windows error sound
            }
        }

        // Dummy Bubble created to store the previous bubble data.
        bubble bbl_old = new bubble();

        // User Message Bubble Creation
        public void addInMessage(string message)
        {
            // Create new chat bubble
            bubble bbl = new bubble(message, msgtype.In);
            bbl.Location = bubble1.Location; // Set the new bubble location from the bubble sample.
            bbl.Left += 50; // Indent the bubble to the right side.
            bbl.Size = bubble1.Size; // Set the new bubble size from the bubble sample.
            bbl.Top = bbl_old.Bottom + 10; // Position the bubble below the previous one with some extra space.

            // Add the new bubble to the panel.
            panel2.Controls.Add(bbl);

            // Force Scroll to the latest bubble
            bbl.Focus();

            // save the last added object to the dummy bubble
            bbl_old = bbl;
        }

        // Bot Message Bubble Creation
        public void addOutMessage(string message)
        {
            // Create new chat bubble
            bubble bbl = new bubble(message, msgtype.Out);
            bbl.Location = bubble1.Location; // Set the new bubble location from the bubble sample.
            bbl.Size = bubble1.Size; // Set the new bubble size from the bubble sample.
            bbl.Top = bbl_old.Bottom + 10; // Position the bubble below the previous one with some extra space.

            // Add the new bubble to the panel.
            panel2.Controls.Add(bbl);

            // Force Scroll to the latest bubble
            bbl.Focus();

            // save the last added object to the dummy bubble
            bbl_old = bbl;
        }

        // Custom close button to close the program when clicked.
        private void close_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        // Clear all the bubbles and chat.log
        private void clearChatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Delete the log file
            File.Delete(@"chat.log");

            // Clear the chat Bubbles
            panel2.Controls.Clear();

            // This reset the position for the next bubble to come back to the top.
            bbl_old.Top = 0 - bbl_old.Height;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void menuButton_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(menuButton, new System.Drawing.Point(0, -contextMenuStrip1.Size.Height));
        }

        private void toggleVoiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // whenever the toggle is clicked, true is set to false visa versa.
            textToSpeech = !textToSpeech;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dt_Ac.Clear();
            da_Ac = new SqlDataAdapter("select * from Chat where convert(varchar,question) like '%" + comboBox1.Text + "%'", Con);
            da_Ac.Fill(dt_Ac);
            this.comboBox1.DataSource = dt_Ac;

            //try
            //{
            //    dt_Ac.Clear();
            //    using (SqlDataAdapter da_Ac = new SqlDataAdapter("SELECT * FROM Chat WHERE question LIKE @q", Con))
            //    {
            //        da_Ac.SelectCommand.Parameters.AddWithValue("@q", "%" + comboBox1.Text + "%");
            //        da_Ac.Fill(dt_Ac);
            //        comboBox1.DataSource = dt_Ac;
            //        comboBox1.DisplayMember = "question";
            //    }

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("خطأ أثناء التصفية: " + ex.Message);
            //}
        }
        

    }
}