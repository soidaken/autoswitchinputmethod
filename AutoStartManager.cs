using Microsoft.Win32;

namespace InputAutoSwitch
{
    /// <summary>
    /// 开机自启动管理类
    /// </summary>
    public static class AutoStartManager
    {
        private const string APP_NAME = "InputAutoSwitch";
        private const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// 检查是否已设置开机自启动
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue(APP_NAME);
                        return value != null;
                    }
                }
            }
            catch
            {
                // 忽略异常
            }
            return false;
        }

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        public static bool EnableAutoStart()
        {
            try
            {
                string exePath = Application.ExecutablePath;
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    if (key != null)
                    {
                        key.SetValue(APP_NAME, $"\"{exePath}\"");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机自启动失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        /// <summary>
        /// 取消开机自启动
        /// </summary>
        public static bool DisableAutoStart()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(APP_NAME, false);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取消开机自启动失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        /// <summary>
        /// 切换开机自启动状态
        /// </summary>
        public static void ToggleAutoStart()
        {
            if (IsAutoStartEnabled())
            {
                DisableAutoStart();
            }
            else
            {
                EnableAutoStart();
            }
        }
    }
}
