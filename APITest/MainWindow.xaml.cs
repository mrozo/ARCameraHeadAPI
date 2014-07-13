using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using dbg = DebugHelper.DbgConsole;

namespace APITest
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ARCameraHeadAPI.COMAPI _cameraHead = null;
        COMSimulator _simulator = null;
        TextBoxStreamWriter _consoleWriter = null;
        Thread _uiUpdater;
        public MainWindow()
        {
            InitializeComponent();
            _consoleWriter = new TextBoxStreamWriter(consoleOutput);
            Console.SetOut(_consoleWriter);

            Closing += MainWindow_Closing;

            foreach (string portName in ARCameraHeadAPI.COMAPI.ListComPorts())
                comPortSelect.Items.Add(portName);

            foreach (string portName in COMSimulator.ListComPorts())
                simulatorComPortSelect.Items.Add(portName);

            connectionButton.IsEnabled = false;
            connectionButton.Content = "Connect";

            simulatorConnect.IsEnabled = false;
            connectionButton.Content = "Connect";

            
            _uiUpdater = new Thread(new ThreadStart(uiUpdateMethod));
            _uiUpdater.Name = "MainWindow uiUpdater";
            _uiUpdater.Start();

#if DEBUG

            //automatically select and use given ports
            String port1Name = "COM6";
            String port2Name = "COM5";

            DebugHelper.DbgConsole.Message("Starting in automatic mode");
            Thread _dbgThr = new Thread(new ThreadStart(() => {

                int port1Index = -1;
                int port2Index = -1;
                Thread.Sleep(500);
                Application.Current.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        for (int i = 0; i < comPortSelect.Items.Count; i++)
                            port1Index = ((string)comPortSelect.Items[i]) == port1Name ? i : port1Index;
                        for (int i = 0; i < simulatorComPortSelect.Items.Count; i++)
                            port2Index = ((string)simulatorComPortSelect.Items[i]) == port2Name ? i : port2Index;
                        comPortSelect.SelectedIndex = port1Index;
                        simulatorComPortSelect.SelectedIndex = port2Index;
                        Thread.Sleep(100);

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            simulatorConnectionButton_Click(this, new RoutedEventArgs());
                        }));
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                           Thread.Sleep(1000);
                           connectionButton_Click(this, new RoutedEventArgs());
                        }));
                    })
                    );

            }));
            _dbgThr.Start();


#endif
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            comPortSelect.IsEnabled = false;
            command0.IsEnabled = false;
            command1.IsEnabled = false;
            command2.IsEnabled = false;
            simulatorComPortSelect.IsEnabled = false;
            simulatorConnect.IsEnabled = false;

            if (null != _cameraHead)
                _cameraHead.Disconnect(() => _cameraHead=null );

            if (null != _simulator)
                _simulator.Disconnect(() => _simulator=null );

            while (null != _simulator || null != _cameraHead)
                Thread.Sleep(50);
        }

        private void connectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (null==_cameraHead)
            {
                _cameraHead = new ARCameraHeadAPI.COMAPI(comPortSelect.SelectedItem.ToString());
                comPortSelect.IsEnabled = false;
                connectionButton.Content = "Disconnect";
                command0.IsEnabled = true;
                command1.IsEnabled = true;
                command2.IsEnabled = true;
            }
            else
            {
                connectionButton.IsEnabled = false;
                _cameraHead.Disconnect(() =>
                {
                    comPortSelect.IsEnabled = true;
                    connectionButton.Content = "Connect";
                    command0.IsEnabled = false;
                    command1.IsEnabled = false;
                    command2.IsEnabled = false;
                    connectionButton.IsEnabled = true;
                });
            }
            
        }

        private void simulatorConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == _simulator)
            {
                _simulator = new COMSimulator(simulatorComPortSelect.SelectedItem.ToString());
                simulatorComPortSelect.IsEnabled = false;
                simulatorConnect.Content = "Disconnect";
            }
            else
            {
                _simulator.Disconnect(() => _simulator=null);
                simulatorComPortSelect.IsEnabled = true;
                simulatorConnect.Content = "Connect";
            }

        }

        private void comPortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            connectionButton.IsEnabled = true;
        }

        private void simulatorComPortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            simulatorConnect.IsEnabled = true;
        }

        public void uiUpdateMethod()
        {
            try
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        if (_cameraHead != null)
                        {
                            senderHorizontalAngle.Content = _cameraHead.horizontalAngle.ToString("F6");
                            senderVerticalAngle.Content = _cameraHead.verticalAngle.ToString("F6");
                            senderDownloaded.Content = _cameraHead.receivedPackets.ToString();
                            senderUploaded.Content = _cameraHead.transmittedPackets.ToString();
                        }
                        if (_simulator != null)
                        {
                            simulatorHorizontalAngle.Content = _simulator.horizontalAngle.ToString("F6");
                            simulatorVerticalAngle.Content = _simulator.verticalAngle.ToString("F6");
                            simulatorDownloaded.Content = _simulator.receivedPackets.ToString();
                            simulatorUploaded.Content = _simulator.transmittedPackets.ToString();
                        }
                    }));
                    Thread.Sleep(100);
                }
            }
            catch (System.NullReferenceException e) //wyjatek wyrzucany podczas wylaczania aplikacji ( Application.Current == null )
            {

            }

        }

        private void horizontalAngleInc_Click(object sender, RoutedEventArgs e)
        {
            dbg.Message("");
            _cameraHead.horizontalAngle = _cameraHead.transmitterData.horizontalAngle.value + .1;
        }

        private void horizontalAngleDec_Click(object sender, RoutedEventArgs e)
        {
            dbg.Message("");
            _cameraHead.horizontalAngle = _cameraHead.transmitterData.horizontalAngle.value - .1;
        }

        private void verticalAngleInc_Click(object sender, RoutedEventArgs e)
        {
            dbg.Message("");
            _cameraHead.verticalAngle = _cameraHead.transmitterData.verticalAngle.value + .1;
        }

        private void verticalAngleDec_Click(object sender, RoutedEventArgs e)
        {
            dbg.Message("");
            _cameraHead.verticalAngle = _cameraHead.transmitterData.verticalAngle.value - .1;
        }

    }
}
