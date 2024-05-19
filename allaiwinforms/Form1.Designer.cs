using CefSharp;
using CefSharp.WinForms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using WindowsInput;
using static System.Net.Mime.MediaTypeNames;
namespace allaiwinforms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private List<BrowserAndDetails> browserAndDetails = new List<BrowserAndDetails>(){
            new ChatGptBrowserAndDetails(),
            new BingChatBrowserAndDetails(),
            new ClaudeBrowserAndDetails(),
        };
        private TextBox inputLine;
        private IntPtr vscodeHandle;
        private SplitContainer firstRowSplitContainer;
        private SplitContainer secondRowSplitContainer;
        private SplitContainer rowSplitContainer;

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        private ChromiumWebBrowser CreateBrowser(string url)
        {
            var browser = new ChromiumWebBrowser(url)
            {
                Dock = DockStyle.Fill
            };
            browserAndDetails.FirstOrDefault(u => u.Url == url).Browser = browser;
            return browser;
        }
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            #region Adjust the height of the input line
            int lineHeight = this.Font.Height;
            int totalLines = 0;

            // Split the text into lines
            string[] lines = inputLine.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                // Calculate the width of the line
                int lineWidth = TextRenderer.MeasureText(line, this.Font).Width;

                // Calculate the number of lines needed for this line of text
                int lineCount = Math.Max(1, (int)Math.Ceiling((double)lineWidth / (this.Width - 42)));

                // Add the number of lines to the total
                totalLines += lineCount;
            }

            // Adjust height to ensure all text is visible
            inputLine.Height = Math.Min(200, Math.Max(20, totalLines * lineHeight + Padding.Top + Padding.Bottom)) + 5; // Add some padding for better appearance
            #endregion
        }
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += Form1_Load;
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);


            // Create a SplitContainer for the rows
            rowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
            };
            this.Controls.Add(rowSplitContainer);
            rowSplitContainer.SplitterDistance = rowSplitContainer.Width / 2;

            // Create a SplitContainer for the first row's columns
            firstRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
            };
            rowSplitContainer.Panel1.Controls.Add(firstRowSplitContainer);
            firstRowSplitContainer.SplitterDistance = firstRowSplitContainer.Width / 2;

            // Create a SplitContainer for the second row's columns
            secondRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
            };
            rowSplitContainer.Panel2.Controls.Add(secondRowSplitContainer);
            secondRowSplitContainer.SplitterDistance = secondRowSplitContainer.Width / 2;
            // Create 4 ChromiumWebBrowser instances and add them to the SplitContainers
            var urls = browserAndDetails.Select(x => x.Url).ToList();
            // firstRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[0]));
            StartVsCode().ContinueWith(res =>
            {
                if (res.Result != IntPtr.Zero)
                    browserAndDetails.Add(new VsCodeSubmitter(res.Result));
                else
                {
                    if (this.rowSplitContainer.InvokeRequired)
                    {
                        this.rowSplitContainer.Invoke(new MethodInvoker(delegate
                        {
                            this.rowSplitContainer.SplitterDistance = rowSplitContainer.Width / 3;
                            firstRowSplitContainer.SplitterDistance = 0;
                            //secondRowSplitContainer.SplitterDistance = secondRowSplitContainer.Width / 2;
                        }));
                    }
                    else
                    {
                        this.rowSplitContainer.SplitterDistance = rowSplitContainer.Width / 3;
                        firstRowSplitContainer.SplitterDistance = 0;
                        //secondRowSplitContainer.SplitterDistance = secondRowSplitContainer.Width / 2;
                    }
                }
            });
            firstRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[1]));
            secondRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[2]));
            secondRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[0]));
            
            // Create a Button for the submit action
            var submitButton = new Button { 
                Text = "Submit", 
                Dock = DockStyle.Top,
                BackColor = Color.LightBlue, // Change the background color
                ForeColor = Color.DarkBlue, // Change the text color
                FlatStyle = FlatStyle.Flat, // Change the style to flat
                Cursor = Cursors.Hand, // Change the cursor to a hand when it's over the button
            };
            this.Controls.Add(submitButton);
            // Handle the Click event of the submit button
            submitButton.Click += OnSubmit;
            inputLine = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Height = 25
            };
            this.Controls.Add(inputLine);
            
            inputLine.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    // Handle Enter key press here
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    OnSubmit(null, null);
                }
            };
            inputLine.TextChanged += TextBox_TextChanged;
            this.Text = "Form1";
            this.SizeChanged += Form1_SizeChanged;
            this.SizeChanged += TextBox_TextChanged;
            firstRowSplitContainer.SizeChanged += Form1_SizeChanged;
            firstRowSplitContainer.Panel1.SizeChanged += Form1_SizeChanged;
            firstRowSplitContainer.Panel2.SizeChanged += Form1_SizeChanged;
            secondRowSplitContainer.SizeChanged += Form1_SizeChanged;
            secondRowSplitContainer.Panel1.SizeChanged += Form1_SizeChanged;
            secondRowSplitContainer.Panel2.SizeChanged += Form1_SizeChanged;

        }
        private async void OnSubmit(object sender, EventArgs e)
        {
            {
                // Get the input line's text
                string inputText = inputLine.Text;
                // Set the value of the HTML input in each browser
                foreach (var browserAndDetail in browserAndDetails)
                {
                    /**
                    function getElementByXPath(path) {
  return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
}                   // gemini:
                     getElementByXPath('//*[@id="app-root"]/main/side-navigation-v2/mat-sidenav-container/mat-sidenav-content/div/div[2]/chat-window/div[1]/div[2]/div[1]/input-area-v2/div/div/div[1]/div/div/rich-textarea/div[1]').focus()
                     */
                    try
                    {
                        await browserAndDetail.Submit(inputText);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception: " + ex.Message + ex.StackTrace);
                    }
                }
                //Properties.Settings.Default.PromptHistory.Add(inputText);
                inputLine.Clear();
            };
        }

        #endregion

        private async Task<IntPtr> StartVsCode()
        {
            try
            {
                Dictionary<IntPtr, string> windowHandlesWithNames = WindowHandleGetter.GetWindowHandlesWithProcessNames().ToDictionary(entry => entry.Key, entry => (string)entry.Value.Clone());
                await Task.Delay(1000);
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string vscodePath = Path.Combine(localAppData, @"Programs\Microsoft VS Code\Code.exe");
                Process.Start(vscodePath);
                // Add a delay to give Visual Studio Code time to start up
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(100);
                    Dictionary<IntPtr, string> newWindowHandlesWithNames = WindowHandleGetter.GetWindowHandlesWithProcessNames();
                    vscodeHandle = newWindowHandlesWithNames
                        .Where(kvp => kvp.Value == "Code" && !windowHandlesWithNames.ContainsKey(kvp.Key))
                        .Select(kvp => kvp.Key)
                        .FirstOrDefault();
                    if (vscodeHandle != IntPtr.Zero) break;
                }

                // Embed Visual Studio Code into the first panel of the first row
                SetParent(vscodeHandle, firstRowSplitContainer.Panel1.Handle);
                MoveWindow(vscodeHandle, 0, 0, firstRowSplitContainer.Panel1.Width, firstRowSplitContainer.Panel1.Height, true);
                return vscodeHandle;
            }
            catch (Exception ex)
            {
                return IntPtr.Zero;
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (vscodeHandle != IntPtr.Zero)
                MoveWindow(vscodeHandle, 0, 0, firstRowSplitContainer.Panel1.Width, firstRowSplitContainer.Panel1.Height, true);
        }
    }

    public class WindowHandleGetter
    {
        // Import the required Win32 functions
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // Define the callback delegate for EnumWindows
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Dictionary to store the window handles and process names
        private static Dictionary<IntPtr, string> windowHandlesWithProcessNames = new Dictionary<IntPtr, string>();

        // Callback function for EnumWindows
        private static bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            // Get the process name
            Process process = Process.GetProcessById((int)processId);
            string processName = process.ProcessName;

            // Check if the window associated with the process is visible
            if (IsWindowVisible(hWnd))
            {
                // Add the window handle and process name to the dictionary
                windowHandlesWithProcessNames[hWnd] = processName;
            }

            // Continue enumerating
            return true;
        }

        public static Dictionary<IntPtr, string> GetWindowHandlesWithProcessNames()
        {
            windowHandlesWithProcessNames.Clear();
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            return windowHandlesWithProcessNames;
        }
    }

    [DebuggerDisplay("url={url},inputXpath={inputXpath},submitXpath={submitXpath}")]
    internal abstract class BrowserAndDetails
    {
        public ChromiumWebBrowser Browser { get; set; }
        public string Url { get; protected set; }
        public string InputXpath { get; protected set; } = "";
        public string SubmitXpath { get; protected set; } = "";

        public abstract Task Submit(string inputText);
    }
    internal class ChatGptBrowserAndDetails : BrowserAndDetails
    {
        public ChatGptBrowserAndDetails()
        {
            Url = "https://chat.openai.com";
        }

        public override async Task Submit(string inputText)
        {
            Browser.Focus();
            string enterInputScript = $@"
                            var input = document.evaluate('{InputXpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                            input.focus();
                            // input.dispatchEvent(event);
                        ";
            await Browser.EvaluateScriptAsync(enterInputScript);
            foreach (char c in inputText)
            {
                var keyEvent = new KeyEvent
                {
                    WindowsKeyCode = (int)c,
                    Type = KeyEventType.Char,
                    IsSystemKey = false
                };

                Browser.GetBrowser().GetHost().SendKeyEvent(keyEvent);
            }
            string getButtonCoordinatesScript = $@"
                        var button1 = document.getElementsByClassName(""absolute bottom-1.5 right-2 rounded-lg border border-black bg-black p-0.5 text-white transition-colors"")[0]
                        button1.click()
                        var rect = button1.getBoundingClientRect();
                        ({{
                            x: rect.left + window.scrollX,
                            y: rect.top + window.scrollY
                        }})
                    ";
            try
            {


                JavascriptResponse response = await Browser.EvaluateScriptAsync(getButtonCoordinatesScript);
                Dictionary<string, object> coordinates = response.Result as Dictionary<string, object>;
                Debug.WriteLine(response.Result);
                if (response.Result is IDictionary<string, object> coordinate2)
                {
                    double x = Convert.ToDouble(coordinate2["x"]);
                    double y = Convert.ToDouble(coordinate2["y"]);

                }

                string clickOnSubmitScript = $@"
                                          function getElementByXPath(path) {{
                                            return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                                          }}
                                          getElementByXPath('//*[@id=""__next""]/div[1]/div/main/div[1]/div[2]/div[1]/div/form/div/div[2]/div/button').click() ";
                await Browser.EvaluateScriptAsync(clickOnSubmitScript);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception on ChatGpt click on submit:" + ex.Message + ex.StackTrace);
            }

        }

    }
    internal class VsCodeSubmitter : BrowserAndDetails
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private nint vsCodeHandle;

        public VsCodeSubmitter(IntPtr handle)
        {
            this.vsCodeHandle = handle;
        }
        private void SendKeysToVSCode(string keys)
        {
            if (this.vsCodeHandle == IntPtr.Zero)
                return;
            // Activate the Visual Studio Code window
            SetForegroundWindow(this.vsCodeHandle);

            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.TextEntry(keys);
            SendKeys.SendWait("\n");
        }

        public override async Task Submit(string inputText)
        {
            // Escape the input text for use in a JavaScript string
            string escapedInputText = System.Security.SecurityElement.Escape(inputText);
            SendKeysToVSCode(inputText);
            // SendKeysToVSCode("\n");


        }
    }
    internal class ClaudeBrowserAndDetails : BrowserAndDetails
    {
        public ClaudeBrowserAndDetails()
        {
            Url = "https://claude.ai";
        }
        public override async Task Submit(string inputText)
        {
            Browser.Focus();
            var focusScript = @"document.querySelector('[contenteditable=""true""]').focus()";
            await Browser.EvaluateScriptAsync(focusScript);

            foreach (char c in inputText)
            {
                var keyEvent = new KeyEvent
                {
                    WindowsKeyCode = (int)c,
                    Type = KeyEventType.Char,
                    IsSystemKey = false
                };

                Browser.GetBrowser().GetHost().SendKeyEvent(keyEvent);
            }

            var clickOnSubmitScript = @"document.querySelector('[aria-label=""Send Message""]').click()";

            await Browser.EvaluateScriptAsync(clickOnSubmitScript);

        }
    }
    internal class BingChatBrowserAndDetails : BrowserAndDetails
    {
        public BingChatBrowserAndDetails()
        {
            Url = "https://www.bing.com/chat";
        }
        public override async Task Submit(string inputText)
        {
            Browser.Focus();
            // Escape the input text for use in a JavaScript string
            string escapedInputText = System.Security.SecurityElement.Escape(inputText);
            string selectTextAreaScript = @"
                function getElementByXPath(path) {
                    return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                }
                getElementByXPath('//*[@id=""b_sydConvCont""]/cib-serp').shadowRoot.querySelector('#cib-action-bar-main').shadowRoot.querySelector('cib-text-input').shadowRoot.querySelector('#searchbox').click() ";
            await Browser.EvaluateScriptAsync(selectTextAreaScript);
            foreach (char c in inputText)
            {
                var keyEvent = new KeyEvent
                {
                    WindowsKeyCode = (int)c,
                    Type = KeyEventType.Char,
                    IsSystemKey = false
                };

                Browser.GetBrowser().GetHost().SendKeyEvent(keyEvent);
            }

            // Submit
            var clickOnSubmitScript = @"
                    getElementByXPath('//*[@id=""b_sydConvCont""]/cib-serp').shadowRoot
                        .querySelector('#cib-action-bar-main').shadowRoot
                        .querySelector('button[description=""Submit""]').click()
                ";
            await Browser.EvaluateScriptAsync(clickOnSubmitScript);
        }

    }
}
