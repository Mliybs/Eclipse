using Avalonia.Controls;

namespace Eclipse;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // EncryptProvider.AESEncrypt(new byte[] { 0xb2 }, EncryptProvider.CreateAesKey().Key, "0000000000000000");

        try
        {
            NewClient();

            IsNull = false;

            BeginReceive(Receiving, ip);
        }
        catch
        {
            this.GetControl<TextBlock>("Exception").Text = "接收端口被占用！请更换端口！";
        }
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
        if (IsNull)
        {
            this.GetControl<TextBlock>("Exception").Text = "接收端口被占用！请更换端口！";

            return;
        }

        var text = this.GetControl<TextBox>("SendBox").Text;

        this.GetControl<TextBox>("SendBox").Text = string.Empty;

        if (!string.IsNullOrEmpty(text))
        {
            // var key = this.GetControl<TextBox>("EnKey").Text;

            var content = new MMPBuilder(new(MMPIdentifier.MlineMesProto_Text)/* , key */)
                .Append(text);

            var block = new TextBlock()
            {
                Text = text
            };

            block.Classes.Add("Sent");

            try
            {
                // client.Connect(this.GetControl<TextBox>("SendIP").Text ?? string.Empty, Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));

                client.SendAsync(content, content, this.GetControl<TextBox>("SendIP").Text ?? string.Empty, Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));

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
            catch (ArgumentException exc)
            {
                this.GetControl<TextBlock>("Exception").Text = $"参数错误！：{exc.Message}";
            }
            catch (Exception exc)
            {
                this.GetControl<TextBlock>("Exception").Text = exc.Message;
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

                        ClientClose();

                        NewClient();

                        IsNull = false;

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

                        this.GetControl<TextBlock>("Exception").Text = "无法访问该端口！";

                        break;

                    case "Address already in use":

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

    private async void SendFile(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem)
        {
            using var file = (await StorageProvider.OpenFilePickerAsync(new()
            {
                Title = "打开文件",
                
                AllowMultiple = false
            })).SingleOrDefault();

            if (file is null)
                this.GetControl<TextBlock>("Exception").Text = "未选择文件！";

            else
            {
                var path = file.Path.ToString().Replace("file://", string.Empty).Replace("content://", string.Empty);

                var content = new MMPBuilder
                (
                    new
                    (
                        MMPIdentifier.MlineMesProto_File,
                        (
                            file.Name,
                            new FileInfo(path).Length
                        )
                    )
                ).Append(File.ReadAllBytes(path));

                client.Send(content, content, this.GetControl<TextBox>("SendIP").Text ?? string.Empty, Convert.ToInt32(this.GetControl<TextBox>("SendPort").Text));
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

    private void GenerateKey(object sender, RoutedEventArgs e)
    {
        this.GetControl<TextBox>("NewKey").Text = EncryptProvider.CreateAesKey().Key;
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

            // var key = Dispatcher.UIThread.Invoke(() => this.GetControl<TextBox>("DeKey").Text);

            if (state is not null)
            {
                var client = state?.client;

                var ip = state?.ip;

                var result = client!.EndReceive(ar, ref ip);

                // result = EncryptProvider.AESDecrypt(result, key, "0000000000000000");

                if (Encoding.UTF8.GetString(result[..14]) != "MlineMesProto_" || result.Length < 17)
                {
                    Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "接收到了无法解析的消息！");
                }
                else
                {
                    int size;

                    if (!int.TryParse(Encoding.UTF8.GetString(result[14..17]), out size) || result.Length < 17 + size)
                        Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "接收到了不合法的MMP消息！");

                    else
                    {
                        var header = JObject.Parse(Encoding.UTF8.GetString(result[17..(17 + size)]));

                        switch (header["Type"].Value<string>())
                        {
                            case "Text":

                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    var block = new TextBlock()
                                    {
                                        Text = Encoding.UTF8.GetString(result[(17 + size)..])
                                    };

                                    block.Classes.Add("Received");
                                    
                                    this.GetControl<StackPanel>("MessageWindow").Children.Add(block);

                                    this.GetControl<ScrollViewer>("Scroll").ScrollToEnd();
                                });

                                break;

                            case "File":

                                if (header["FileName"].Value<string>() is null || header["FileSize"].Value<long?>() is null)
                                {
                                    Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "接收到了不合法的MMP文件消息！");

                                    break;
                                }

                                Dispatcher.UIThread.Invoke(async () =>
                                {
                                    using var file = await StorageProvider.SaveFilePickerAsync(new()
                                    {
                                        SuggestedFileName = header["FileName"].Value<string>()
                                    });

                                    if (file is null)
                                        this.GetControl<TextBlock>("Exception").Text = "未选择路径，消息保存失败！";

                                    else
                                    {
                                        using var stream = await file.OpenWriteAsync();

                                        stream.Write(result[(17 + size)..]);
                                    }
                                });

                                break;

                            default:

                                Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "接收到了不合法的MMP消息！");

                                break;
                        }
                    }
                }

                // if (Encoding.UTF8.GetString(result[..18]) == "MlineMesProto_Text")
                //     Dispatcher.UIThread.Invoke(() =>
                //     {
                //         var block = new TextBlock()
                //         {
                //             Text = Encoding.UTF8.GetString(result[18..])
                //         };

                //         block.Classes.Add("Received");
                        
                //         this.GetControl<StackPanel>("MessageWindow").Children.Add(block);

                //         this.GetControl<ScrollViewer>("Scroll").ScrollToEnd();
                //     });

                client.BeginReceive(Receiving, (client, ip));
            }
        }
        catch (ObjectDisposedException)
        {
            // 在改端口的时候Close（Close内部有Dispose）会直接报错内容被释放了，依靠这一点确定端口更改成功，逆天吧（叉腰）
            Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "端口已更改！");
        }
        catch (InvalidOperationException)
        {
            Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "执行线程错误！");
        }
        catch (JsonReaderException)
        {
            Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = "接收到了不合法的MMP消息！");
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Invoke(() => this.GetControl<TextBlock>("Exception").Text = e.Message);
        }
    }
}