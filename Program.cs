namespace InputAutoSwitch
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 防止多开
            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, "InputAutoSwitch_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("程序已在运行中!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
        }
    }
}




