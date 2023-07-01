using Avalonia.Controls;

namespace Eclipse;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        client.BeginReceive(Receiving, (client, ip));
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
                client.Connect("127.0.0.1", Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));
            }
            catch
            {
                this.GetControl<TextBlock>("Exception").Text = "请输入0至65535之间的数！";
            }

            client.BeginSend(Encoding.UTF8.GetBytes(content), Encoding.UTF8.GetByteCount(content), Sending, client);

            this.GetControl<StackPanel>("MessageWindow").Children.Add(block);
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

                        client = new(ip);

                        client.BeginReceive(Receiving, (client, ip));

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
            }
        }
    }

    public void Sending(IAsyncResult ar)
    {
        var state = ar.AsyncState as UdpClient;

        if (state is not null)
        {
            var client = state;

            var result = client!.EndSend(ar);

            Console.WriteLine($"已发送{result}个字节");
        }
    }

    public void Receiving(IAsyncResult ar)
    {
        try
        {
            var state = ar.AsyncState as (UdpClient client, IPEndPoint ip)?;

            if (state is not null)
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
                    });

                Console.WriteLine(Encoding.UTF8.GetString(result[..18]));

                Console.WriteLine(Encoding.UTF8.GetString(result));

                Statics.client.BeginReceive(Receiving, (Statics.client, ip));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}