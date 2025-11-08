namespace InputAutoSwitch
{
    public partial class MainForm : Form
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;
        private KeyboardHook? keyboardHook;
        private System.Windows.Forms.Timer? idleTimer;
        private bool isEnabled = true;
        private const int IDLE_MILLISECONDS = 2000; // 2秒 = 2000毫秒

        public MainForm()
        {
            InitializeComponent();
            InitializeTrayIcon();
            InitializeKeyboardHook();
            InitializeTimer();

            // 隐藏主窗体,只显示托盘图标
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void InitializeTrayIcon()
        {
            // 创建右键菜单
            trayMenu = new ContextMenuStrip();
            
            var enableItem = new ToolStripMenuItem("启用自动切换");
            enableItem.Checked = isEnabled;
            enableItem.Click += (s, e) => ToggleEnable();
            
            var autoStartItem = new ToolStripMenuItem("开机自启动");
            autoStartItem.Checked = AutoStartManager.IsAutoStartEnabled();
            autoStartItem.Click += (s, e) => ToggleAutoStart();
            
            var testItem = new ToolStripMenuItem("测试输入法检测");
            testItem.Click += (s, e) => TestInputMethod();
            
            var exitItem = new ToolStripMenuItem("退出");
            exitItem.Click += (s, e) => ExitApplication();

            trayMenu.Items.Add(enableItem);
            trayMenu.Items.Add(autoStartItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(testItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(exitItem);

            // 创建托盘图标
            trayIcon = new NotifyIcon
            {
                Text = "输入法自动切换 - 运行中",
                Visible = true,
                ContextMenuStrip = trayMenu
            };

            // 设置图标 (优先使用应用程序图标，否则使用系统图标)
            try
            {
                trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                trayIcon.Icon = SystemIcons.Application;
            }

            // 双击托盘图标显示状态
            trayIcon.DoubleClick += (s, e) => ShowStatus();
        }

        private void InitializeKeyboardHook()
        {
            keyboardHook = new KeyboardHook();
            keyboardHook.KeyPressed += OnKeyPressed;
            keyboardHook.Start();
        }

        private void InitializeTimer()
        {
            // 创建定时器但不启动
            idleTimer = new System.Windows.Forms.Timer();
            idleTimer.Interval = IDLE_MILLISECONDS; // 2000毫秒 = 2秒
            idleTimer.Tick += OnTimerTick;
            // 不自动启动，等待第一次按键
        }

        private void OnKeyPressed(object? sender, EventArgs e)
        {
            if (!isEnabled)
                return;

            // 停止当前定时器（如果正在运行）
            idleTimer?.Stop();
            
            // 重新启动2秒定时器
            idleTimer?.Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // 定时器触发，说明2秒内没有新的按键
            // 停止定时器（单次触发）
            idleTimer?.Stop();

            if (!isEnabled)
                return;

            // 检查输入法状态
            bool isChinese = InputMethodManager.IsChineseInputMode();
            
            if (isChinese)
            {
                // 可选：显示切换提示（如果不想看到提示可以注释掉）
                // trayIcon?.ShowBalloonTip(500, "自动切换", "切换到英文", ToolTipIcon.Info);
                
                // 设置标志,防止循环触发
                keyboardHook?.SetAutoSwitchingFlag(true);
                
                // 发送 Ctrl+Space
                InputMethodManager.SendCtrlSpace();
                
                // 延迟后重置标志
                Task.Delay(100).ContinueWith(_ =>
                {
                    try
                    {
                        this.Invoke(() =>
                        {
                            keyboardHook?.SetAutoSwitchingFlag(false);
                        });
                    }
                    catch { }
                });
            }

        }

        private void ToggleEnable()
        {
            isEnabled = !isEnabled;
            UpdateTrayMenu();
            
            string status = isEnabled ? "已启用" : "已禁用";
            trayIcon!.Text = $"输入法自动切换 - {status}";
            trayIcon.ShowBalloonTip(1000, "状态更改", $"自动切换功能{status}", ToolTipIcon.Info);
        }

        private void ToggleAutoStart()
        {
            AutoStartManager.ToggleAutoStart();
            UpdateTrayMenu();
            
            string status = AutoStartManager.IsAutoStartEnabled() ? "已启用" : "已禁用";
            trayIcon!.ShowBalloonTip(1000, "设置更改", $"开机自启动{status}", ToolTipIcon.Info);
        }

        private void UpdateTrayMenu()
        {
            if (trayMenu != null && trayMenu.Items.Count >= 2)
            {
                ((ToolStripMenuItem)trayMenu.Items[0]).Checked = isEnabled;
                ((ToolStripMenuItem)trayMenu.Items[1]).Checked = AutoStartManager.IsAutoStartEnabled();
            }
        }

        private void ShowStatus()
        {
            string enableStatus = isEnabled ? "运行中" : "已暂停";
            string autoStartStatus = AutoStartManager.IsAutoStartEnabled() ? "已启用" : "未启用";
            
            MessageBox.Show(
                $"输入法自动切换工具\n\n" +
                $"当前状态: {enableStatus}\n" +
                $"开机自启: {autoStartStatus}\n" +
                $"空闲检测: {IDLE_MILLISECONDS / 1000}秒\n\n" +
                $"功能说明:\n" +
                $"- 检测到键盘空闲{IDLE_MILLISECONDS / 1000}秒后\n" +
                $"- 如果输入法为中文模式\n" +
                $"- 自动切换到英文模式",
                "状态信息",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void TestInputMethod()
        {
            bool isChinese = InputMethodManager.IsChineseInputMode();
            string info = InputMethodManager.GetInputMethodInfo();
            
            MessageBox.Show(
                $"输入法检测测试\n\n" +
                $"是否为中文模式: {(isChinese ? "是" : "否")}\n" +
                $"详细信息: {info}\n\n" +
                $"请先在任意文本框中切换到中文输入法\n" +
                $"然后点击此菜单测试",
                "输入法检测",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "确定要退出输入法自动切换工具吗?",
                "确认退出",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                keyboardHook?.Stop();
                keyboardHook?.Dispose();
                idleTimer?.Stop();
                idleTimer?.Dispose();
                trayIcon!.Visible = false;
                trayIcon?.Dispose();
                Application.Exit();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 防止直接关闭,最小化到托盘
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ResumeLayout(false);
        }
    }
}
