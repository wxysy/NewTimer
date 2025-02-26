using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Office2024Core = Microsoft.Office.Core;
using Office2024PPT = Microsoft.Office.Interop.PowerPoint;

namespace PPTLib.Functions
{
    /// <summary>
    /// һ���ļ�ִ��һ��ʵ��
    /// </summary>
    public class PPTPlay : INotifyPropertyChanged
    {
        #region �������Է����仯ʱ�������¼�����ز���������������ǹ̶��ģ�ֱ���á���
        /*--------�����¼��������------------------------------------------------------------------------*/
        /// <summary>
        /// ���Է����仯ʱ�������¼�
        /// </summary>
        //[field: NonSerializedAttribute()]//��֤�¼�PropertyChanged�������л��ı�Ҫ�趨���¼��������л���
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// �����������ǣ�listeners�������Ѿ��仯
        /// </summary>
        /// <param name="propertyName">�仯���������ơ�
        /// ���ǿ�ѡ�������ܹ���CallerMemberName�Զ��ṩ��
        /// ��Ȼ��Ҳ�����ֶ�����</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /* ����ԭ��
         protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
         */

        // �����������Ŀǰ��֪����ɶ��
        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                NotifyPropertyChanged(propertyName);

                return true;
            }
        }

        /*------------------------------------------------------------------------------------------------*/
        #endregion

        /* �ο�
         * 1����C# ��̣��� winform ������ʵ�ֻõ�Ƭpptx���š�
         * https://blog.csdn.net/caifox/article/details/145231396
         * 2������ת��C#����PPT��
         * https://www.cnblogs.com/hhhh2010/p/4630738.html
         * 3���������wpf�����в���PPT����
         * https://www.cnblogs.com/tony-god/p/7650747.html
         */

        #region ���Ժ��ֶ�
        const string pptProgressName = "POWERPNT";//�������ƣ�POWERPNT.EXE
        Office2024PPT.Application? pptApp;
        Office2024PPT.Presentation? pptPresentation;
        Office2024PPT.SlideShowSettings? pptSlideShowSettings;
        Office2024PPT.SlideShowWindows? pptSlideShowWindows; //�������ã�pptApp.SlideShowWindows
        Office2024PPT.SlideShowView? pptSlideShowView; //�������ã�pptPresentation.SlideShowWindow.View

        private bool isPPTShowed = false;
        public bool IsPPTShowed
        {
            get { return isPPTShowed; }
            private set
            { isPPTShowed = value; NotifyPropertyChanged(); }
        }
        #endregion

        public PPTPlay()
        {
            PPTInitialize();
        }

        #region �������¼�
        public event EventHandler? PPTShowBegin;
        private void OnPPTShowBegin() => PPTShowBegin?.Invoke(this, EventArgs.Empty);

        public event EventHandler? PPTShowEnd;
        private void OnPPTShowEnd() => PPTShowEnd?.Invoke(this, EventArgs.Empty);

        private void PPTInitialize()
        {
            if (pptApp != null)  //��ֹ�����򿪶��PPT����
                return;

            pptApp = new Office2024PPT.Application(); //{ Visible = Office2024Core.MsoTriState.msoTrue, }; //����Ϊ�ɼ�(��Ҫ�裬�趨��������ͳ�����)
            pptApp.SlideShowBegin += PptApp_SlideShowBegin; //PPT��ʼ��ӳ
            pptApp.SlideShowEnd += PptApp_SlideShowEnd; //PPT������ӳ
        }
        private void PptApp_SlideShowBegin(Office2024PPT.SlideShowWindow Wn)
        {
            IsPPTShowed = true;
            OnPPTShowBegin();
        }
        private void PptApp_SlideShowEnd(Office2024PPT.Presentation Pres)
        {
            IsPPTShowed = false;
            OnPPTShowEnd();
        }

        public bool PPTOpen(string filepath)
        {           
            try
            {
                if (File.Exists(filepath) != true)
                    return false;

                //�����ѡ�������Application.Quit()����ʱ��"This operation cannot be performed in this event handler."�����һ�ֽ��������
                // https://blog.csdn.net/Neils03/article/details/9878575
                //�����߳��д򿪲�����PPTX�ļ�ʱ����������
                //��һ�δ�һ���ļ��������õ�Ƭʱһ��˳���������ظ�����ͬ���Ĳ������ͻᱨ��"Application (unknown member) : Invalid request.  This operation cannot be performed in this event handler."��COMException�쳣��
                //�Ⱥ�����˶���޸ĵ��õ�λ�ã���ʹ�����̺߳�ʹ�����̵߳��ã��������������е�Quit����ʱ������Google��Ҳû���ҵ���ȷ�Ĵ𰸣�ֻ��˵COM�����ǳ���STAģʽ�¡�
                //����ͻȻ������Open�����Ĳ���˵����
                //����WithWindow��ʼʹ�õ���Ĭ��ֵ����MsoTriState.msoTrue�������ڹرջõ�Ƭʱ�����������Quit����������PPT���ڡ�����WithWindow����ΪMsoTriState.msoFalse����رջõ�Ƭ�������´��ڣ������Ͳ��õ���Quit�����ˡ�
                //���ҵĳ���ر�ʱ�ٵ���Application.Quit()�����������ܱ�֤���ر���PPT����ͬʱҲ������Ϊ�򿪼���PPT�ļ��������ˡ�
                //����Ϊʲô���������¼��е���Quit����ʱ���������

                pptPresentation = pptApp!.Presentations.Open(filepath, Office2024Core.MsoTriState.msoTrue, Office2024Core.MsoTriState.msoFalse, Office2024Core.MsoTriState.msoFalse);
                pptSlideShowSettings = pptPresentation.SlideShowSettings;
                pptSlideShowSettings!.ShowType = Office2024PPT.PpSlideShowType.ppShowTypeSpeaker; //���ò���ģʽ
                pptSlideShowSettings.Run();

                return true;
            }
            catch (Exception)
            { return false; }
        }
        public bool PPTClose()
        {           
            try
            {
                if (pptPresentation != null)
                {
                    pptPresentation?.Close();
                    pptPresentation = null;
                }
                //if (pptApp != null) //pptApp���ܹرգ����PPTOpen������
                //{
                //    pptApp?.Quit();
                //    pptApp = null;
                //}
                //GC.Collect();//ǿ���ڴ���գ�Ҳ��֪����һ�����ڻ���û���ã�

                //Office2024�����⣺ǰ������ر�ֻ�ܹر��ĵ������ܹر�PPT����
                //���һ��C#�رս��̷���
                ClosePPTProgram(pptProgressName);
                return true;
            }
            catch (Exception)
            { return false; }
        }
        private static void ClosePPTProgram(string processName)
        {
            /* �ο�
             * 1����C#ʵ�ֹر�ĳ��ָ������
             * https://blog.csdn.net/laozhuxinlu/article/details/50422057
             * 2����C#���ֽ������̵ķ�����ϸ���ܡ�
             * https://blog.csdn.net/yl2isoft/article/details/54176740
             * https://www.cnblogs.com/zjoch/p/3654940.html
             * 3��PowerPoint����ʵ����Ϣ
             * string pptProgressName = "POWERPNT";//�������ƣ�POWERPNT.EXE
             * string pptMainWindowTitle = "PowerPoint";
             */

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    p.Kill();//̫���ͣ�û��Ҫ
                    //p.CloseMainWindow();//Process.CloseMainWindow��GUI��������Ѻý�����ʽ
                }
            }

            //����������
            //var startInfo = new ProcessStartInfo()
            //{
            //    ArgumentList = { "abc", "def" }, //���������б�MainWindow(string[]? startUpArgs)
            //    FileName = @".\WpfPluginManager.exe",
            //    WorkingDirectory = Directory.GetCurrentDirectory(),
            //    UseShellExecute = true,
            //};
            //var ps = Process.Start(startInfo);
        }
        #endregion

    }
}
