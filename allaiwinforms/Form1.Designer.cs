﻿using CefSharp;
using CefSharp.WinForms;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            new GeminiBrowserAndDetails(),
            new ChatGptBrowserAndDetails(), 
            //BrowserAndDetails("https://chat.openai.com", , "//*[@id='mySubmit']"),
            new BingChatBrowserAndDetails(),
            new ClaudeBrowserAndDetails(),
        };
        private TextBox inputLine;
        private IntPtr vscodeHandle;
        private SplitContainer firstRowSplitContainer;
        private SplitContainer secondRowSplitContainer;

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);


            // Create a SplitContainer for the rows
            var rowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };
            this.Controls.Add(rowSplitContainer);

            // Create a SplitContainer for the first row's columns
            firstRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill
            };
            rowSplitContainer.Panel1.Controls.Add(firstRowSplitContainer);

            // Create a SplitContainer for the second row's columns
            secondRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill
            };
            rowSplitContainer.Panel2.Controls.Add(secondRowSplitContainer);

            // Create 4 ChromiumWebBrowser instances and add them to the SplitContainers
            var urls = browserAndDetails.Select(x => x.Url).ToList();
            // firstRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[0]));
            firstRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[1]));
            secondRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[2]));
            secondRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[3]));


            inputLine = new TextBox
            {
                Dock = DockStyle.Top
            };
            this.Controls.Add(inputLine);

            // Create a Button for the submit action
            var submitButton = new Button { Text = "Submit", Dock = DockStyle.Top };
            this.Controls.Add(submitButton);
            // Handle the Click event of the submit button
            submitButton.Click += OnSubmit;
            inputLine.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Handle Enter key press here
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    OnSubmit(null, null);
                }
            };
            this.Text = "Form1";
            StartVsCode();
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
                    browserAndDetail.Browser.Focus();
                    try
                    {
                        await browserAndDetail.Submit(inputText);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception: " + ex.Message + ex.StackTrace);
                    }
                }
                Properties.Settings.Default.PromptHistory.Add(inputText);
                inputLine.Clear();
            };
        }

        #endregion

        public async Task StartVsCode()
        {
            Dictionary<IntPtr, string> windowHandlesWithNames = WindowHandleGetter.GetWindowHandlesWithProcessNames().ToDictionary(entry => entry.Key, entry => (string)entry.Value.Clone());
            await Task.Delay(1000);
            Process.Start(@"C:\Users\Sahin\AppData\Local\Programs\Microsoft VS Code\Code.exe");

            // Add a delay to give Visual Studio Code time to start up
            await Task.Delay(5000);
            Dictionary<IntPtr, string> newWindowHandlesWithNames = WindowHandleGetter.GetWindowHandlesWithProcessNames();
            IntPtr codeWindowHandle = newWindowHandlesWithNames
                .Where(kvp => kvp.Value == "Code" && !windowHandlesWithNames.ContainsKey(kvp.Key))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            // Embed Visual Studio Code into the first panel of the first row
            SetParent(codeWindowHandle, firstRowSplitContainer.Panel1.Handle);
            MoveWindow(codeWindowHandle, 0, 0, firstRowSplitContainer.Panel1.Width, firstRowSplitContainer.Panel1.Height, true);

        }
        public void SendKeysToVSCode(string keys)
        {
            // Activate the Visual Studio Code window
            SetForegroundWindow(vscodeHandle);

            // Send keys
            SendKeys.SendWait(keys);
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
            // Escape the input text for use in a JavaScript string

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
    internal class GeminiBrowserAndDetails : BrowserAndDetails
    {

        public GeminiBrowserAndDetails()
        {
            Url = "https://gemini.google.com";
        }

        public override async Task Submit(string inputText)
        {
            // Escape the input text for use in a JavaScript string
            string escapedInputText = System.Security.SecurityElement.Escape(inputText);



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
