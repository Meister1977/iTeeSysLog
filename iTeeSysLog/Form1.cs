using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using log4net.Appender;

namespace iTeeSysLog
{
    public partial class Form1 : Form
    {
        private bool _isTextBoxAttached;

        private readonly string _logFilePath = Application.StartupPath;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = !button1.Enabled;
            button2.Enabled = !button2.Enabled;
            SysLogListener.SetUp();
            if (!_isTextBoxAttached)
                AttacheTextBoxToLog();
            _isTextBoxAttached = true;
        }

        private void AttacheTextBoxToLog()
        {
            var watch = new FileSystemWatcher
                        {
                            Path = _logFilePath,
                            Filter = "SysLog.log",
                            NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                        };
            watch.Changed += OnLogFileChanged;
            watch.EnableRaisingEvents = true;
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _logFilePath + @"\SysLog.log")
            {
                // do stuff
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = !button1.Enabled;
            button2.Enabled = !button2.Enabled;
            SysLogListener.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_logFilePath + @"\SysLog.log");
        }
    }

    public class TextBoxAppender : AppenderSkeleton
    {
        public TextBox AppenderTextBox { get; set; }

        public string FormName { get; set; }
        public string TextBoxName { get; set; }

        private static Control FindControlRecursive(Control root, string textBoxName)
        {
            if (root.Name == textBoxName) return root;
            return (from Control c in root.Controls select FindControlRecursive(c, textBoxName)).FirstOrDefault(t => t != null);
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            if (AppenderTextBox == null)
            {
                if (string.IsNullOrEmpty(FormName) ||
                    string.IsNullOrEmpty(TextBoxName))
                    return;

                var form = Application.OpenForms[FormName];
                if (form == null)
                    return;

                AppenderTextBox = (TextBox)FindControlRecursive(form, TextBoxName);
                if (AppenderTextBox == null)
                    return;

                form.FormClosing += (s, e) => AppenderTextBox = null;
            }
            AppenderTextBox.BeginInvoke((MethodInvoker)delegate
            {
                AppenderTextBox.AppendText(RenderLoggingEvent(loggingEvent));
            });
        }
    }
}
