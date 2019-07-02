using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSP.DoxygenEditor.Views
{
    public partial class BuildDocumentationForm : Form
    {
        public string BaseDir { get; }
        public string ConfigFilePath { get; }
        public string CompilerPath { get; }
        public string OutputPath { get; }
        public bool OpenInBrowser => cbOpenInBrowser.Checked;

        private Process _activeProcess = null;

        public BuildDocumentationForm(string baseDir, string configFilePath, string compilerPath, string outputPath, bool openInBrowser)
        {
            InitializeComponent();

            BaseDir = baseDir;
            ConfigFilePath = configFilePath;
            CompilerPath = compilerPath;
            OutputPath = outputPath;

            lblRootPath.Text = $"Root Path: {BaseDir}";
            lblConfigFile.Text = $"Config File: {ConfigFilePath}";
            lblOutputPath.Text = $"Outpath Path: {OutputPath}";
            cbOpenInBrowser.Checked = openInBrowser;
        }

        private void AppendLogLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (rtbLog.InvokeRequired)
                    rtbLog.Invoke(new Action(() => AppendLogLine(line)));
                else
                    rtbLog.AppendText($"{line}{Environment.NewLine}");
            }
        }

        private void Build()
        {
            try
            {
                string doxgenExe = "doxygen.exe";
                if (!string.IsNullOrWhiteSpace(CompilerPath))
                {
                    string fullDoxygenFilePath = Path.Combine(CompilerPath, doxgenExe);
                    if (File.Exists(fullDoxygenFilePath))
                        doxgenExe = fullDoxygenFilePath;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = doxgenExe,
                    Arguments = $"\"{ConfigFilePath}\"",
                    WorkingDirectory = BaseDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process process = _activeProcess = new Process();
                process.StartInfo = startInfo;
                process.OutputDataReceived += (s, oe) => AppendLogLine(oe.Data);
                process.ErrorDataReceived += (s, oe) => AppendLogLine(oe.Data);
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                int exitCode = process.ExitCode;
                process.Close();
                if (exitCode != 0)
                    MessageBox.Show(this, $"Failed to generate doxygen documentation from compiler '{doxgenExe}' and configuration '{ConfigFilePath}'{Environment.NewLine}Code: {exitCode}", "Doxygen documentation generator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
            }
            finally
            {
                _activeProcess = null;
            }
        }

        private void Done()
        {
            if (OpenInBrowser && !string.IsNullOrWhiteSpace(OutputPath) && Directory.Exists(OutputPath))
            {
                string htmlOutFilePath = Path.Combine(OutputPath, "html", "index.html");
                if (File.Exists(htmlOutFilePath))
                {
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo();
                    proc.StartInfo.FileName = htmlOutFilePath;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            btnBuild.Enabled = false;
            btnCancel.Text = "Stop";
            rtbLog.Clear();
            var task = Task.Run(() =>
            {
                Build();
            }).ContinueWith((t) =>
            {
                this.Invoke(new Action(() =>
                {
                    btnBuild.Enabled = true;
                    btnCancel.Text = "Cancel";
                    Done();
                }));
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_activeProcess == null || _activeProcess.HasExited)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
