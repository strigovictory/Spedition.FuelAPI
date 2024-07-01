using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class FileBase : NotifyBase
{
    private readonly IWebHostEnvironment env;

    public FileBase(IWebHostEnvironment env)
        : base()
    {
        this.env = env;
    }

    protected IWebHostEnvironment Env => env;

    /// <summary>
    /// Путь к файлу-источнику (из формы, из шаблона).
    /// </summary>
    public string FilePathSource { get; set; } = string.Empty;

    /// <summary>
    /// Путь к файлу-назначению (отчеты).
    /// </summary>
    public string FilePathDestination { get; set; } = string.Empty;

    /// <summary>
    /// Путь к дирректории в, которой будет храниться файл-назначение.
    /// </summary>
    public string DirectoryPathDestination { get; set; } = string.Empty;

    /// <summary>
    /// Метод для удаления файлов.
    /// </summary>
    public void DeleteFiles(bool isSource = true, bool isDestination = true)
    {
        try
        {
            if (isSource && File.Exists(FilePathSource))
            {
                System.IO.File.Delete(FilePathSource);
            }

            if (isDestination && System.IO.File.Exists(FilePathDestination))
            {
                System.IO.File.Delete(FilePathDestination);
            }
        }
        catch (Exception exc)
        {
            NotifyMessage += exc.GetExeceptionMessages();
            exc.LogError(nameof(DeleteFiles), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для загрузки файла по указанному пути.
    /// </summary>
    /// <param Name="content">Контент, содержащий загруженный файл.</param>
    public bool UploadFile(UploadedContent content, CancellationToken token = default, string dirToSave = null)
    {
        bool result = default;

        if ((content?.Content?.Length ?? 0) == 0)
        {
            NotifyMessage += "Файл не был прикреплен!";
            return result;
        }

        DirectoryPathDestination = dirToSave == null ? Env.WebRootPath + "/src/tmp" : dirToSave;

        FilePathSource = Path.Combine(DirectoryPathDestination, content.FileName);

        // Копирование файла во временную папку
        try
        {
            using var writer = new BinaryWriter(File.Create(FilePathSource));
            writer.Write(content.Content);

            if (File.Exists(FilePathSource))
            {
                FilePathDestination = FilePathSource;
                return true;
            }
            else
            {
                NotifyMessage += "Файл не удалось загрузить!";
                FilePathDestination = string.Empty;
                return false;
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(UploadFile), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Вспомогательный метод для копирования шаблона в новый отчет.
    /// </summary>
    /// <returns>Результат - удачно/неудачно.</returns>
    public bool CopySourceToDestination(FilesExtension ext)
    {
        var result = false;
        try
        {
            // Сгенерировать случайное имя для файла, который будет временно хранить сгенерированный отчет
            var randomFileName = string.Concat(Path.GetRandomFileName(), ".", Enum.GetName(ext));

            // Задать путь к временному файлу, который будет хранить сгенерированный отчет
            FilePathDestination = Path.Combine(DirectoryPathDestination, randomFileName);

            // Скопировать шаблон в отчет
            File.Copy(FilePathSource, FilePathDestination, true);

            if (File.Exists(FilePathDestination))
            {
                result = true;
            }
            else
            {
                Log.Error($"Файл «{FilePathDestination ?? string.Empty}» не существует! ");
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(CopySourceToDestination), GetType().FullName);
            throw;
        }

        return result;
    }
}
