﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//Isuru Weerasinghe
namespace captive
{
    public partial class Service1 : ServiceBase
    {
        private AutoResetEvent AutoEventInstance { get; set; }
        private StatusChecker StatusCheckerInstance { get; set; }
        private Timer StateTimer { get; set; }
        public int TimerInterval { get; set; }
        public Service1()
        {
            InitializeComponent();
            TimerInterval = 1000;
        }
        Process proc = new Process();

        protected override void OnStart(string[] args)
        {

            AutoEventInstance = new AutoResetEvent(false);
            StatusCheckerInstance = new StatusChecker();

            // Create the delegate that invokes methods for the timer.
            TimerCallback timerDelegate =
                new TimerCallback(StatusCheckerInstance.CheckStatus);

            // Create a timer that signals the delegate to invoke 
            // 1.CheckStatus immediately, 
            // 2.Wait until the job is finished,
            // 3.then wait 5 minutes before executing again. 
            // 4.Repeat from point 2.
            Console.WriteLine("{0} Creating timer.\n",
                DateTime.Now.ToString("h:mm:ss.fff"));
            //Start Immediately but don't run again.
            StateTimer = new Timer(timerDelegate, AutoEventInstance, 0, Timeout.Infinite);
            while (StateTimer != null)
            {
                Process[] pname = Process.GetProcessesByName("autolog");
                if (pname.Length == 0)
                {
                    proc.StartInfo.WorkingDirectory = "C:\\au";
                    proc.StartInfo.FileName = "autolog.exe";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    proc.Start();
                    proc.WaitForExit();
                }

                //Wait until the job is done
                AutoEventInstance.WaitOne();
                //Wait for 5 minutes before starting the job again.
                StateTimer.Change(TimerInterval, Timeout.Infinite);
            }
            //If the Job somehow takes longer than 5 minutes to complete then it wont matter because we will always wait another 5 minutes before running again.
            ////////////////////////////////


        }

        protected override void OnStop()
        {
            proc.Kill();
            proc.WaitForExit();
        }
    }
    class StatusChecker
    {

        public StatusChecker()
        {
        }

        // This method is called by the timer delegate.
        public void CheckStatus(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            Console.WriteLine("{0} Start Checking status.",
                DateTime.Now.ToString("h:mm:ss.fff"));
            //This job takes time to run. For example purposes, I put a delay in here.
            int milliseconds = 5000;
            Thread.Sleep(milliseconds);
            //Job is now done running and the timer can now be reset to wait for the next interval
            Console.WriteLine("{0} Done Checking status.",
                DateTime.Now.ToString("h:mm:ss.fff"));
            autoEvent.Set();
        }
    }
}
