using Avalonia.Controls;

namespace Eclipse;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        BeginReceive(Receiving, ip);
    }

    private void Enter(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox box)
        {
            if (e.KeyModifiers == KeyModifiers.Shift)
            {
                var index = box.CaretIndex;

                box.Text = box.Text?.Insert(index, "\n");

                box.CaretIndex = ++index;
            }

            else
                SendMessage(box, e);
        }
    }

    private void SendMessage(object sender, RoutedEventArgs e)
    {
        var text = this.GetControl<TextBox>("SendBox").Text;

        this.GetControl<TextBox>("SendBox").Text = string.Empty;

        if (!string.IsNullOrEmpty(text))
        {
            var content = MMPText + text;

            var block = new TextBlock()
            {
                Text = text
            };

            block.Classes.Add("Sent");

            try
            {
                // client.Connect(this.GetControl<TextBox>("SendIP").Text ?? string.Empty, Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));

                client.SendAsync(Encoding.UTF8.GetBytes(content), Encoding.UTF8.GetByteCount(content), this.GetControl<TextBox>("SendIP").Text ?? string.Empty, Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));

                this.GetControl<StackPanel>("MessageWindow").Children.Add(block);

                this.GetControl<ScrollViewer>("Scroll").ScrollToEnd();

                this.GetControl<TextBlock>("Exception").Text = string.Empty;
            }
            catch (FormatException)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }
            catch (OverflowException)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }
            catch (ArgumentOutOfRangeException)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }
            catch (Exception)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入正确的IP！";
            }
        }
    }

    private void PortChangeEvent(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox)
            textchanged = true;
    }

    private void PortChange(object sender, RoutedEventArgs e)
    {
        if (textchanged && sender is TextBox box)
        {
            try
            {
                switch (Convert.ToInt32(box.Text))
                {
                    case int input when input is <= IPEndPoint.MaxPort and >= IPEndPoint.MinPort:

                        ip.Port = input;

                        NewClient();

                        BeginReceive(Receiving, ip);

                        this.GetControl<TextBlock>("Exception").Text = string.Empty;

                        break;

                    default:

                        this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";

                        break;
                }
            }
            catch (FormatException)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }
            catch (OverflowException)
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }
            catch (SocketException exc)
            {
                switch (exc.Message)
                {
                    case "Permission denied":

                        this.GetControl<TextBlock>("Exception").Text = "该端口已被占用！";

                        break;

                    default:

                        this.GetControl<TextBlock>("Exception").Text = exc.Message;

                        break;
                }
            }
            catch (Exception exc)
            {
                this.GetControl<TextBlock>("Exception").Text = exc.Message;
            }
            finally
            {
                textchanged = false;

                ToReceive = false;
            }
        }
    }

    private void IPv6Change(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox box)
        {
            ipv6 = box.IsChecked ?? false;
        }
    }

    public void Sending(IAsyncResult ar)
    {
        var state = ar.AsyncState as UdpClient;

        if (state is not null)
        {
            var client = state;

            var result = client!.EndSend(ar);
        }
    }

    public void Receiving(IAsyncResult ar)
    {
        try
        {
            var state = ar.AsyncState as (UdpClient client, IPEndPoint ip)?;

            if (ToReceive && state is not null)
            {
                var client = state?.client;

                var ip = state?.ip;

                var result = client!.EndReceive(ar, ref ip);

                if (Encoding.UTF8.GetString(result[..18]) == "MlineMesProto_Text")
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var block = new TextBlock()
                        {
                            Text = Encoding.UTF8.GetString(result[18..])
                        };

                        block.Classes.Add("Received");
                        
                        this.GetControl<StackPanel>("MessageWindow").Children.Add(block);

                        this.GetControl<ScrollViewer>("Scroll").ScrollToEnd();
                    });

                client.BeginReceive(Receiving, (client, ip));
            }
            else
                ToReceive = true;
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = e.Message);
        }
    }
}