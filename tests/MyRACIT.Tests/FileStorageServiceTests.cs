using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using MyRACIT.Services;

namespace MyRACIT.Tests;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly FileStorageService _svc;

    public FileStorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempRoot);

        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(_tempRoot);
        _svc = new FileStorageService(env.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static IFormFile MakeFile(string name, long length)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.Length).Returns(length);
        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    [Fact]
    public async Task SaveFileAsync_ValidFile_ReturnsRelativePath()
    {
        var result = await _svc.SaveFileAsync(MakeFile("doc.pdf", 512), "assignments");

        Assert.StartsWith("uploads/assignments/", result);
        Assert.Contains("doc.pdf", result);
    }

    [Fact]
    public async Task SaveFileAsync_NullFile_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _svc.SaveFileAsync(null!, "x"));
    }

    [Fact]
    public async Task SaveFileAsync_EmptyFile_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _svc.SaveFileAsync(MakeFile("empty.pdf", 0), "x"));
    }

    [Fact]
    public async Task SaveFileAsync_FileTooLarge_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _svc.SaveFileAsync(MakeFile("big.pdf", 11 * 1024 * 1024), "x"));
    }

    [Fact]
    public async Task GetFileAsync_NonExistingFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _svc.GetFileAsync("uploads/ghost.pdf"));
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_RemovesFile()
    {
        var uploadsDir = Path.Combine(_tempRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var filePath = Path.Combine(uploadsDir, "del.txt");
        await File.WriteAllTextAsync(filePath, "content");

        await _svc.DeleteFileAsync("uploads/del.txt");

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        var uploadsDir = Path.Combine(_tempRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);
        File.WriteAllText(Path.Combine(uploadsDir, "check.txt"), "hi");

        Assert.True(_svc.FileExists("uploads/check.txt"));
    }

    [Fact]
    public void FileExists_NonExistingFile_ReturnsFalse()
    {
        Assert.False(_svc.FileExists("uploads/doesnotexist.txt"));
    }

    [Fact]
    public void GetFileSize_ExistingFile_ReturnsPositiveSize()
    {
        var uploadsDir = Path.Combine(_tempRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);
        File.WriteAllText(Path.Combine(uploadsDir, "size.txt"), "hello world");

        Assert.True(_svc.GetFileSize("uploads/size.txt") > 0);
    }

    [Fact]
    public void GetFileSize_NonExistingFile_ReturnsMinusOne()
    {
        Assert.Equal(-1, _svc.GetFileSize("uploads/ghost.txt"));
    }

    [Theory]
    [InlineData("doc.pdf", "application/pdf")]
    [InlineData("doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("img.png", "image/png")]
    [InlineData("archive.zip", "application/zip")]
    [InlineData("file.xyz", "application/octet-stream")]
    public void GetContentType_Extension_ReturnsCorrectMime(string name, string expected)
    {
        Assert.Equal(expected, _svc.GetContentType(name));
    }
}
