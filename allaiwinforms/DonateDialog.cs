using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace allaiwinforms
{
    public partial class DonateDialog : Form
    {
        public DonateDialog()
        {
            InitializeComponent();

            this.Text = "Donation";
            this.Size = new Size(300, 200);
                this.StartPosition = FormStartPosition.CenterScreen; // Add this line


            var label = new Label { Text = "PromptAll is free, and always will be.  Thanks to the support of generous donors like you, we can continue to offer our services free of charge.  Would you consider making a donation today?", Dock = DockStyle.Top };
            this.Controls.Add(label);

            var donateButton = new Button { Text = "Support Us🎉", Dock = DockStyle.Bottom };
            donateButton.Click += (sender, e) => 
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://linktr.ee/sahin52",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                this.Close();
            };   
            this.Controls.Add(donateButton);

            var cancelButton = new Button { Text = "Cancel", Dock = DockStyle.Bottom };
            cancelButton.Click += (sender, e) => this.Close();
            this.Controls.Add(cancelButton);
        }
    }
}
