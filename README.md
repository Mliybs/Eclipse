# Eclipse
.NET实现点对点（P2P）聊天软件

当前版本：v0.2，具体内容请查看更新日志：[Update](/Update.md)

# 鸣谢
[NETCore.Encrypt](https://github.com/myloveCc/NETCore.Encrypt)，采用MIT协议
[nanoid-net](https://github.com/codeyu/nanoid-net)，采用MIT协议

# MlinetlesMessageProtocol（MMP）
MlinetlesMessageProtocol（MMP）是一种基于UDP协议的应用层协议

## MMP0.2
测试使用的MMP协议，由以下两部分组成：

1. MMP请求头：为MMP类型标识符（MMP Identifier），大小为18个字节

2. MMP数据：为UTF8编码的字节内容

# MMP类型标识符（MMP Identifier）
MMP类型标识符（MMP Identifier）用来指示MMP数据的类型

## MMP I0.2
与MMP0.2配合使用的MMP类型标识符，有：

1. MlineMesProto_001，表示内容为UTF8文本内容