using CefSharp;
using CefSharp.WinForms;
using System.Diagnostics;

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
            var firstRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill
            };
            rowSplitContainer.Panel1.Controls.Add(firstRowSplitContainer);

            // Create a SplitContainer for the second row's columns
            var secondRowSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill
            };
            rowSplitContainer.Panel2.Controls.Add(secondRowSplitContainer);

            // Create 4 ChromiumWebBrowser instances and add them to the SplitContainers
            var urls = browserAndDetails.Select(x => x.Url).ToList();
            firstRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[0]));
            firstRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[1]));
            secondRowSplitContainer.Panel1.Controls.Add(CreateBrowser(urls[2]));
            secondRowSplitContainer.Panel2.Controls.Add(CreateBrowser(urls[3]));


            var inputLine = new TextBox
            {
                Dock = DockStyle.Top
            };
            this.Controls.Add(inputLine);

            // Create a Button for the submit action
            var submitButton = new Button { Text = "Submit", Dock = DockStyle.Top };
            this.Controls.Add(submitButton);
            // Handle the Click event of the submit button
            submitButton.Click += async (sender, e) =>
            {
                // Get the input line's text
                string inputText = inputLine.Text;
                // Set the value of the HTML input in each browser
                foreach (var browserAndDetail in browserAndDetails)
                {
                    /**
                    function getElementByXPath(path) {
  return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
}                   gemini:
                     getElementByXPath('//*[@id="app-root"]/main/side-navigation-v2/mat-sidenav-container/mat-sidenav-content/div/div[2]/chat-window/div[1]/div[2]/div[1]/input-area-v2/div/div/div[1]/div/div/rich-textarea/div[1]').focus()
                     */
                    browserAndDetail.Browser.Focus();
                    await browserAndDetail.Submit(inputText);
                }
            };
            this.Text = "Form1";
        }

        #endregion
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
            string escapedInputText = System.Security.SecurityElement.Escape(inputText);

            string enterInputScript = $@"
                            var input = document.evaluate('{InputXpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                            var event = new KeyboardEvent('keydown', {{key: '{escapedInputText}'}});
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
            string getHtmlScript = @"
                        document.documentElement.outerHTML;
                    ";
            JavascriptResponse res = await Browser.EvaluateScriptAsync(getHtmlScript);
            string html = res.Result.ToString();
            Console.WriteLine(html);
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
                Console.WriteLine(response.Result);
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
            // Escape the input text for use in a JavaScript string
            string escapedInputText = System.Security.SecurityElement.Escape(inputText);

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

        }

    }
}
