using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ContentPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SWF_BASE_URL = "file://127.0.0.1";
        private const string CONTENT_CONFIG = "content.conf";
        private DateTime contentConfigWriteTime;
        private List<Content> listContent;
        private int selectedContentIdx; //selected content index

        private DispatcherTimer timer;
        private int timerTick;
        private int playDuration;

        public MainWindow()
        {
            InitializeComponent();

            InitContentConfig();
            LoadContent();

            InitTimer();
            StartTimer();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
        }

        private void LoadContent()
        {
            if (listContent.Count > 0)
            {
                int index = selectedContentIdx % listContent.Count;
                string relativePath = listContent[index].FilePath;

                if (relativePath.EndsWith(".swf")) LoadFlashContent(relativePath);
                else LoadMediaContent(relativePath);
            }
        }

        private void LoadNextContent()
        {
            selectedContentIdx++;
            LoadContent();
            StartTimer();
        }

        private void StartTimer()
        {
            timerTick = 0;
            int index = selectedContentIdx % listContent.Count;
            playDuration = listContent[index].PlayDuration;
            timer.Start();
        }

        #region Content Player Functions

        private void LoadMediaContent(string relativePath)
        {
            string mediaFile = AppDomain.CurrentDomain.BaseDirectory + relativePath;
            media.Source = new Uri(mediaFile);
            media.Play();
            SwitchToSWFPlayer(false);
        }

        private void LoadFlashContent(string relativePath)
        {
            media.Stop();
            string baseURL = AppDomain.CurrentDomain.BaseDirectory;
            string drive = baseURL[0].ToString().ToLower() + "$";
            string rootD = baseURL.Substring(3, baseURL.Length - 4);
            string swfPath = String.Format("{0}/{1}/{2}/{3}", SWF_BASE_URL, drive, rootD, relativePath);

            swfPlayer.Source = new Uri(swfPath);
            SwitchToSWFPlayer(true);
        }

        private void SwitchToSWFPlayer(bool status)
        {
            swfPlayer.Visibility = status ? Visibility.Visible : Visibility.Collapsed;
            media.Visibility = status ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Content Configuration Functions

        private void InitContentConfig()
        {
            contentConfigWriteTime = File.GetLastWriteTime(CONTENT_CONFIG);
            var text = File.ReadAllText(CONTENT_CONFIG);
            listContent = JsonConvert.DeserializeObject<List<Content>>(text);
            selectedContentIdx = 0;
        }

        private void CheckContentConfig()
        {
            var lastModified = File.GetLastWriteTime(CONTENT_CONFIG);
            if (lastModified != contentConfigWriteTime)
            {
                ReloadContentConfig();
            }
        }

        private void ReloadContentConfig()
        {
            InitContentConfig();
            LoadContent();
            StartTimer();
        }

        #endregion

        #region UI Event

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timerTick >= playDuration)
            {
                timer.Stop();
                LoadNextContent();
            }
            timerTick++;
            CheckContentConfig();
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            LoadNextContent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: Close(); break;
                case Key.Right: LoadNextContent(); break;
                default:
                    break;
            }
        }

        #endregion

    }
}
