using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace AppBlockerService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // processInstaller
            // 
            this.processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.processInstaller.Password = null;
            this.processInstaller.Username = null;
            this.processInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.processInstaller_AfterInstall);
            // 
            // serviceInstaller
            // 
            this.serviceInstaller.Description = "Monitors and blocks applications based on configurable rules";
            this.serviceInstaller.DisplayName = "Application Blocker & Process Logger";
            this.serviceInstaller.ServiceName = "AppBlockerService";
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.processInstaller,
            this.serviceInstaller});

        }

        private void processInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}