using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gas.Helpers
{
    /// <summary>
    /// Displays a large message in the center of the screen using Windows.Forms.
    /// </summary>
    public partial class BigScreenMessage : Form
    {
        public BigScreenMessage( string messageText )
        {
            InitializeComponent();
            messageLabel.Text = messageText;
        }

        /// <summary>
        /// Gets and sets the big message to display.
        /// </summary>
        public string Message
        {
            set
            {
                messageLabel.Text = value;
            }
            get
            {
                return messageLabel.Text;
            }
        }
    }
}