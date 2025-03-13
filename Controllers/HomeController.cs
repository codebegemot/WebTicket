using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebTicket.Models;

namespace WebTicket.Controllers;

public class HomeController : Controller
{
    //private readonly string _rootPath = Directory.GetCurrentDirectory();
    private readonly string _rootPath = "/Users/dmitrysergeev";


public IActionResult Index(string path = "")
{
    path = path ?? ""; // Защита от null
    string fullPath = Path.Combine(_rootPath, path);

    if (!Directory.Exists(fullPath))
        return NotFound("Директория не найдена");

    var directories = Directory.GetDirectories(fullPath)
        .Select(d => new DirectoryInfo(d).Name)
        .ToList(); // Преобразуем в список, чтобы избежать проблем с ViewBag

    var files = Directory.GetFiles(fullPath)
        .Select(f => new FileInfo(f))
        .ToList();

    ViewBag.CurrentPath = path;
    ViewBag.Directories = directories;
    ViewBag.Files = files;

    return View();
}


[HttpPost]
public IActionResult Upload(string path, IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("Файл не выбран");

    path = path ?? ""; // Защита от null
    string fullPath = Path.Combine(_rootPath, path, file.FileName);

    // Проверяем, существует ли папка, куда загружается файл
    string directoryPath = Path.GetDirectoryName(fullPath);
    if (!Directory.Exists(directoryPath))
    {
        Directory.CreateDirectory(directoryPath); // Создаём, если нет
    }

    using (var stream = new FileStream(fullPath, FileMode.Create))
    {
        file.CopyTo(stream);
    }

    return RedirectToAction("Index", new { path });
}


        [HttpPost]
        public IActionResult CreateFolder(string path, string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return BadRequest("Имя папки не указано");

            // Если path == null, заменяем на "" (чтобы не было null-ошибки)
            path = path ?? "";

            string fullPath = Path.Combine(_rootPath, path, folderName);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            return RedirectToAction("Index", new { path });
        }

public IActionResult Download(string path, string fileName)
{
    if (string.IsNullOrWhiteSpace(fileName))
        return BadRequest("Имя файла не указано");

    path = path ?? ""; // Если path == null, заменяем на пустую строку
    string fullPath = Path.Combine(_rootPath, path, fileName);

    if (!System.IO.File.Exists(fullPath))
        return NotFound("Файл не найден");

    byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
    return File(fileBytes, "application/octet-stream", fileName);
}
[HttpPost]
public IActionResult DeleteFolder(string path, string folderName)
{
    try
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return BadRequest("Имя папки не указано");

        path = path ?? ""; // Защита от null
        string fullPath = Path.Combine(_rootPath, path, folderName);

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        else
        {
            return NotFound($"Папка '{folderName}' не найдена в '{fullPath}'");
        }

        return RedirectToAction("Index", new { path });
    }
    catch (Exception ex)
    {
        return BadRequest($"Ошибка удаления папки: {ex.Message}");
    }
}

[HttpPost]
public IActionResult DeleteFile(string path, string fileName)
{
    try
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Имя файла не указано");

        path = path ?? ""; // Защита от null
        string fullPath = Path.Combine(_rootPath, path, fileName);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
        else
        {
            return NotFound($"Файл '{fileName}' не найден в '{fullPath}'");
        }

        return RedirectToAction("Index", new { path });
    }
    catch (Exception ex)
    {
        return BadRequest($"Ошибка удаления файла: {ex.Message}");
    }
}
public IActionResult ViewFile(string path, string fileName)
{
    try
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Имя файла не указано");

        path = path ?? ""; // Защита от null
        string fullPath = Path.Combine(_rootPath, path, fileName);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("Файл не найден");

        // Определяем, является ли файл текстовым
        string extension = Path.GetExtension(fullPath).ToLower();
        string[] allowedExtensions = { ".txt", ".log", ".json", ".md", ".html", ".css", ".js", ".xml", ".csv" };

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Файл нельзя просмотреть, поддерживаются только текстовые форматы.");

        // Читаем содержимое файла
        string content = System.IO.File.ReadAllText(fullPath);

        ViewBag.FileName = fileName;
        ViewBag.FilePath = path;
        ViewBag.FileContent = content;

        return View("View");
    }
    catch (Exception ex)
    {
        return BadRequest($"Ошибка при чтении файла: {ex.Message}");
    }
}

}
