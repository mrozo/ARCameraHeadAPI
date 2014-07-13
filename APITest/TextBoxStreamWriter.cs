using System;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace APITest
{
    /// <summary>
    /// Class to capture and redirect stdout stream to a text box
    /// </summary>
    public class TextBoxStreamWriter : TextWriter
    {
        TextBlock _output = null;
        
        public TextBoxStreamWriter(TextBlock output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                base.Write(value);
                _output.Text += value.ToString();
            }
            else
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    Write(value);
                }));

        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
