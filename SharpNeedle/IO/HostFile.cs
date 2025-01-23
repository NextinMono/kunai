﻿namespace SharpNeedle.IO;
using System.IO;

public class HostFile : IFile
{
    private DateTime mLastModified;
    public IDirectory Parent
        => HostDirectory.FromPath(Path.GetDirectoryName(Path.IsPathRooted(FullPath.AsSpan())
            ? FullPath
            : Path.TrimEndingDirectorySeparator(FullPath)));

    string IFile.Path => FullPath;
    public string Name { get; set; }
    public long Length { get; }
    public string FullPath { get; }

    public DateTime LastModified
    {
        get => mLastModified;
        set
        {
            mLastModified = value;
            File.SetLastWriteTime(FullPath, value);
        }
    }

    protected FileAccess Access { get; set; }
    protected FileStream BaseStream { get; set; }
    
    public HostFile(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new FileNotFoundException(fullPath);

        var info = new FileInfo(fullPath);
        Name = info.Name;
        FullPath = fullPath;
        Length = info.Length;
        LastModified = info.LastWriteTime;
    }

    public static HostFile Create(string fullPath)
    {
        File.WriteAllBytes(fullPath, Array.Empty<byte>());
        return new HostFile(fullPath);
    }

    public static HostFile Open(string fullPath)
    {
        return !File.Exists(fullPath) ? null : new HostFile(fullPath);
    }

    public Stream Open(FileAccess access = FileAccess.Read)
    {
        if (CheckDisposed())
            BaseStream = null;

        Access = access;

        if (BaseStream != null && Access == access)
            return BaseStream;
            
        if (BaseStream != null && Access == access)
        {
            Dispose();
            return Open(access);
        }

        BaseStream = new FileStream(FullPath, FileHelper.FileAccessToMode(access), access, FileShare.ReadWrite);
        return BaseStream;
    }

    public bool CheckDisposed()
    {
        if (BaseStream == null)
            return false;

        try
        {
            _ = BaseStream.Length;
            return false;
        }
        catch
        {
            return true;
        }
    }

    public override string ToString()
        => Name;

    public override bool Equals(object obj)
        => obj is HostFile file && Equals(file);

    protected bool Equals(HostFile other)
        => FullPath == other.FullPath;

    public override int GetHashCode()
    {
        return HashCode.Combine(Length, FullPath);
    }

    public void Dispose()
    {
        BaseStream?.Dispose();
        BaseStream = null;
    }
}