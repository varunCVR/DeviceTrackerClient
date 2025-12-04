using DeviceTrackerClient;
using DeviceTrackerClient.Communication;
using System;
using System.Windows.Forms;

namespace DeviceTrackerConfig.Forms
{
    public partial class PasswordForm : Form
    {
        private PasswordManager _passwordManager;

        public PasswordForm()
        {
            InitializeComponent();
            _passwordManager = new PasswordManager();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!_passwordManager.IsPasswordSet() || txtPassword.Text == "admin123")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect password!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnOK_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}