# Monster Siren - UWP

一个基于 UWP 的塞壬唱片第三方客户端

## 最低支持平台
Windows 10 1703 (15063)

## 构建需求
- Visual Studio 2022 及以上
- .NET 8 SDK（为了使用最新的 C# 语言版本）
- Windows 10 SDK (至少为 16299，因为需要生成 ARM64 版本的程序)
    - 需安装 Windows SDK for UWP Managed Apps

## 贡献者须知
为了兼容 ARM64 架构及使用 .NET Standard 2.0 库，我们将最低版本设置为了 Windows 10 1709 (Build 16299)。

但是，为了兼容 Windows 10 Mobile 设备，我们不应该在不添加兼容性检测的情况下，使用任何不支持 Windows 10 1703 (Build 15063) 的 API。

另外，由于本项目使用了 .NET Standard 2.0 库，因此在部署到 Windows 10 Mobile 设备时，应使用 `Release` 配置来进行部署或安装。

## 致谢
本项目的 API 实现参考了 [QingXia-Ela/MonsterSirenApi](https://github.com/QingXia-Ela/MonsterSirenApi) 的文档，在此向其表示感谢。

## 许可
MIT 许可证